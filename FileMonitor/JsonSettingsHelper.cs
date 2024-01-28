using System;
using System.IO;
using System.Text.Json;

namespace FileMonitor
{
    internal static class JsonSettingsHelper
    {
        /// <summary>
        /// Read from and write to the "overwriteUpdatedFiles" value from Settings.json. If set to false, the program
        /// will make copies of updated files whenever they are backed up. If set to true, the program will overwrite
        /// the previously copied file.
        /// </summary>
        public static bool OverwriteUpdatedFiles
        {
            get
            {
                Settings? settings = GetJsonSettings();
                return settings.overwriteUpdatedFiles;
            }

            set
            {
                Settings? settings = GetJsonSettings();
                settings.overwriteUpdatedFiles = value;
                SaveSettings(settings);
            }
        }

        /// <summary>
        /// Read from and write to the "includeAllSubFolders" value from Settings.json. If set to false, the program
        /// will only monitor files contained within the monitored folder. If set to true, the program will monitor all
        /// files an sub-files contained within the monitored folder. 
        /// </summary>
        public static bool IncludeAllSubFolders
        {
            get
            {
                Settings? settings = GetJsonSettings();
                return settings.includeAllSubfolders;
            }

            set
            {
                Settings? settings = GetJsonSettings();
                settings.includeAllSubfolders = value;
                SaveSettings(settings);
            }
        }

        private static Settings? GetJsonSettings()
        {
            string text = File.ReadAllText($"{Environment.CurrentDirectory}\\Settings.json");
            return JsonSerializer.Deserialize<Settings>(text);
        }

        private static void SaveSettings(Settings settings)
        {
            string data = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText($"{Environment.CurrentDirectory}\\Settings.json", data);
        }

        /// <summary>
        /// An object representing the data fields from Settings.json.
        /// </summary>
        public class Settings
        {
            public bool overwriteUpdatedFiles { get; set; }
            public bool includeAllSubfolders { get; set; }
        }
    }
}
