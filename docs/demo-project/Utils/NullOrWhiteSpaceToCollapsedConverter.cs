using System;
using System.Globalization;
using System.Windows;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class NullOrWhiteSpaceToCollapsedConverter : OneWayValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace((string)value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
