using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MMT.Core
{
    public class ProfileManager
    {
        private readonly string _customProfilesPath;

        public ProfileManager()
        {
            string localAppData = StaticResources.LocalAppData;
            _customProfilesPath = Path.Combine(localAppData, StaticResources.CustomProfiles);
        }

        public List<string> GetProfiles()
        {
            if (!Directory.Exists(_customProfilesPath))
                Directory.CreateDirectory(_customProfilesPath);

            return Directory.GetDirectories(_customProfilesPath).Select(p => new DirectoryInfo(p).Name).ToList();
        }

        public void Save(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                throw new ArgumentNullException("Profile name is required.");

            if (GetProfiles().Any(p => p.Equals(profileName)))
                throw new ArgumentException("This profile already exists.");

            string path = Path.Combine(_customProfilesPath, profileName);
            Directory.CreateDirectory(path);
        }

        public void Delete(string profileName)
        {
            string path = Path.Combine(_customProfilesPath, profileName);
            if (Directory.Exists(path))
                Directory.Delete(path);
        }
    }
}
