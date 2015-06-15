#region

using System;
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

        public static void CheckForUpdates(bool silent = false)
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                if (!silent)
                    Popup.Show("This application was not installed via ClickOnce and cannot be updated automatically.");
                return;
            }
            var ad = ApplicationDeployment.CurrentDeployment;

            UpdateCheckInfo info;
            try
            {
                info = ad.CheckForDetailedUpdate();
            }
            catch (DeploymentDownloadException dde)
            {
                if (!silent)
                    Popup.Show(
                        "The new version of the application cannot be downloaded at this time.\n\nPlease check your network connection, and try again later.\n\nError: " +
                        dde.Message);
                return;
            }
            catch (InvalidDeploymentException ide)
            {
                if (!silent)
                    Popup.Show(
                        "Cannot check for a new version of the application. The ClickOnce installation is corrupt. Please reinstall the application and try again.\n\nError: " +
                        ide.Message);
                return;
            }
            catch (InvalidOperationException ioe)
            {
                if (!silent)
                    Popup.Show(
                        "This application cannot be updated. It is likely not a ClickOnce application.\n\nError: " +
                        ioe.Message);
                return;
            }
            catch (Exception ex)
            {
                if (!silent)
                    Popup.Show(
                        "An error occurred while trying to check for updates.\n\nError: " +
                        ex.Message);
                return;
            }

            if (info.UpdateAvailable)
            {
                var doUpdate = true;

                if (info.IsUpdateRequired)
                {
                    Popup.Show(
                        "A mandatory update is available.\n\nThe application will now install the update and restart.");
                }
                else
                {
                    if (Popup.Show("An update is available.\n\nWould you like to update now?",
                        MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                        doUpdate = false;
                }

                if (doUpdate)
                {
                    try
                    {
                        ad.Update();
                        RestartApplication();
                    }
                    catch (DeploymentDownloadException dde)
                    {
                        Popup.Show(
                            "Cannot install the latest version of the application.\n\nPlease check your network connection, or try again later.\n\nError: " +
                            dde);
                    }
                }
            }
        }

        public static void RestartApplication()
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
    }
}