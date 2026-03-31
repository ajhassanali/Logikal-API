using System;
using System.Globalization;
using System.Windows.Data;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    [ValueConversion(typeof(bool), typeof(object))]
    public class UnixToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ulong val)
                return UiTools.UnixToDateTime(val);
            else
                return UiTools.UnixToDateTime(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
