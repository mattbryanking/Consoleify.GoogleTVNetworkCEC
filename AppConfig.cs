using System;
using System.IO;
using System.Text.Json;

namespace Consoleify.GoogleTVNetworkCEC
{
    public class AppConfig
    {
        public string TvIpAddress { get; set; } = "";
        public string HdmiCommand { get; set; } = "";

        private static readonly string ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Consoleify");
        private static readonly string ConfigPath = Path.Combine(ConfigDirectory, "NetworkCEC-settings.json");

        public static AppConfig Load()
        {
            if (!File.Exists(ConfigPath))
            {
                var defaultConfig = new AppConfig();
                defaultConfig.Save();
                return defaultConfig;
            }

            try
            {
                string json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch (JsonException ex)
            {
                Logger.Error($"Settings file is corrupted, using defaults: {ex.Message}");
                return new AppConfig();
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(ConfigDirectory);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(ConfigPath, json);
        }
    }
}
