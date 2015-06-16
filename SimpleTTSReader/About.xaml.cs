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

            textBlock.Text = string.Format(Properties.Resources.About, AssemblyInfo.GetTitle(),
                AssemblyInfo.GetVersion(),
                Properties.Resources.Website, Properties.Resources.GitHubIssues, Properties.Resources.DonateLink,
                AssemblyInfo.GetCopyright());
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}