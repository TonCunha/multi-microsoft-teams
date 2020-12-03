﻿using System;
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
                throw new ArgumentException("Profile name is required.");

            if (GetProfiles().Any(p => p.ToUpper().Equals(profileName.ToUpper())))
                throw new ArgumentException("This profile already exists.");

            Directory.CreateDirectory(Path.Combine(_customProfilesPath, profileName));
            Directory.CreateDirectory(Path.Combine(_customProfilesPath, profileName, "Desktop"));
        }

        public void Delete(string profileName)
        {
            string path = Path.Combine(_customProfilesPath, profileName);

            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public void Enable(string profileName)
        {
            if (Directory.Exists(Path.Combine(_customProfilesPath, profileName)))
                Directory.Move(Path.Combine(_customProfilesPath, profileName), Path.Combine(_customProfilesPath, profileName.Substring(11)));
        }

        public void Disable(string profileName)
        {
            if (Directory.Exists(Path.Combine(_customProfilesPath, profileName)))
                Directory.Move(Path.Combine(_customProfilesPath, profileName), Path.Combine(_customProfilesPath, $"[Disabled] {profileName}"));
        }
    }
}
