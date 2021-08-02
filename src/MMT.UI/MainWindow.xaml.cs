using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MMT.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MMT.UI
{
    public partial class MainWindow : MetroWindow
    {
        private readonly ProfileManager _profileManager;
        private readonly TeamsLauncher _teamsLauncher;
        private readonly RegistryManager _registryManager;
        private TaskbarIcon _tray;

        public MainWindow()
        {
            InitializeComponent();
            _profileManager = new ProfileManager();
            _teamsLauncher = new TeamsLauncher();
            _registryManager = new RegistryManager();
            DataContext = _profileManager;
            ChangeTabVisibility();
            CreateTray();
            AutoStartCheck();
        }

        private void ChangeTabVisibility()
        {
            if (tbiNewProfile.Visibility == Visibility.Visible)
            {
                tbiProfiles.Visibility = Visibility.Visible;
                tbiNewProfile.Visibility = Visibility.Collapsed;
                tbcMain.SelectedItem = tbiProfiles;
            }
            else
            {
                tbiProfiles.Visibility = Visibility.Collapsed;
                tbiNewProfile.Visibility = Visibility.Visible;
                tbcMain.SelectedItem = tbiNewProfile;
            }
        }

        private void CreateTray()
        {
            _tray = new TaskbarIcon
            {
                Icon = Resource.Taskbar,
                ToolTipText = StaticResources.AppName,
                Visibility = Visibility.Collapsed
            };
            _tray.TrayMouseDoubleClick += TrayMouseDoubleClick;
            var items = DataContext as IList<string>;
            if (items?.Count > 0)
            {
                _tray.ContextMenu = new ContextMenu();
                items.ToList().ForEach((profile) =>
                {
                    var menuItem = new MenuItem() { Header = profile };
                    menuItem.Click += MenuItem_Click;
                    _tray.ContextMenu.Items.Add(menuItem);
                });
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (sender != null)
            {
                _teamsLauncher.Start(menuItem.Header.ToString());
            }
        }

        private void TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            MetroWindow_StateChanged(sender, e);
        }

        private void AutoStartCheck()
        {
            chkAutoStart.IsChecked = _registryManager.IsApplicationInStartup(StaticResources.AppName);

            if (chkAutoStart.IsChecked.HasValue && chkAutoStart.IsChecked.Value)
            {
                Show();
                WindowState = WindowState.Minimized;
                MetroWindow_StateChanged(null, null);

                var thread = new Thread(() =>
                {
                    foreach (var item in lstProfiles.Items)
                        if (!item.ToString().StartsWith("[Disabled]"))
                            _teamsLauncher.Start(item.ToString());
                });
                thread.Start();
            }
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Visibility = Visibility.Collapsed;
                _tray.Visibility = Visibility.Visible;
                _tray.ShowBalloonTip(StaticResources.AppName, "This app is running", BalloonIcon.Info);
            }
            else
            {
                _tray.Visibility = Visibility.Collapsed;
                Visibility = Visibility.Visible;
            }
        }

        private void ChkAutoStart_Click(object sender, RoutedEventArgs e)
        {
            if (chkAutoStart.IsChecked.HasValue && chkAutoStart.IsChecked.Value)
                _registryManager.AddApplicationInStartup(StaticResources.AppName);
            else if (_registryManager.IsApplicationInStartup(StaticResources.AppName))
                _registryManager.RemoveApplicationFromStartup(StaticResources.AppName);
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
                if (lstProfiles.SelectedItems?.Count > 0)
                {
                    lstProfiles.SelectedItems.OfType<string>()
                        .Where((item) => !item.StartsWith("[Disabled]"))
                        .ToList()
                        .ForEach((item) =>
                    {
                        _teamsLauncher.Start(item);
                    });
                }
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
                for (int i = (lstProfiles.SelectedItems.Count - 1); i >= 0; i--)
                {
                    var selectedProfile = lstProfiles.SelectedItems[i].ToString();
                    if (await MessageHelper.Confirm(string.Format("Delete profile?\nProfile name: {0}", selectedProfile)) == MessageDialogResult.Affirmative)
                    {
                        _profileManager.Delete(selectedProfile);
                    }
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }

        private async void LstProfiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && item.DataContext is string selectedProfile)
            {
                if (selectedProfile.StartsWith("[Disabled]"))
                    _profileManager.Enable(selectedProfile);
                else if (await MessageHelper.Confirm($"Disable profile?\nProfile name: {selectedProfile}") == MessageDialogResult.Affirmative)
                    _profileManager.Disable(selectedProfile);
            }
        }
    }
}
