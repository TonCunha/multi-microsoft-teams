using MMT.Core;
using System.Text.Json.Serialization;
using System.Windows;

namespace MMT.UI
{
    public class UserSettings : DependencyObject
    {
        private static readonly SettingsManager<UserSettings> _userSettingsManager = new(StaticResources.UserSettingsFileName);

        /// <summary>
        /// Gets or sets if all existing, enabled Teams profiles should be launched on Application start.
        /// </summary>
        public bool LaunchAllOnStartup
        {
            get => (bool)GetValue(LaunchAllOnStartupProperty);
            set => SetValue(LaunchAllOnStartupProperty, value);
        }

        // Using a DependencyProperty as the backing store for LaunchAllOnStartup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LaunchAllOnStartupProperty =
            DependencyProperty.Register(nameof(LaunchAllOnStartup), typeof(bool), typeof(UserSettings));

        [JsonConstructor]
        public UserSettings(bool launchAllOnStartup)
        {
            LaunchAllOnStartup = launchAllOnStartup;
        }

        public static UserSettings Init() => _userSettingsManager.LoadSettings() ?? new UserSettings(false);

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            _userSettingsManager.SaveSettings(this);
        }
    }
}
