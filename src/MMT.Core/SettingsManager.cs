using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace MMT.Core
{
    public class SettingsManager<T> where T : class
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions() { WriteIndented = true, AllowTrailingCommas = true, IgnoreReadOnlyProperties = true };
        private bool _isLoading = false;

        public SettingsManager(string fileName)
        {
            _filePath = GetLocalFilePath(fileName);
        }

        private string GetLocalFilePath(string fileName)
        {
            string folder = Path.Combine(StaticResources.LocalAppData, StaticResources.AppDataFolderName);
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, fileName);
        }

        public T? LoadSettings()
        {
            _isLoading = true;
            T? settings = File.Exists(_filePath) ?
                JsonSerializer.Deserialize<T>(File.ReadAllText(_filePath), _serializerOptions) :
                null;
            _isLoading = false;
            return settings;
        }



        public void SaveSettings(T settings)
        {
            if (!_isLoading)
            {
                string json = JsonSerializer.Serialize(settings, _serializerOptions);
                File.WriteAllText(_filePath, json);
            }
        }
    }
}
