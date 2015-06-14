#region

using System.Deployment.Application;
using System.Diagnostics;
using System.Windows;
using SimpleTTSReader.Properties;

#endregion

namespace SimpleTTSReader
{
    internal class ClickOnceHelper
    {
        public static bool IsFirstLaunch => Settings.Default.Launches == 1;
        public static bool IsUpdateable => ApplicationDeployment.IsNetworkDeployed;

        public static bool CheckForUpdates()
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                return false;
            }
            var ad = ApplicationDeployment.CurrentDeployment;

            UpdateCheckInfo info;
            try
            {
                info = ad.CheckForDetailedUpdate();
            }
            catch
            {
                return false;
            }

            if (info.UpdateAvailable)
            {
                var doUpdate = true;

                if (!info.IsUpdateRequired)
                {
                    var dr = Popup.Show("An update is available. Would you like to update now?",
                        MessageBoxButton.OKCancel);
                    if (dr != MessageBoxResult.OK)
                    {
                        doUpdate = false;
                    }
                }
                else
                {
                    // Display a message that the app MUST reboot. Display the minimum required version.
                    Popup.Show(
                        "A mandatory update is available. The application will now install the update and restart.");
                }

                if (doUpdate)
                {
                    try
                    {
                        ad.Update();
                        Process.Start(Application.ResourceAssembly.Location);
                        Application.Current.Shutdown();
                    }
                    catch (DeploymentDownloadException dde)
                    {
                        Popup.Show(
                            "Cannot install the latest version of the application. \n\nPlease check your network connection, or try again later. Error: " +
                            dde);
                    }
                }
            }
            return false;
        }
    }
}