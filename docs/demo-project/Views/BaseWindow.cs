using System.Windows;

namespace Ofcas.Lk.Api.Client.Demo.Views
{
    public abstract class BaseWindow : Window
    {
        static BaseWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseWindow),
                new FrameworkPropertyMetadata(typeof(BaseWindow)));
        }
    }
}
