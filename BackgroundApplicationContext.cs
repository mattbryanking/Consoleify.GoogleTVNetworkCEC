using System;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace Consoleify.GoogleTVNetworkCEC
{
    public class BackgroundApplicationContext : ApplicationContext
    {
        private Timer pollTimer;
        private DateTime lastCommandTime = DateTime.MinValue;
        private readonly TimeSpan commandCooldown = TimeSpan.FromSeconds(5);

        private int lastGamepadPacket = 0;

        public BackgroundApplicationContext(bool isSilent)
        {
            if (!isSilent)
            {
                new SettingsForm().Show();
            }

            InitializeAdb();

            pollTimer = new Timer { Interval = 1000 };
            pollTimer.Tick += PollInput;
            pollTimer.Start();    
        }

        private async void InitializeAdb()
        {
            await AdbManager.EnsureAdbInstalledAsync();
            System.Diagnostics.Debug.WriteLine("ADB Environment Ready.");
        }

        private async void PollInput(object sender, EventArgs e)
        {
            bool inputDetected = false;

            for (int i = 8; i < 255; i++)
            {

                if ((GetAsyncKeyState(i) & 0x8001) != 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[TRIGGER] Keyboard key pressed (Code: {i})");
                    inputDetected = true;
                    break; 
                }
            }

            XINPUT_STATE xState = new XINPUT_STATE();
            if (XInputGetState(0, ref xState) == 0)
            {
                if (lastGamepadPacket != 0 && xState.dwPacketNumber != lastGamepadPacket)
                {
                    System.Diagnostics.Debug.WriteLine($"[TRIGGER] Gamepad caused input! (Packet changed)");
                    inputDetected = true;
                }
                lastGamepadPacket = (int)xState.dwPacketNumber;
            }

            System.Diagnostics.Debug.WriteLine($"About to check! Time: {DateTime.Now.ToString("HH:mm:ss")}");
            if (inputDetected && (DateTime.Now - lastCommandTime) > commandCooldown)
            {
                System.Diagnostics.Debug.WriteLine("Keyboard/Gamepad input detected, sending wake command to TV...");
                lastCommandTime = DateTime.Now;
                await AdbManager.WakeAndSwitchTV();
            }
        }

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [StructLayout(LayoutKind.Sequential)]
        struct XINPUT_STATE
        {
            public uint dwPacketNumber;
            public XINPUT_GAMEPAD Gamepad;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct XINPUT_GAMEPAD
        {
            public ushort wButtons;
            public byte bLeftTrigger, bRightTrigger;
            public short sThumbLX, sThumbLY, sThumbRX, sThumbRY;
        }
        [DllImport("xinput1_4.dll")]
        private static extern int XInputGetState(int dwUserIndex, ref XINPUT_STATE pState);
    }
}