#region

#endregion

using Simple_TTS.Properties;

namespace Simple_TTS
{
    public static class SettingsHelper
    {
        public static void SaveSettings()
        {
            Settings.Default.Save();

            ClickOnceHelper.SetRunOnStartup(Settings.Default.RunOnStartup);
        }

        public static void UpgradeSettings()
        {
            if (Settings.Default.MustUpgrade)
            {
                Settings.Default.Upgrade();
                Settings.Default.MustUpgrade = false;
                Settings.Default.Save();
            }
        }

        public static void OpenOptions()
        {
            var dialog = new Options();
            dialog.ShowDialog();
        }
    }
}