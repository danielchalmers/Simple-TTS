#region

using System.Windows;

#endregion

namespace SimpleTTSReader
{
    /// <summary>
    ///     Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();

            textBlock.Text = AssemblyInfo.GetAboutDescription();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}