using System;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace Consoleify.GoogleTVNetworkCEC
{
    public class BackgroundApplicationContext : ApplicationContext
    {
        private readonly Timer _pollTimer;
        private DateTime _lastCommandTime = DateTime.MinValue;
        private readonly TimeSpan _commandCooldown = TimeSpan.FromSeconds(5);
        private int _lastGamepadPacket = 0;
        private bool _isPolling = false;
        private readonly Task _adbInitTask;

        public BackgroundApplicationContext(bool isSilent)
        {
            if (!isSilent)
            {
                new SettingsForm().Show();
            }

            _adbInitTask = AdbManager.EnsureAdbInstalledAsync();

            _pollTimer = new Timer { Interval = 1000 };
            _pollTimer.Tick += PollInput;
            _pollTimer.Start();
        }

        private async void PollInput(object? sender, EventArgs e)
        {
            if (_isPolling) return;
            _isPolling = true;

            try
            {
                try { await _adbInitTask; }
                catch (Exception ex)
                {
                    Logger.Error($"ADB initialization failed, skipping poll: {ex.Message}");
                    return;
                }

                bool inputDetected = false;

                for (int i = 8; i < 255; i++)
                {
                    if ((GetAsyncKeyState(i) & 0x8001) != 0)
                    {
                        Logger.Info($"[TRIGGER] Keyboard key pressed (Code: {i})");
                        inputDetected = true;
                        break;
                    }
                }

                XINPUT_STATE xState = new XINPUT_STATE();
                if (XInputGetState(0, ref xState) == 0)
                {
                    if (_lastGamepadPacket != 0 && xState.dwPacketNumber != _lastGamepadPacket)
                    {
                        Logger.Info("[TRIGGER] Gamepad input detected.");
                        inputDetected = true;
                    }
                    _lastGamepadPacket = (int)xState.dwPacketNumber;
                }

                if (inputDetected && (DateTime.UtcNow - _lastCommandTime) > _commandCooldown)
                {
                    Logger.Info("Input detected, sending wake command to TV...");
                    _lastCommandTime = DateTime.UtcNow;
                    await AdbManager.WakeAndSwitchTV();
                }
            }
            finally
            {
                _isPolling = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pollTimer.Stop();
                _pollTimer.Dispose();
            }
            base.Dispose(disposing);
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
