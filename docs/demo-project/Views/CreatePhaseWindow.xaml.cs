using System.Windows;

namespace Ofcas.Lk.Api.Client.Demo.Views
{
    public partial class CreatePhaseWindow
    {
        public CreatePhaseWindow()
        {
            InitializeComponent();
        }

        private void OkayOnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}