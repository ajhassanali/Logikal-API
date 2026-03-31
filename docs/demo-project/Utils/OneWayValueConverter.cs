using System;
using System.Globalization;
using System.Windows.Data;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public abstract class OneWayValueConverter : IValueConverter
    {
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This is a one-way converter.");
        }
    }
}