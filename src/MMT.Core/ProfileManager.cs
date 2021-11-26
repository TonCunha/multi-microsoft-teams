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
		private IList<Profile> _profiles = new List<Profile>();
		public IList<Profile> Profiles => _profiles;

		private static readonly string _customProfilesPath = Path.Combine(StaticResources.LocalAppData, StaticResources.CustomProfiles);

		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyRaised(string propertyname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));

		public ProfileManager()
		{
			UpdateProfiles();
			MigrateDisabledProfiles();
		}

		#region Public
		public void Save(string profileName)
		{
			if (string.IsNullOrWhiteSpace(profileName))
			{
				throw new ArgumentException("Profile name is required.");
			}

			if (Profiles.Any(p => string.Equals(p.Name, profileName, StringComparison.OrdinalIgnoreCase)))
			{
				throw new ArgumentException("This profile already exists.");
			}

			string? path = Path.Combine(_customProfilesPath, profileName);
			Directory.CreateDirectory(path);
			Directory.CreateDirectory(Path.Combine(path, "Desktop"));
			Directory.CreateDirectory(Path.Combine(path, "Downloads"));

			UpdateProfiles();
		}

		public void Update(Profile profile, string newName)
		{
			if (string.IsNullOrWhiteSpace(newName))
			{
				throw new ArgumentException("Profile name is required.");
			}
			
			if (Profiles.Any(p => string.Equals(p.Name, newName, StringComparison.OrdinalIgnoreCase)))
			{
				throw new ArgumentException("This profile already exists.");
			}

			var existingProfile = Profiles.FirstOrDefault(x => x.Name == profile.Name);
			if (existingProfile == null)
			{
				throw new ArgumentException("Invalid profile.");
			}

			if (existingProfile.IsDefault)
			{
				Properties.UserSettings.Default.DefaultInstanceName = newName;
				Properties.UserSettings.Default.Save();
			}
			else
			{
				var oldPath = Path.Combine(_customProfilesPath, existingProfile.Name);
				var newPath = Path.Combine(_customProfilesPath, newName);
				existingProfile.Name = newName;
				Directory.Move(oldPath, newPath);
			}

			UpdateProfiles();
		}

		public void Delete(Profile profile)
		{
			if (!profile.IsDefault)
			{
				if (Directory.Exists(profile.Path))
				{
					bool isFilesLocked = new DirectoryInfo(profile.Path).GetFiles("*.*", SearchOption.AllDirectories).Any(f => IsFileLocked(f));
					if (isFilesLocked)
					{
						throw new IOException("Some files are locked, you need close all instances of Microsoft Teams before delete this profile.");
					}

					Directory.Delete(profile.Path, true);
				}
			}

			UpdateProfiles();
		}

		protected virtual bool IsFileLocked(FileInfo file)
		{
			try
			{
				using FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
				stream.Close();
			}
			catch (IOException)
			{
				return true;
			}

			return false;
		}

		public void Enable(Profile profile)
		{
			if (profile.IsDisabled)
			{
				File.Delete(GetDisabledFilePath(profile));
				UpdateProfiles();
			}
		}

		public void Disable(Profile profile)
		{
			if (!profile.IsDisabled)
			{
				using (File.Create(GetDisabledFilePath(profile)))
				{
					UpdateProfiles();
				}
			}
		}

		public static bool IsDisabled(Profile profile) => File.Exists(GetDisabledFilePath(profile));
		#endregion

		#region Private Helper
		private void UpdateProfiles()
		{
			if (!Directory.Exists(_customProfilesPath))
				Directory.CreateDirectory(_customProfilesPath);

			_profiles = Directory.GetDirectories(_customProfilesPath)
				 .Select(fullPath =>
				 {
					 string name = new DirectoryInfo(fullPath).Name;
					 return new Profile(name, fullPath);
				 })
				 .ToList();

			_profiles.Insert(0, new Profile(Properties.UserSettings.Default.DefaultInstanceName, StaticResources.UserProfile) { IsDefault = true });

			OnPropertyRaised(nameof(Profiles));
		}

		private static string GetDisabledFilePath(Profile profile) => Path.Combine(profile.Path, "MMT.disabled");

		private void MigrateDisabledProfiles()
		{
			string _disabledProfilesPath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!, "disabled-profiles.txt");

			List<string> oldDisabledProfiles = new List<string>();
			if (File.Exists(_disabledProfilesPath))
			{
				using StreamReader? sr = new StreamReader(_disabledProfilesPath);
				while (!sr.EndOfStream)
				{
					oldDisabledProfiles.Add(sr.ReadLine()!);
				}

				oldDisabledProfiles
					 .Select(disableProfileName => _profiles.FirstOrDefault(profile => profile.Name == disableProfileName))
					 .Where(profileToMigrate => profileToMigrate != null)
					 .ToList()
					 .ForEach(Disable);

				File.Delete(_disabledProfilesPath);
			}
		}
		#endregion
	}
}
