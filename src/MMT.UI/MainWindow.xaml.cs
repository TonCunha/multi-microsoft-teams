using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MMT.Core;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace MMT.UI
{
    public partial class MainWindow : MetroWindow
    {
        private readonly ProfileManager _profileManager;
        private TeamsLauncher _teamsLauncher;

        public MainWindow()
        {
            InitializeComponent();
            _profileManager = new ProfileManager();
            _teamsLauncher = new TeamsLauncher();
            ChangeTabVisibility();
        }

        private void ChangeTabVisibility()
        {
            if (tbiNewProfile.Visibility == Visibility.Visible)
            {
                tbiProfiles.Visibility = Visibility.Visible;
                tbiNewProfile.Visibility = Visibility.Collapsed;
                tbcMain.SelectedItem = tbiProfiles;
                LoadProfiles();
            }
            else
            {
                tbiProfiles.Visibility = Visibility.Collapsed;
                tbiNewProfile.Visibility = Visibility.Visible;
                tbcMain.SelectedItem = tbiNewProfile;
            }
        }

        private void LoadProfiles()
        {
            lstProfiles.Items.Clear();
            List<string> profiles = _profileManager.GetProfiles();
            profiles.ForEach(p => lstProfiles.Items.Add(p));
        }

        private void BtnNewProfile_Click(object sender, RoutedEventArgs e)
        {
            txtProfileName.Clear();
            ChangeTabVisibility();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _profileManager.Save(txtProfileName.Text);
                ChangeTabVisibility();
            }
            catch (Exception ex)
            {
                MessageHelper.Info(ex.Message);
                txtProfileName.Focus();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ChangeTabVisibility();
        }

        private void BtnLaunchTeams_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkAllProfiles.IsChecked.HasValue && chkAllProfiles.IsChecked.Value)
                    foreach (var item in lstProfiles.Items)
                        _teamsLauncher.Start(item.ToString());
                else if (lstProfiles.SelectedItem != null)
                    _teamsLauncher.Start(lstProfiles.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                MessageHelper.Info(ex.Message);
                txtProfileName.Focus();
            }
        }

        private async void LstProfiles_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                string selectedProfile = lstProfiles.SelectedItem.ToString();
                if (await MessageHelper.Confirm(string.Format("Delete profile?\nProfile name: {0}", selectedProfile)) == MessageDialogResult.Affirmative)
                {
                    _profileManager.Delete(selectedProfile);
                    LoadProfiles();
                }
            }
        }
    }
}
