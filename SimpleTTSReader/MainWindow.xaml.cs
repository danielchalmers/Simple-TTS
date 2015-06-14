#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Media;
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
        private readonly UpdateChecker _updateChecker;

        public MainWindow()
        {
            InitializeComponent();

            SettingsHelper.UpgradeSettings();
            Settings.Default.Launches++;

            txtDoc.Text = Settings.Default.Doc;
            txtDoc.SelectionStart = Settings.Default.SelectionStart;

            Title = $"Simple TTS Reader (Beta {AssemblyInfo.GetVersion()})";

            // Initialize classes.
            _speechEngine = new SpeechEngine(this);
            _updateChecker = new UpdateChecker();

            if (ClickOnceHelper.IsFirstLaunch)
            {
                txtDoc.Text = AssemblyInfo.GetWelcomeMessage();

                _speechEngine.Start();
                txtDoc.Text +=
                    $"{Environment.NewLine}{Environment.NewLine}{AssemblyInfo.GetCopyright()}";
                txtDoc.Text += $"{Environment.NewLine}{Environment.NewLine}(This message will only appear once.)";
            }

            // Start update checker.
            _updateChecker.Start();

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

        public void SetUiState(bool start)
        {
            if (start)
            {
                // Started.
                btnStop.IsEnabled = true;
                btnPause.IsEnabled = true;
                btnStart.IsEnabled = false;
                btnPause.Content = "Pause";

                sliderSpeed.IsEnabled = false;
                sliderVolume.IsEnabled = false;
                cbGender.IsEnabled = false;

                txtDoc.IsReadOnly = true;
            }
            else
            {
                // Finished.
                btnStop.IsEnabled = false;
                btnPause.IsEnabled = false;
                btnStart.IsEnabled = true;

                sliderSpeed.IsEnabled = true;
                sliderVolume.IsEnabled = true;
                cbGender.IsEnabled = true;

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
                btnPause.Content = "Pause";
                _speechEngine.Resume();
            }
            else
            {
                btnPause.Content = "Resume";
                _speechEngine.Pause();
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            _speechEngine.Start();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            ToggleState();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _speechEngine.Stop();
        }

        private void btnResetSettings_Click(object sender, RoutedEventArgs e)
        {
            if (Popup.Show("Are you sure you want to reset all settings to default?\nThis cannot be undone.",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Settings.Default.Reset();
                Settings.Default.MustUpgrade = false;
                Settings.Default.Launches = 1;
            }
        }
        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.Doc = txtDoc.Text;
            Settings.Default.SelectionStart = txtDoc.SelectionStart;
            Settings.Default.Save();
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
    }
}