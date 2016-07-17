#region

using System;
using System.Deployment.Application;
using Microsoft.Win32;
using SimpleTTSReader.Properties;

#endregion

namespace SimpleTTSReader
{
    public static class ClickOnceHelper
    {
        private static readonly string AppPath =
            $"\"{Environment.GetFolderPath(Environment.SpecialFolder.Programs)}\\Daniel Chalmers\\{Resources.AppName}.appref-ms\"";

        public static bool IsFirstLaunch => Settings.Default.Launches == 1;
        public static bool IsUpdateable => ApplicationDeployment.IsNetworkDeployed;

        public static void SetRunOnStartup(bool runOnStartup)
        {
            try
            {
                var registryKey = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (registryKey == null)
                    return;
                if (runOnStartup)
                    registryKey.SetValue(Resources.AppPathName, AppPath);
                else
                    registryKey.DeleteValue(Resources.AppPathName);
            }
            catch
            {
                // ignored
            }
        }
    }
}