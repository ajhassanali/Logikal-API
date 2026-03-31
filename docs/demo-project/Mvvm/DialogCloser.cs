using System.Windows;

namespace Ofcas.Lk.Api.Client.Demo.Mvvm
{
    public static class DialogCloser
    {
        public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached(
            "DialogResult", typeof(bool?), typeof(DialogCloser), new PropertyMetadata(DialogResultChanged));

        private static void DialogResultChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs arguments)
        {
            Window window = dependencyObject as Window;
            if (window != null)
                window.DialogResult = arguments.NewValue as bool?;
        }

        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
    }
}