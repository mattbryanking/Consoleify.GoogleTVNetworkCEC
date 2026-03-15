using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace Consoleify.GoogleTVNetworkCEC
{
    public static class AdbManager
    {
        private static readonly string AdbDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "platform-tools");
        private static readonly string AdbExePath = Path.Combine(AdbDirPath, "adb.exe");
        private const string AdbDownloadUrl = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip";

        public static async Task EnsureAdbInstalledAsync()
        {
            if (File.Exists(AdbExePath)) return;

            string zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "platform-tools.zip");

            using (HttpClient client = new HttpClient())
            {
                byte[] zipBytes = await client.GetByteArrayAsync(AdbDownloadUrl);
                File.WriteAllBytes(zipPath, zipBytes);
            }

            if (Directory.Exists(AdbDirPath)) Directory.Delete(AdbDirPath, true);
            ZipFile.ExtractToDirectory(zipPath, AppDomain.CurrentDomain.BaseDirectory);
            File.Delete(zipPath);
        }

        public static async Task WakeAndSwitchTV()
        {
            AppConfig config = AppConfig.Load();

            string tvIp = config.TvIpAddress;
            string hdmiCommand = config.HdmiCommand;

            if (string.IsNullOrWhiteSpace(tvIp))
            {
                System.Diagnostics.Debug.WriteLine("ERROR: TV IP Address is not set in settings.json.");
                return;
            }

            if (string.IsNullOrWhiteSpace(hdmiCommand))
            {
                System.Diagnostics.Debug.WriteLine("ERROR: HDMI command is not set in settings.json.");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Connecting to TV at {tvIp}...");

            RunProcess(AdbExePath, $"connect {tvIp}", true);

            RunProcess(AdbExePath, "shell input keyevent KEYCODE_WAKEUP", false);
            RunProcess(AdbExePath, $"shell {hdmiCommand}", false);
        }

        private static void RunProcess(string fileName, string args, bool waitForExit)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                using (Process process = Process.Start(psi))
                {
                    if (waitForExit && process != null)
                    {
                        process.WaitForExit(3000);
                    }
                }
            }
            catch { System.Diagnostics.Debug.WriteLine($"Failed to run: {args}"); }
        }
    }
}