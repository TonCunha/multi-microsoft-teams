using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MMT.Core
{
    public class TeamsLauncher
    {
        public void Start(Profile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.Name))
            {
                throw new ArgumentNullException("Profile name is required.");
            }

            string? originalUserProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");

            if (!profile.IsDefault)
            {
                string mmtUserProfilePath = Path.Combine(StaticResources.LocalAppData, StaticResources.CustomProfiles, profile.Name);
                Environment.SetEnvironmentVariable("USERPROFILE", mmtUserProfilePath);
            }

            string updateExePath = Path.Combine(StaticResources.UserProfile, StaticResources.UpdateExe);
            UpdateProfileAndStartTeams(updateExePath);

            Environment.SetEnvironmentVariable("USERPROFILE", originalUserProfilePath);
        }

        private void UpdateProfileAndStartTeams(string updatePath)
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
            while (!updateExeProcess.StandardOutput.EndOfStream) { }
            updateExeProcess.WaitForExit();
        }

        public void CloseAllInstances()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = "/C taskkill /IM Teams.exe /F"
                }
            };
            process.Start();
            Thread.Sleep(TimeSpan.FromSeconds(15));
        }
    }
}
