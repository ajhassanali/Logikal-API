using System.Windows;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public static class DialogCloser
    {
        public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached("DialogResult",
            typeof(bool?), typeof(DialogCloser), new PropertyMetadata(DialogResultPropertyChanged));

        private static void DialogResultPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var window = dependencyObject as Window;
            if (window != null)
                window.DialogResult = e.NewValue as bool?;
        }

        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
    }
}
