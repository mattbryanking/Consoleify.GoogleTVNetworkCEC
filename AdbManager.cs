using System.Diagnostics;
using System.IO.Compression;

namespace Consoleify.GoogleTVNetworkCEC
{
    public static class AdbManager
    {
        private static readonly string AdbDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "platform-tools");
        private static readonly string AdbExePath = Path.Combine(AdbDirPath, "adb.exe");
        private const string AdbDownloadUrl = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip";
        private static string? _connectedTvIp = null;

        public static async Task EnsureAdbInstalledAsync()
        {
            if (File.Exists(AdbExePath)) return;

            string zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "platform-tools.zip");
            Logger.Info("Downloading ADB platform-tools...");

            using (var client = new HttpClient { Timeout = TimeSpan.FromMinutes(2) })
            {
                byte[] zipBytes = await client.GetByteArrayAsync(AdbDownloadUrl);
                await File.WriteAllBytesAsync(zipPath, zipBytes);
            }

            if (Directory.Exists(AdbDirPath)) Directory.Delete(AdbDirPath, true);
            ZipFile.ExtractToDirectory(zipPath, AppDomain.CurrentDomain.BaseDirectory);
            File.Delete(zipPath);
            Logger.Success("ADB platform-tools installed.");
        }

        public static async Task WakeAndSwitchTV()
        {
            AppConfig config = AppConfig.Load();
            string tvIp = config.TvIpAddress;
            string hdmiCommand = config.HdmiCommand;

            if (string.IsNullOrWhiteSpace(tvIp))
            {
                Logger.Error("TV IP Address is not set in settings.");
                return;
            }

            if (string.IsNullOrWhiteSpace(hdmiCommand))
            {
                Logger.Error("HDMI command is not set in settings.");
                return;
            }

            if (_connectedTvIp != tvIp)
            {
                Logger.Info($"Connecting to TV at {tvIp}...");
                bool connected = await RunProcessAsync(AdbExePath, $"connect {tvIp}");
                if (!connected)
                {
                    Logger.Error($"Failed to connect to TV at {tvIp}.");
                    return;
                }
                _connectedTvIp = tvIp;
            }

            bool woke = await RunProcessAsync(AdbExePath, "shell input keyevent KEYCODE_WAKEUP");
            bool switched = await RunProcessAsync(AdbExePath, $"shell {hdmiCommand}");

            if (!woke || !switched)
            {
                Logger.Warning("Wake or HDMI switch command failed. Will reconnect on next attempt.");
                _connectedTvIp = null;
            }
            else
            {
                Logger.Success("TV wake and HDMI switch sent successfully.");
            }
        }

        private static async Task<bool> RunProcessAsync(string fileName, string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                using var process = Process.Start(psi);
                if (process == null) return false;

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await process.WaitForExitAsync(cts.Token);
                return process.ExitCode == 0;
            }
            catch (OperationCanceledException)
            {
                Logger.Warning($"Process timed out: {args}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to run process '{args}': {ex.Message}");
                return false;
            }
        }
    }
}
