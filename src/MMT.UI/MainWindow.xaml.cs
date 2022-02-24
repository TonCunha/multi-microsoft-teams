using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MMT.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
		private Profile editProfile;
		
		private UserSettings _userSettings;
		public UserSettings UserSettings { get => _userSettings ??= UserSettings.Init(); }

		public MainWindow()
		{
			InitializeComponent();
			_profileManager = new ProfileManager();
			_teamsLauncher = new TeamsLauncher();
			_registryManager = new RegistryManager();
			DataContext = _profileManager;
			ChangeTabVisibility(TabEnum.Main);
			AutoStartCheck();
			AutoLaunchCheck();
		}

		private void ChangeTabVisibility(TabEnum tab)
		{
			switch (tab)
			{
				case TabEnum.NewProfile:
					tbcMain.SelectedItem = tbiNewProfile;
					tbiProfiles.Visibility = Visibility.Collapsed;
					tbiNewProfile.Visibility = Visibility.Visible;
					tbiEditProfile.Visibility = Visibility.Collapsed;
					break;
				case TabEnum.EditProfile:
					tbcMain.SelectedItem = tbiEditProfile;
					tbiProfiles.Visibility = Visibility.Collapsed;
					tbiNewProfile.Visibility = Visibility.Collapsed;
					tbiEditProfile.Visibility = Visibility.Visible;
					break;
				default:
					tbcMain.SelectedItem = tbiProfiles;
					tbiProfiles.Visibility = Visibility.Visible;
					tbiNewProfile.Visibility = Visibility.Collapsed;
					tbiEditProfile.Visibility = Visibility.Collapsed;
					break;
			}

		}
		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem menuItem && menuItem.DataContext is Profile profile)
			{
				_teamsLauncher.Start(profile);
			}
		}

		private void TrayMouseDoubleClick(object sender, RoutedEventArgs e)
		{
			Show();
			WindowState = WindowState.Normal;
			MetroWindow_StateChanged(sender, e);
		}

		private void LaunchAllEnabledProfiles()
        {
			Thread thread = new Thread(() =>
			{
				foreach (Profile item in lstProfiles.Items.OfType<Profile>().Where((p) => p.IsEnabled))
				{
					_teamsLauncher.Start(item);
				}
			});
			thread.Start();
		}

		private void AutoStartCheck()
		{
			chkAutoStart.IsChecked = _registryManager.IsApplicationInStartup(StaticResources.AppName);

			if (chkAutoStart.IsChecked.HasValue && chkAutoStart.IsChecked.Value)
			{
				Show();
				WindowState = WindowState.Minimized;
				MetroWindow_StateChanged(null, null);
				LaunchAllEnabledProfiles();
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
			{
				_registryManager.AddApplicationInStartup(StaticResources.AppName);
			}
			else if (_registryManager.IsApplicationInStartup(StaticResources.AppName))
			{
				_registryManager.RemoveApplicationFromStartup(StaticResources.AppName);
			}
		}

		private void BtnNewProfile_Click(object sender, RoutedEventArgs e)
		{
			txtProfileName.Clear();
			ChangeTabVisibility(TabEnum.NewProfile);
		}

		private void BtnSave_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				_profileManager.Save(txtProfileName.Text);
				ChangeTabVisibility(TabEnum.Main);
			}
			catch (Exception ex)
			{
				MessageHelper.Info(ex.Message);
				txtProfileName.Focus();
			}
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			ChangeTabVisibility(TabEnum.Main);
		}

		private void BtnLaunchTeams_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (lstProfiles.SelectedItems?.Count > 0)
				{
					lstProfiles.SelectedItems.OfType<Profile>()
						 .Where((item) => !item.IsDisabled)
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
			}
		}

		private async void LstProfiles_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
			{
				for (int i = lstProfiles.SelectedItems.Count - 1; i >= 0; i--)
				{
					var selectedProfile = lstProfiles.SelectedItems[i] as Profile;
					if (selectedProfile != null)
					{
						await RemoveProfile(selectedProfile);
					}
				}
			}
		}

		private async Task RemoveProfile(Profile selectedProfile)
		{
			if (!selectedProfile.IsDefault &&
				 await MessageHelper.Confirm($"Delete profile?\nProfile name: {selectedProfile.Name}") == MessageDialogResult.Affirmative)
			{
				try
				{
					_profileManager.Delete(selectedProfile);
				}
				catch (UnauthorizedAccessException)
				{
					MessageHelper.Info($"Profile {selectedProfile.Name} has not been deleted. Close Microsoft Teams and try again.");
				}
				catch (IOException ex)
				{
					if (await MessageHelper.Confirm($"{ex.Message} Do you want continue?") == MessageDialogResult.Affirmative)
					{
						var controller = await MessageHelper.Wait("Processing, please wait.");
						_ = Task.Run(() =>
						{
							_teamsLauncher.CloseAllInstances();
							_profileManager.Delete(selectedProfile);
						}).ContinueWith(a => controller.CloseAsync());
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
			if (sender is ListBoxItem item && item.DataContext is Profile selectedProfile)
			{
				if (selectedProfile.IsDisabled)
				{
					_profileManager.Enable(selectedProfile);
				}
				else if (await MessageHelper.Confirm($"Disable profile?\nProfile name: {selectedProfile.Name}") == MessageDialogResult.Affirmative)
				{
					_profileManager.Disable(selectedProfile);
				}
			}
		}

		private void MenuItemEdit_OnClick(object sender, RoutedEventArgs e)
		{
			var menuItem = sender as MenuItem;
			if (menuItem == null) return;

			var profile = menuItem.DataContext as Profile;
			if (profile == null) return;

			editProfile = profile;
			txtProfileNameEdit.Text = profile.Name;

			ChangeTabVisibility(TabEnum.EditProfile);

		}

		private async void MenuItemDelete_OnClick(object sender, RoutedEventArgs e)
		{
			var menuItem = sender as MenuItem;
			if (menuItem == null) return;

			var profile = menuItem.DataContext as Profile;
			if (profile != null)
			{
				await RemoveProfile(profile);
			}
		}

		private void BtnCancelEdit_Click(object sender, RoutedEventArgs e)
		{
			ChangeTabVisibility(TabEnum.Main);
		}

		private void BtnSaveEdit_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				_profileManager.Update(editProfile, txtProfileNameEdit.Text);
				ChangeTabVisibility(TabEnum.Main);
			}
			catch (Exception ex)
			{
				MessageHelper.Info(ex.Message);
				txtProfileName.Focus();
			}
		}

		private void AutoLaunchCheck()
        {
			if (UserSettings.LaunchAllOnStartup)
            {
				Show();
				LaunchAllEnabledProfiles();
            }
        }
    }
}

