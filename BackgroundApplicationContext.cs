using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;
using SDL3; 

namespace Consoleify.GoogleTVNetworkCEC
{
    public class BackgroundApplicationContext : ApplicationContext
    {
        private readonly Timer _pollTimer;
        private DateTime _lastCommandTime = DateTime.MinValue;
        private readonly TimeSpan _commandCooldown = TimeSpan.FromSeconds(5);
        private bool _isPolling = false;
        private readonly Task _adbInitTask;

        private bool _sdlInitialized = false;

        public BackgroundApplicationContext(bool isSilent)
        {
            if (!isSilent)
            {
                new SettingsForm().Show();
            }

            _adbInitTask = AdbManager.EnsureAdbInstalledAsync();

            // sdl3 covers xinput, directinput, and steam controllers
            if (SDL.Init(SDL.InitFlags.Gamepad))
            {
                _sdlInitialized = true;
            }
            else
            {
                Logger.Error($"Failed to initialize SDL3: {SDL.GetError()}");
            }

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

                // sdl3 can't do global keyboard hooking without a focused window, fall back to win32
                for (int i = 8; i < 255; i++)
                {
                    if ((GetAsyncKeyState(i) & 0x8001) != 0)
                    {
                        Logger.Info($"[TRIGGER] Keyboard key pressed (Code: {i})");
                        inputDetected = true;
                        break;
                    }
                }

                // drain the sdl event queue
                if (_sdlInitialized)
                {
                    while (SDL.PollEvent(out var sdlEvent))
                    {
                        var eventType = (SDL.EventType)sdlEvent.Type;

                        if (eventType == SDL.EventType.GamepadAdded)
                        {
                            SDL.OpenGamepad(sdlEvent.GDevice.Which);
                            Logger.Info("[SDL3] Gamepad connected (Ignored for wake).");
                        }
                        else if (eventType == SDL.EventType.GamepadRemoved)
                        {
                            var gamepad = SDL.GetGamepadFromID(sdlEvent.GDevice.Which);
                            if (gamepad != IntPtr.Zero)
                            {
                                SDL.CloseGamepad(gamepad);
                            }
                            Logger.Info("[SDL3] Gamepad disconnected (Ignored for wake).");
                        }
                        else if (eventType == SDL.EventType.GamepadButtonDown)
                        {
                            Logger.Info("[TRIGGER] Gamepad button pressed.");
                            inputDetected = true;
                        }
                        else if (eventType == SDL.EventType.GamepadAxisMotion)
                        {
                            // large deadzone to avoid stick drift waking the TV
                            if (Math.Abs(sdlEvent.GAxis.Value) > 15000)
                            {
                                Logger.Info("[TRIGGER] Gamepad axis movement detected.");
                                inputDetected = true;
                            }
                        }
                    }
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

            if (_sdlInitialized)
            {
                SDL.Quit();
            }

            base.Dispose(disposing);
        }

        // global keyboard - sdl3 requires a focused window for this
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
    }
}