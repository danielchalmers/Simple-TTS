#region

using System.Windows;
using Simple_TTS.Properties;

#endregion

namespace Simple_TTS
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