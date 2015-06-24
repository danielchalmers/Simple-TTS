#region

using System.Windows;
using SimpleTTSReader.Properties;

#endregion

namespace SimpleTTSReader
{
    internal class SettingsHelper
    {
        public static void ResetSettings()
        {
            if (Popup.Show("Are you sure you want to reset all settings to default?\nThis cannot be undone.",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Settings.Default.Reset();
                Settings.Default.MustUpgrade = false;
                Settings.Default.Launches = 1;
                Settings.Default.Save();
            }
        }

        public static void SaveSettings(MainWindow mainWindow)
        {
            Settings.Default.Doc = mainWindow.txtDoc.Text;
            Settings.Default.SelectionStart = mainWindow.txtDoc.SelectionStart;
            Settings.Default.Save();

            ClickOnceHelper.RunOnStartup(Settings.Default.RunOnStartup);
        }

        public static void UpgradeSettings()
        {
            // Upgrade settings from old version.
            if (Settings.Default.MustUpgrade)
            {
                Settings.Default.Upgrade();
                Settings.Default.MustUpgrade = false;
                Settings.Default.Save();
            }
        }

        public static void OpenOptions()
        {
            // Open options window.
            var dialog = new Options();
            dialog.ShowDialog();

            if (Settings.Default.ResetSettings)
                ResetSettings();
        }
    }
}