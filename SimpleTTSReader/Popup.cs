#region

using System.Windows;
using SimpleTTSReader.Properties;

#endregion

namespace SimpleTTSReader
{
    public static class Popup
    {
        public static MessageBoxResult Show(string text, MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.Information, MessageBoxResult defaultButton = MessageBoxResult.OK)
        {
            return MessageBox.Show(text, Resources.AppName, button, image, defaultButton);
        }
    }
}