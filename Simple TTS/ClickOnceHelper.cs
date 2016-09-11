#region

using System;
using System.Deployment.Application;
using Simple_TTS.Properties;

#endregion

namespace Simple_TTS
{
    public static class ClickOnceHelper
    {
        private static readonly string AppPath =
            $"\"{Environment.GetFolderPath(Environment.SpecialFolder.Programs)}\\Daniel Chalmers\\{Resources.AppName}.appref-ms\"";

        public static bool IsFirstLaunch => Settings.Default.Launches == 1;
        public static bool IsUpdateable => ApplicationDeployment.IsNetworkDeployed;
    }
}