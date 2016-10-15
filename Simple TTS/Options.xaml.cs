#region

using System;
using System.ComponentModel;
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
        public Options()
        {
            InitializeComponent();
            cbGender.ItemsSource = Enum.GetValues(typeof (VoiceGender));
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