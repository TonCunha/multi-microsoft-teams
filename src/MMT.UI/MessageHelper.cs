using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using System.Windows;

namespace MMT.UI
{
    public class MessageHelper
    {
        private static readonly string _title = "Multi MS Teams";

        public static async void Info(string message)
        {
            await ((MetroWindow)(Application.Current.MainWindow)).ShowMessageAsync(_title, message);
        }

        public static async Task<MessageDialogResult> Confirm(string message)
        {
            return await ((MetroWindow)(Application.Current.MainWindow)).ShowMessageAsync(_title, message, MessageDialogStyle.AffirmativeAndNegative);
        }
    }
}
