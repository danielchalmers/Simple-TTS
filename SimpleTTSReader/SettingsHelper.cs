#region

using SimpleTTSReader.Properties;

#endregion

namespace SimpleTTSReader
{
    internal class SettingsHelper
    {
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
    }
}