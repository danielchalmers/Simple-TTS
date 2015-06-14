﻿#region

using System;
using System.Deployment.Application;
using System.Reflection;
using System.Speech.Synthesis;
using System.Windows;
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

        public MainWindow()
        {
            InitializeComponent();

            if (Settings.Default.MustUpgrade)
            {
                Settings.Default.Upgrade();
                Settings.Default.MustUpgrade = false;
                Settings.Default.Save();
            }
            
            cbGender.Text = Settings.Default.Gender;

            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SpeakStarted += (sender, args) => SetPauseVisibilityState(true);
            _synthesizer.SpeakCompleted += (sender, args) => SetPauseVisibilityState(false);
            _synthesizer.SpeakProgress += Synthesizer_OnSpeakProgress;

            Title = $"Simple TTS Reader (Beta {GetVersion()})";
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
            }
            else
            {
                // Finished.
                btnStop.IsEnabled = false;
                btnPause.IsEnabled = false;
                btnStart.IsEnabled = true;

                sliderSpeed.IsEnabled = true;
                sliderVolume.IsEnabled = true;

                txtWord.Text = string.Empty;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            var selection = txtDoc.SelectionStart;
            if (selection == txtDoc.Text.Length || selection == -1)
                selection = 0;
            var text = txtDoc.Text.Substring(selection, txtDoc.Text.Length - selection);
            if (string.IsNullOrWhiteSpace(text))
                return;
            _currentPrompt = new Prompt(text);

            _synthesizer.Rate = (int) (sliderSpeed.Value - 10);
            _synthesizer.Volume = (int) (sliderVolume.Value*5);

            _synthesizer.SelectVoiceByHints(Settings.Default.Gender == "Female" ? VoiceGender.Female :  VoiceGender.Male);

            _synthesizer.SpeakAsync(_currentPrompt);
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
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

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _synthesizer.SpeakAsyncCancel(_currentPrompt);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Gender = cbGender.Text;
            Settings.Default.Save();
        }
    }
}