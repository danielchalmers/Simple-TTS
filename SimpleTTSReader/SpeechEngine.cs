#region

using System;
using System.Speech.Synthesis;
using SimpleTTSReader.Properties;

#endregion

namespace SimpleTTSReader
{
    internal class SpeechEngine
    {
        private readonly MainWindow _mainWindow;
        private readonly SpeechSynthesizer _synthesizer;
        private Prompt _currentPrompt;
        private int _wordOffset;
        private int _speechLength;

        public SpeechEngine(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SpeakStarted += (sender, args) => _mainWindow.SetUiState(true);
            _synthesizer.SpeakCompleted += (sender, args) => _mainWindow.SetUiState(false);
            _synthesizer.SpeakProgress += Synthesizer_OnSpeakProgress;
        }

        public SynthesizerState State => _synthesizer.State;

        public void Synthesizer_OnSpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            _mainWindow.SetCurrentWord(e.Text);
            _mainWindow.SetProgressStatus(e.CharacterPosition + e.Text.Length, _speechLength);
            _mainWindow.DocSelect(e.CharacterPosition + _wordOffset, e.Text.Length);
        }

        public void Start()
        {
            var selection = _mainWindow.DocSelection;
            if (selection == _mainWindow.DocText.Length || selection == -1)
                selection = 0;
            var text = _mainWindow.DocText.Substring(selection, _mainWindow.DocText.Length - selection);
            _wordOffset = selection;
            if (string.IsNullOrWhiteSpace(text))
                return;
            _currentPrompt = new Prompt(text);

            _speechLength = text.Length;

            _synthesizer.Rate = Settings.Default.Speed - 10;
            _synthesizer.Volume = Settings.Default.Volume;

            _synthesizer.SelectVoiceByHints(Settings.Default.Gender == "Female" ? VoiceGender.Female : VoiceGender.Male);

            _synthesizer.SpeakAsync(_currentPrompt);
        }

        public void Stop()
        {
            _synthesizer.SpeakAsyncCancel(_currentPrompt);
        }

        public void Pause()
        {
            _synthesizer.Pause();
        }

        public void Resume()
        {
            _synthesizer.Resume();
        }
    }
}