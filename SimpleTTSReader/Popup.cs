#region

using System.Windows;

#endregion

namespace SimpleTTSReader
{
    internal class Popup
    {
        public static MessageBoxResult Show(string text, MessageBoxButton btn = MessageBoxButton.OK,
            MessageBoxImage img = MessageBoxImage.Information, MessageBoxResult defaultbtn = MessageBoxResult.OK)
        {
            return MessageBox.Show(text, Properties.Resources.AppName, btn, img, defaultbtn);
        }
    }
}