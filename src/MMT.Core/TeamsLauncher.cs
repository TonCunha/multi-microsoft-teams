using System;
using System.Diagnostics;
using System.IO;

namespace MMT.Core
{
    public class TeamsLauncher
    {
        public void Start(Profile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.Name))
                throw new ArgumentNullException("Profile name is required.");

            string? originalUserProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");

            // Profile called [Default] will start Teams in the current users profile (like not using MMT)
            if (!profile.IsDefault)
            {
                // Overwrite env variable to force teams to run with the selected profile
                string mmtUserProfilePath = Path.Combine(StaticResources.LocalAppData, StaticResources.CustomProfiles, profile.Name);
                Environment.SetEnvironmentVariable("USERPROFILE", mmtUserProfilePath);
            }

            string updateExePath = Path.Combine(StaticResources.UserProfile, StaticResources.UpdateExe);
            UpdateProfileAndStartTeams(updateExePath);

            // revert USERPROFILE path to default
            Environment.SetEnvironmentVariable("USERPROFILE", originalUserProfilePath);
        }

        private void UpdateProfileAndStartTeams(string updatePath)
        {
            try
            {
                Process? updateExeProcess = new Process
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
                    string? line = updateExeProcess.StandardOutput.ReadLine();
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
