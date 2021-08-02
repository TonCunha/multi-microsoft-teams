using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MMT.Core
{
    public class ProfileManager : INotifyPropertyChanged
    {
        private IList<string> _profiles = new List<string>();
        public IList<string> Profiles => _profiles;

        private static readonly string _customProfilesPath = Path.Combine(StaticResources.LocalAppData, StaticResources.CustomProfiles);

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyRaised(string propertyname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

        public ProfileManager()
        {
            UpdateProfiles();
        }

        private void UpdateProfiles()
        {
            if (!Directory.Exists(_customProfilesPath))
                Directory.CreateDirectory(_customProfilesPath);

            _profiles = Directory.GetDirectories(_customProfilesPath)
                .Select(path => new DirectoryInfo(path).Name)
                .Where(name => !String.IsNullOrWhiteSpace(name))
                .Select(profile => IsDisabled(profile) ? $"[Disabled] {profile}" : profile)
                .ToList();
            _profiles.Insert(0, "[Default]");

            OnPropertyRaised(nameof(Profiles));
        }

        private static string GetProfilePath (string profileName) => Path.Combine(_customProfilesPath, profileName);

        public void Save(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                throw new ArgumentException("Profile name is required.");

            if (Profiles.Any(p => String.Equals(p, profileName, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("This profile already exists.");

            // Create UserProfile-Folder-Structure
            var path = GetProfilePath(profileName);
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, "Desktop"));
            Directory.CreateDirectory(Path.Combine(path, "Downloads"));

            UpdateProfiles();
        }

        public void Delete(string profileName)
        {
            if (!IsDefault(profileName))
            {
                string path = GetProfilePath(profileName);

                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }

            UpdateProfiles();
        }

        private static string GetDisabledFilePath(string profileName) => Path.Combine(GetProfilePath(profileName), "MMT.disabled");

        private static bool IsDisabled(string profileName) => File.Exists(GetDisabledFilePath(profileName));

        public void Enable(string profileName)
        {
            if (IsDisabled(profileName))
            {
                File.Delete(GetDisabledFilePath(profileName));
                UpdateProfiles();
            }
        }

        public void Disable(string profileName)
        {
            if (!IsDisabled(profileName))
            {
                using (File.Create(GetDisabledFilePath(profileName))) { }
                UpdateProfiles();
            }
        }

        public static bool IsDefault(string profileName) => string.Equals(profileName, "[Default]");
    }
}
