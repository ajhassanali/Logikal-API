using System.Windows;

namespace Ofcas.Lk.Api.Client.Demo.Views
{
    /// <summary>
    /// Interaction logic for SelectElevationEditModeWindow.xaml
    /// </summary>
    public partial class SelectElevationEditModeWindow
    {
        public SelectElevationEditModeWindow()
        {
            InitializeComponent();
        }

        private void OnClickDefault(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
