﻿using System;

namespace MMT.Core
{
    public static class StaticResources
    {
        private static string? _userProfile;
        public static string UserProfile
        {
            get
            {
                if (string.IsNullOrEmpty(_userProfile))
                    _userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                return _userProfile;
            }
        }

        private static string? _localAppData;
        public static string LocalAppData
        {
            get
            {
                if (string.IsNullOrEmpty(_localAppData))
                    _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                return _localAppData;
            }
        }

        public static string CustomProfiles
        {
            get
            {
                return @"Microsoft\Teams\CustomProfiles";
            }
        }

        public static string UpdateExe
        {
            get
            {
                return @"AppData\Local\Microsoft\Teams\Update.exe";
            }
        }

        public static string StartupApplications
        {
            get
            {
                return @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            }
        }

        public static string AppName
        {
            get
            {
                return "Multi MS Teams";
            }
        }

        public static string AppDataFolderName => "MultiTeams";

        public static string UserSettingsFileName => "UserSettings.json";
    }
}
