using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MMT.Core
{
    public class ProfileManager
    {
        private readonly string _customProfilesPath;
        private string _disabledProfilesPath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "disabled-profiles.txt");

        public ProfileManager()
        {
            string localAppData = StaticResources.LocalAppData;
            _customProfilesPath = Path.Combine(localAppData, StaticResources.CustomProfiles);
        }

        public List<string> GetProfiles()
        {
            if (!Directory.Exists(_customProfilesPath))
                Directory.CreateDirectory(_customProfilesPath);

            var allProfiles = Directory.GetDirectories(_customProfilesPath).Select(p => new DirectoryInfo(p).Name).ToList();
            var disabledProfiles = GetDisabledProfiles();

            var profiles = new List<string>();
            foreach (var profile in allProfiles)
            {
                if (disabledProfiles.Any(p => p == profile))
                    profiles.Add($"[Disabled] {profile}");
                else
                    profiles.Add(profile);
            }

            return profiles;
        }

        public void Save(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                throw new ArgumentException("Profile name is required.");

            if (GetProfiles().Any(p => p.ToUpper().Equals(profileName.ToUpper())))
                throw new ArgumentException("This profile already exists.");

            Directory.CreateDirectory(Path.Combine(_customProfilesPath, profileName));
            Directory.CreateDirectory(Path.Combine(_customProfilesPath, profileName, "Desktop"));
            Directory.CreateDirectory(Path.Combine(_customProfilesPath, profileName, "Downloads"));
        }

        public void Delete(string profileName)
        {
            string path = Path.Combine(_customProfilesPath, profileName);

            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        private List<string> GetDisabledProfiles()
        {
            var profiles = new List<string>();
            if (File.Exists(_disabledProfilesPath))
            {
                using var sr = new StreamReader(_disabledProfilesPath);
                while (!sr.EndOfStream)
                {
                    profiles.Add(sr.ReadLine());
                }
            }

            return profiles;
        }


        public void Enable(string profileName)
        {
            profileName = profileName.Replace("[Disabled] ", string.Empty);
            var disabledProfiles = GetDisabledProfiles();
            disabledProfiles.Remove(profileName);
            using var sw = new StreamWriter(_disabledProfilesPath);
            disabledProfiles.ForEach(p => sw.WriteLine(p));
        }

        public void Disable(string profileName)
        {
            var disabledProfiles = GetDisabledProfiles();
            if (!disabledProfiles.Any(p => p == profileName))
            {
                using var sw = new StreamWriter(_disabledProfilesPath);
                disabledProfiles.ForEach(p => sw.WriteLine(p));
                sw.WriteLine(profileName);                
            }
        }
    }
}
