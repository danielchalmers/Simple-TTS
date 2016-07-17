#region

using System;
using System.ComponentModel;
using System.IO;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SimpleTTSReader.Properties;

#endregion

namespace SimpleTTSReader
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly SpeechSynthesizer _synthesizer;
        private int _currentCharacterIndex;
        private Prompt _currentPrompt;
        private string _currentWord;
        private int _maxCharacters;
        private int _speechLength;
        private SynthesizerState _synthesizerState;
        private int _wordOffset;

        public MainWindow()
        {
            InitializeComponent();

            SettingsHelper.UpgradeSettings();
            Settings.Default.Launches++;

            txtDocument.SelectionStart = Settings.Default.SelectionStart;

            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SpeakCompleted += (sender, args) => ResetDocumentSelection();
            _synthesizer.StateChanged += (sender, args) => SynthesizerState = args.State;
            _synthesizer.SpeakProgress += Synthesizer_OnSpeakProgress;

            if (ClickOnceHelper.IsFirstLaunch)
            {
                Settings.Default.Document = string.Format(Properties.Resources.WelcomeMessage, Environment.UserName,
                    AssemblyInfo.Version, Properties.Resources.GitHubIssues);

                StartSpeech();
                Settings.Default.Document +=
                    $"{Environment.NewLine}{Environment.NewLine}{AssemblyInfo.Copyright}";
                Settings.Default.Document +=
                    $"{Environment.NewLine}{Environment.NewLine}(This message will only appear once.)";
            }

            DataContext = this;
            txtDocument.Focus();
        }

        public SynthesizerState SynthesizerState
        {
            get { return _synthesizerState; }
            set
            {
                if (_synthesizerState != value)
                {
                    _synthesizerState = value;
                    RaisePropertyChanged(nameof(SynthesizerState));
                }
            }
        }

        public int CurrentCharacterIndex
        {
            get { return _currentCharacterIndex; }
            set
            {
                if (_currentCharacterIndex != value)
                {
                    _currentCharacterIndex = value;
                    RaisePropertyChanged(nameof(CurrentCharacterIndex));
                }
            }
        }

        public string CurrentWord
        {
            get { return _currentWord; }
            set
            {
                if (_currentWord != value)
                {
                    _currentWord = value;
                    RaisePropertyChanged(nameof(CurrentWord));
                }
            }
        }

        public int MaxCharacters
        {
            get { return _maxCharacters; }
            set
            {
                if (_maxCharacters != value)
                {
                    _maxCharacters = value;
                    RaisePropertyChanged(nameof(MaxCharacters));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.SelectionStart = txtDocument.SelectionStart;
            SettingsHelper.SaveSettings();
            StopSpeech();
        }

        private void Synthesizer_OnSpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            CurrentWord = e.Text;
            CurrentCharacterIndex = e.CharacterPosition + e.Text.Length;
            MaxCharacters = _speechLength;
            txtDocument.Select(e.CharacterPosition + _wordOffset, e.Text.Length);
        }

        private void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void ToggleState()
        {
            switch (_synthesizer.State)
            {
                case SynthesizerState.Paused:
                    ResumeSpeech();
                    break;
                case SynthesizerState.Speaking:
                    PauseSpeech();
                    break;
                default:
                    StartSpeech();
                    break;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            ToggleState();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopSpeech();
            ResetDocumentSelection();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() ?? false)
                OpenFile(dialog.FileName);
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            if (dialog.ShowDialog() ?? false)
                File.WriteAllText(dialog.FileName, Settings.Default.Document);
        }

        private static void OpenFile(string path)
        {
            if (Settings.Default.Document.Length > 0 &&
                Popup.Show("Are you sure you want to open this file? You will lose all current text.",
                    MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            Settings.Default.Document = File.ReadAllText(path);
        }

        private void txtDocument_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (_synthesizer.State == SynthesizerState.Speaking)
                return;
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void txtDocument_PreviewDrop(object sender, DragEventArgs e)
        {
            if (_synthesizer.State == SynthesizerState.Speaking)
                return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length != 0)
                OpenFile(files[0]);
            e.Handled = true;
        }

        private void txtDocument_OnLostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void txtDocument_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SynthesizerState == SynthesizerState.Speaking)
                e.Handled = true;
        }

        private void MenuItemAbout_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new About();
            dialog.ShowDialog();
        }

        private void MenuItemOptions_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsHelper.OpenOptions();
        }

        private void StartSpeech()
        {
            var selection = txtDocument.SelectionStart;
            if (selection >= Settings.Default.Document.Length || selection == -1)
                selection = 0;
            var text = Settings.Default.Document.Substring(selection, Settings.Default.Document.Length - selection);
            _wordOffset = selection;
            if (string.IsNullOrWhiteSpace(text))
                return;
            _currentPrompt = new Prompt(text);
            _speechLength = text.Length;
            _synthesizer.Rate = Settings.Default.Speed - 10;
            _synthesizer.Volume = Settings.Default.Volume;

            _synthesizer.SelectVoiceByHints(Settings.Default.Gender == VoiceGender.Male
                ? System.Speech.Synthesis.VoiceGender.Male
                : System.Speech.Synthesis.VoiceGender.Female);

            _synthesizer.SpeakAsync(_currentPrompt);
        }

        private void StopSpeech()
        {
            if (_currentPrompt == null)
                return;
            _synthesizer.SpeakAsyncCancel(_currentPrompt);
            _currentPrompt = null;
            _synthesizer.Resume();

            btnStart.Focus();
        }

        private void PauseSpeech()
        {
            _synthesizer.Pause();
        }

        private void ResumeSpeech()
        {
            _synthesizer.Resume();
        }

        private void ResetDocumentSelection()
        {
            txtDocument.SelectionStart = 0;
            txtDocument.Select(0, 0);
        }
    }
}