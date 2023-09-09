using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace MMT.Core
{
    public class TeamsLauncher
    {
        private string UpdatePath =>  Path.Combine(StaticResources.UserProfile, StaticResources.UpdateExe);

        private string TeamsPath => Path.Combine(StaticResources.UserProfile, StaticResources.LaunchExe);

        private const string _systemInstalledTag = "--system-initiated";

        private void Start(Profile profile, Action<bool, string> teamsLaunchFunc, bool inBackground, string arguments = "")
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

            teamsLaunchFunc(inBackground, arguments);

            Environment.SetEnvironmentVariable("USERPROFILE", originalUserProfilePath);
        }

        public void AutoStart(Profile profile)
        {
            Start(profile, UpdateProfileAndStartTeams, true);
        }
        
        public void UserStart(Profile profile)
        {
            Start(profile, UpdateProfileAndStartTeams, false);
        }
        
        public void OpenLink(Profile profile, string link)
        {
            Start(profile, StartTeams, false, link);
        }

        private void UpdateProfileAndStartTeams(bool inBackground, string arguments = "")
        {
            var fullArguments = new StringBuilder("--processStart Teams.exe ");
            if (inBackground || !String.IsNullOrEmpty(arguments))
            {
                fullArguments.Append("--process-start-args \"");
                if (inBackground)
                {
                    fullArguments.Append(_systemInstalledTag + " ");
                }

                if (!String.IsNullOrEmpty(arguments))
                {
                    fullArguments.Append(arguments);
                }

                fullArguments.Append("\"");
            }
            var updateExeProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = UpdatePath,
                    Arguments = fullArguments.ToString(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            updateExeProcess.Start();
        }

        private void StartTeams(bool inBackground, string arguments = "")
        {
            var updateExeProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = TeamsPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    Arguments = arguments +  (inBackground ? $" \"{_systemInstalledTag}\"" : "")
                }
            };
            updateExeProcess.Start();
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
