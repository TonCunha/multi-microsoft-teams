using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MMT.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MMT.UI
{
    public partial class ChooseProfileForLink : MetroWindow
    {
        private readonly ProfileManager _profileManager;
        private readonly TeamsLauncher _teamsLauncher;
        private readonly string _link; 

        public ChooseProfileForLink(string link)
        {
            InitializeComponent();
            _profileManager = new ProfileManager();
            _teamsLauncher = new TeamsLauncher();
            _link = link;
            DataContext = _profileManager;
        }
        
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Profile profile)
            {
                _teamsLauncher.OpenLink(profile, _link);
                Close();
            }
        }

        private void BtnOpenLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstProfiles.SelectedItems?.Count > 0)
                {
                    lstProfiles.SelectedItems.OfType<Profile>()
                        .Where(item => !item.IsDisabled)
                        .ToList()
                        .ForEach(item =>
                    {
                        _teamsLauncher.OpenLink(item, _link);
                        Close();
                    });
                }
            }
            catch (Exception ex)
            {
                MessageHelper.Info(ex.Message);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var processInfo = new ProcessStartInfo(e.Uri.AbsoluteUri) 
            { 
                UseShellExecute = true 
            };
            Process.Start(processInfo);
        }
    }
}

