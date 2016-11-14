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
    }
}