#region

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using Simple_TTS.Properties;

#endregion

namespace Simple_TTS
{
    /// <summary>
    ///     Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options(IEnumerable<InstalledVoice> installedVoices)
        {
            InitializeComponent();
            cbVoice.ItemsSource = installedVoices.Where(x => x.Enabled).Select(x => x.VoiceInfo.Name);
            Settings.Default.Save();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Options_OnClosing(object sender, CancelEventArgs e)
        {
            if (DialogResult != true)
            {
                Settings.Default.Reload();
            }
        }
    }
}