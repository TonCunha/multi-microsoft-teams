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
        private IList<string> _profiles;
        public IList<string> Profiles => _profiles;

        private readonly string _customProfilesPath;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyRaised(string propertyname)
        {
            if (propertyname == nameof(Profiles))
            {
                _profiles = GetProfiles();
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        public ProfileManager()
        {
            string localAppData = StaticResources.LocalAppData;
            _customProfilesPath = Path.Combine(localAppData, StaticResources.CustomProfiles);
            _profiles = GetProfiles();
        }

        public List<string> GetProfiles()
        {
            if (!Directory.Exists(_customProfilesPath))
                Directory.CreateDirectory(_customProfilesPath);

            return Directory.GetDirectories(_customProfilesPath)
                .Select(path => new DirectoryInfo(path).Name)
                .Where(name => !String.IsNullOrWhiteSpace(name))
                .Select(profile => IsDisabled(profile) ? $"[Disabled] {profile}" : profile)
                .ToList();
        }

        private string GetProfilePath (string profileName) => Path.Combine(_customProfilesPath, profileName);

        public void Save(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                throw new ArgumentException("Profile name is required.");

            if (GetProfiles().Any(p => p.ToUpper().Equals(profileName.ToUpper())))
                throw new ArgumentException("This profile already exists.");

            var path = GetProfilePath(profileName);
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, "Desktop"));
            Directory.CreateDirectory(Path.Combine(path, "Downloads"));
            
            OnPropertyRaised(nameof(Profiles));
        }

        public void Delete(string profileName)
        {
            string path = GetProfilePath(profileName);

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            OnPropertyRaised(nameof(Profiles));
        }

        private string GetDisabledFilePath(string profileName) => Path.Combine(GetProfilePath(profileName), "MMT.disabled");

        private bool IsDisabled(string profileName) => File.Exists(GetDisabledFilePath(profileName));

        public void Enable(string profileName)
        {
            if (IsDisabled(profileName))
            {
                File.Delete(GetDisabledFilePath(profileName));
                OnPropertyRaised(nameof(Profiles));
            }
        }

        public void Disable(string profileName)
        {
            if (!IsDisabled(profileName))
            {
                using (File.Create(GetDisabledFilePath(profileName))) { }
                OnPropertyRaised(nameof(Profiles));
            }
        }
    }
}
