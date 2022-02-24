using MMT.Core;
using System.Windows;

namespace MMT.UI
{
    public class UserSettings : DependencyObject
    {
        private static readonly SettingsManager<UserSettings> _userSettingsManager = new SettingsManager<UserSettings>(StaticResources.UserSettingsFileName);

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
            DependencyProperty.Register("LaunchAllOnStartup", typeof(bool), typeof(UserSettings));

        public UserSettings() { }

        public static UserSettings Init()
        {
            return _userSettingsManager.LoadSettings();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            _userSettingsManager.SaveSettings(this);
        }

        protected override bool ShouldSerializeProperty(DependencyProperty dp)
        {
            return dp.OwnerType == GetType();
        }
    }
}
