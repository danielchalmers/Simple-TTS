#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Newtonsoft.Json;

#endregion

namespace SimpleTTSReader
{
    /// <summary>
    ///     Interaction logic for UpdatePrompt.xaml
    /// </summary>
    public partial class UpdatePrompt : Window
    {
        public enum UpdateResponse
        {
            UpdateNow,
            RemindLater,
            RemindNever
        }

        private readonly bool _isRequired;
        private readonly Version _updateVersion;
        private string _updateText;
        public UpdateResponse updateResponse;

        public UpdatePrompt(Version updateVersion, bool isRequired)
        {
            InitializeComponent();

            _updateVersion = updateVersion;
            _isRequired = isRequired;
            txtUpdateSubMsg.Text = !_isRequired
                ? ($"{AssemblyInfo.GetTitle()} {AssemblyInfo.GetVersionString(_updateVersion)} {(_updateVersion.Minor == 95 ? "(Beta) " : "")}is now available!\nDo you want to update to the latest version?")
                : $"A mandatory update is available.\nPress \"{btnOK.Content}\" to update to the latest version.";
            btnNo.Visibility = _isRequired ? Visibility.Hidden : Visibility.Visible;
            btnRemindLater.Visibility = _isRequired ? Visibility.Hidden : Visibility.Visible;
            GetChangelog();
        }

        private void GetChangelog()
        {
            var bw = new BackgroundWorker();
            bw.DoWork += BwOnDoWork;
            bw.RunWorkerCompleted += BwOnRunWorkerCompleted;
            bw.RunWorkerAsync();
            textBlock.Text = "Downloading changelog...";
        }

        private void BwOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            textBlock.Text = _updateText;
        }

        private void BwOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            try
            {
                var str = new StringBuilder();
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("User-Agent: Other");
                    var json =
                        webClient.DownloadString(Properties.Resources.GitHubApiCommits);
                    var jsonD = JsonConvert.DeserializeObject<List<GitHubApiCommitsRootObject>>(json);
                    var versionDetected = false;
                    foreach (var j in jsonD)
                    {
                        var commit = j.commit.message;
                        var index = commit.IndexOf("\n", StringComparison.Ordinal);
                        if (index > 0)
                            commit = commit.Substring(0, index);
                        var isVersion = Regex.Match(commit, @"^\d+(\.\d+)+$") != Match.Empty;
                        var firstVersion = (!versionDetected && isVersion);
                        if (isVersion)
                            versionDetected = true;

                        if (!versionDetected)
                            continue;
                        str.AppendLine(isVersion ? $"{(firstVersion ? "" : Environment.NewLine)}{commit}" : $" {commit}");
                    }
                    _updateText = str.ToString();
                }
            }
            catch
            {
                _updateText = "Changelog could not be downloaded.";
            }
        }

        private void btnRemindLater_Click(object sender, RoutedEventArgs e)
        {
            updateResponse = UpdateResponse.RemindLater;
            DialogResult = true;
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            updateResponse = UpdateResponse.RemindNever;
            DialogResult = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            updateResponse = UpdateResponse.UpdateNow;
            DialogResult = true;
        }
    }
}