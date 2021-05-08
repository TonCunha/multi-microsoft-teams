using System;
using System.Diagnostics;
using System.IO;

namespace MMT.Core
{
    public class TeamsLauncher
    {        
        public void Start(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                throw new ArgumentNullException("Profile name is required.");

            string oldUserProfile = StaticResources.UserProfile;
            string userProfile = Path.Combine(StaticResources.LocalAppData, StaticResources.CustomProfiles, profileName);
            Directory.CreateDirectory(userProfile);
            Directory.CreateDirectory(Path.Combine(userProfile, "Desktop"));
            Directory.CreateDirectory(Path.Combine(userProfile, "Downloads"));
            Environment.SetEnvironmentVariable("USERPROFILE", userProfile);
            string updateExePath = Path.Combine(oldUserProfile, StaticResources.UpdateExe);
            
            UpdateProfileAndStartTeams(updateExePath);
        }

        private void UpdateProfileAndStartTeams(string updatePath)
        {
            try
            {
                var updateExeProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = updatePath,
                        Arguments = "--processStart Teams.exe",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                updateExeProcess.Start();

                while (!updateExeProcess.StandardOutput.EndOfStream)
                {
                    var line = updateExeProcess.StandardOutput.ReadLine();
                    Console.WriteLine(line);
                }

                updateExeProcess.WaitForExit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }        
    }
}
