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

            // Profile called [Default] will start Teams in the current users profile (like not using MMT)
            if (!ProfileManager.IsDefault(profileName))
            {
                // Overwrite env variable to force teams to run with the selected profile
                string mmtUserProfilePath = Path.Combine(StaticResources.LocalAppData, StaticResources.CustomProfiles, profileName);
                Environment.SetEnvironmentVariable("USERPROFILE", mmtUserProfilePath);
            }

            string updateExePath = Path.Combine(StaticResources.UserProfile, StaticResources.UpdateExe);
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
                Console.WriteLine(e.ToString());
            }
        }        
    }
}
