#region

using SimpleTTSReader.Properties;

#endregion

namespace SimpleTTSReader
{
    internal class ClickOnceHelper
    {
        public static bool IsFirstLaunch => Settings.Default.Launches == 1;
    }
}