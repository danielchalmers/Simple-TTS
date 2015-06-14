#region

using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.IO;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text;
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
        private readonly SpeechSynthesizer _synthesizer;
        private Prompt _currentPrompt;
        private int WordOffset = 0;

        public MainWindow()
        {
            InitializeComponent();

            if (Settings.Default.MustUpgrade)
            {
                Settings.Default.Upgrade();
                Settings.Default.MustUpgrade = false;
                Settings.Default.Save();
            }
            Settings.Default.Launches++;

            cbGender.Text = Settings.Default.Gender;
            txtDoc.Text = Settings.Default.Doc;
            txtDoc.SelectionStart = Settings.Default.SelectionStart;

            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SpeakStarted += (sender, args) => SetPauseVisibilityState(true);
            _synthesizer.SpeakCompleted += (sender, args) => SetPauseVisibilityState(false);
            _synthesizer.SpeakProgress += Synthesizer_OnSpeakProgress;

            Title = $"Simple TTS Reader (Beta {GetVersion()})";

            if (Settings.Default.Launches == 1)
            {
                var example = new StringBuilder();
                example.AppendLine($"WARNING: This application is currently in beta and will receive frequent updates and may have issues.");
                example.AppendLine();
                example.AppendLine($"Hello {Environment.UserName}.");
                example.AppendLine($"Welcome to Simple Text to Speech Reader version {GetVersion()}.");
                example.AppendLine();
                example.AppendLine($"If you want to change Text to Speech voice properties such as gender, speed, and volume, use the options on the right.");
                example.AppendLine($"To start, stop, or pause voice playback, use the buttons on the left.");
                example.AppendLine($"You can find settings, help, etc in the menu above.");
                example.AppendLine();
                example.AppendLine();
                example.AppendLine($"All features in this application are available free of charge, but feel free to leave a donation at \"{Properties.Resources.DonateLink}\"");
                example.AppendLine();
                example.AppendLine($"If you have any issues or requests, you can report them at \"{Properties.Resources.GitHubIssues}\".");
                txtDoc.Text = example.ToString();

                Start();
                txtDoc.Text += $"{Environment.NewLine}{Environment.NewLine}{GetAssemblyAttribute<AssemblyCopyrightAttribute>(a => a.Copyright)}";
                txtDoc.Text += $"{Environment.NewLine}{Environment.NewLine}(This message will only appear once.)";
            }
        }

        public string GetAssemblyAttribute<T>(Func<T, string> value)
            where T : Attribute
        {
            var attribute = (T) Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof (T));
            return value.Invoke(attribute);
        }

        private static string GetVersion()
        {
            var obj = ApplicationDeployment.IsNetworkDeployed
                ? ApplicationDeployment.CurrentDeployment.
                    CurrentVersion
                : Assembly.GetExecutingAssembly().GetName().Version;
            return $"{obj.Major}.{obj.Minor}.{obj.Build}";
        }

        private void Synthesizer_OnSpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            txtWord.Text = e.Text;
            txtDoc.Select(e.CharacterPosition + WordOffset, e.Text.Length);
        }

        private void SetPauseVisibilityState(bool pause)
        {
            if (pause)
            {
                // Started.
                btnStop.IsEnabled = true;
                btnPause.IsEnabled = true;
                btnStart.IsEnabled = false;
                btnPause.Content = "Pause";

                sliderSpeed.IsEnabled = false;
                sliderVolume.IsEnabled = false;
                cbGender.IsEnabled = false;

                txtDoc.SelectionBrush = Brushes.Yellow;
                txtDoc.IsReadOnly = true;

                txtDoc.Focus();
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

                txtDoc.SelectionBrush = new SolidColorBrush(SystemColors.HighlightColor);
                txtDoc.IsReadOnly = false;

                txtDoc.SelectionStart = 0;

                btnStart.Focus();
            }
        }

        private void Start()
        {
            var selection = txtDoc.SelectionStart;
            if (selection == txtDoc.Text.Length || selection == -1)
                selection = 0;
            var text = txtDoc.Text.Substring(selection, txtDoc.Text.Length - selection);
            WordOffset = selection;
            if (string.IsNullOrWhiteSpace(text))
                return;
            _currentPrompt = new Prompt(text);

            _synthesizer.Rate = (int) (sliderSpeed.Value - 10);
            _synthesizer.Volume = (int) (sliderVolume.Value*5);

            _synthesizer.SelectVoiceByHints(cbGender.Text == "Female" ? VoiceGender.Female : VoiceGender.Male);

            _synthesizer.SpeakAsync(_currentPrompt);
        }

        private void ToggleState()
        {
            if (_synthesizer.State == SynthesizerState.Paused)
            {
                btnPause.Content = "Pause";
                _synthesizer.Resume();
            }
            else
            {
                btnPause.Content = "Resume";
                _synthesizer.Pause();
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            ToggleState();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _synthesizer.SpeakAsyncCancel(_currentPrompt);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.Doc = txtDoc.Text;
            Settings.Default.SelectionStart = txtDoc.SelectionStart;
            Settings.Default.Gender = cbGender.Text;
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
            txtDoc.Text = File.ReadAllText(dia.FileName);
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var dia = new SaveFileDialog();
            dia.ShowDialog();
            File.WriteAllText(dia.FileName, txtDoc.Text);
        }
    }
}