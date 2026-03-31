using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public static class KeyboardFocusManager
    {
        public static readonly DependencyProperty FocusOnStartupProperty = DependencyProperty.RegisterAttached("FocusOnStartup",
            typeof(bool), typeof(KeyboardFocusManager), new FrameworkPropertyMetadata(OnFocusOnStartupChanged));

        public static bool GetFocusOnStartup(UIElement element)
        {
            return (bool)element.GetValue(FocusOnStartupProperty);
        }

        public static void SetFocusOnStartup(UIElement element, bool value)
        {
            element.SetValue(FocusOnStartupProperty, value);
        }

        private static void OnFocusOnStartupChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            var frameworkElement = (FrameworkElement)dependencyObject;
            if (!(e.NewValue is bool)) return;
            if (!(bool)e.NewValue) return;

            frameworkElement.Loaded += delegate
            {
                frameworkElement.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
                    new Action(delegate
                    {
                        Keyboard.Focus(frameworkElement);
                        var textBox = frameworkElement as TextBox;
                        if (textBox == null) return;

                        textBox.SelectAll();
                    }));
            };
        }
    }
}