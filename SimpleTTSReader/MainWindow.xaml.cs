#region

using System;
using System.ComponentModel;
using System.IO;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SimpleTTSReader.Properties;

#endregion

namespace SimpleTTSReader
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SpeechEngine _speechEngine;

        public MainWindow()
        {
            InitializeComponent();

            SettingsHelper.UpgradeSettings();
            Settings.Default.Launches++;

            txtDoc.Text = Settings.Default.Doc;
            txtDoc.SelectionStart = Settings.Default.SelectionStart;

            Title = $"{Properties.Resources.AppName} (Beta {AssemblyInfo.GetVersionString()})";

            // Initialize classes.
            _speechEngine = new SpeechEngine(this);

            if (ClickOnceHelper.IsFirstLaunch)
            {
                txtDoc.Text = string.Format(Properties.Resources.WelcomeMessage, Environment.UserName,
                    AssemblyInfo.GetVersionString(), Properties.Resources.GitHubIssues);

                _speechEngine.Start();
                txtDoc.Text +=
                    $"{Environment.NewLine}{Environment.NewLine}{AssemblyInfo.GetCopyright()}";
                txtDoc.Text += $"{Environment.NewLine}{Environment.NewLine}(This message will only appear once.)";
            }

            // Start update checker.
            UpdateChecker.Start();

            btnStart.Content = PlayButtonImage("play");
            btnStop.Content = PlayButtonImage("stop");

            txtDoc.Focus();
        }

        public string DocText
        {
            get { return txtDoc.Text; }
            set { txtDoc.Text = value; }
        }

        public int DocSelection
        {
            get { return txtDoc.SelectionStart; }
            set { txtDoc.SelectionStart = value; }
        }

        public void DocSelect(int start, int length)
        {
            txtDoc.Select(start, length);
        }

        public void SetCurrentWord(string text)
        {
            txtWord.Text = text;
        }

        private static Image PlayButtonImage(string imgname)
        {
            var finalImage = new Image {Height = 32, Stretch = Stretch.None};
            var logo = new BitmapImage();
            logo.BeginInit();
            logo.UriSource = new Uri($"pack://application:,,,/SimpleTTSReader;component/Resources/{imgname}.png");
            logo.EndInit();
            finalImage.Source = logo;
            RenderOptions.SetBitmapScalingMode(finalImage, BitmapScalingMode.NearestNeighbor);
            return finalImage;
        }

        public void SetUiState(bool start)
        {
            if (start)
            {
                // Started.
                btnStop.IsEnabled = true;
                btnStart.Content = PlayButtonImage("pause");

                txtDoc.IsReadOnly = true;
            }
            else
            {
                // Finished.
                btnStop.IsEnabled = false;
                btnStart.Content = PlayButtonImage("play");

                txtWord.Text = string.Empty;

                txtDoc.IsReadOnly = false;

                txtDoc.SelectionStart = 0;
                txtDoc.Select(0, 0);

                btnStart.Focus();
            }
        }

        private void ToggleState()
        {
            if (_speechEngine.State == SynthesizerState.Paused)
            {
                btnStart.Content = PlayButtonImage("pause");
                _speechEngine.Resume();
            }
            else if (_speechEngine.State == SynthesizerState.Speaking)
            {
                btnStart.Content = PlayButtonImage("play");
                _speechEngine.Pause();
            }
            else
            {
                btnStart.Content = PlayButtonImage("pause");
                _speechEngine.Start();
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            ToggleState();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _speechEngine.Stop();
            _speechEngine.Resume();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SettingsHelper.SaveSettings(this);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            var dia = new OpenFileDialog();
            dia.ShowDialog();
            OpenFile(dia.FileName);
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var dia = new SaveFileDialog();
            dia.ShowDialog();
            File.WriteAllText(dia.FileName, txtDoc.Text);
        }

        private void OpenFile(string path)
        {
            if (txtDoc.Text.Length > 0 &&
                Popup.Show("Are you sure you want to open this file? You will lose all current text.",
                    MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            txtDoc.Text = File.ReadAllText(path);
        }

        private void txtDoc_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (_speechEngine.State == SynthesizerState.Speaking)
                return;
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void txtDoc_PreviewDrop(object sender, DragEventArgs e)
        {
            if (_speechEngine.State == SynthesizerState.Speaking)
                return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length != 0)
                OpenFile(files[0]);
            e.Handled = true;
        }

        private void txtDoc_OnLostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void MenuItemAbout_OnClick(object sender, RoutedEventArgs e)
        {
            var dia = new About();
            dia.ShowDialog();
        }

        private void MenuItemUpdates_OnClick(object sender, RoutedEventArgs e)
        {
            ClickOnceHelper.CheckForUpdates();
        }

        private void MenuItemOptions_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsHelper.OpenOptions();
        }
    }
}