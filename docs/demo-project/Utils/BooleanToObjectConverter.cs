using System;
using System.Globalization;
using System.Windows.Data;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    [ValueConversion(typeof(bool), typeof(object))]
    public class BooleanToObjectConverter : IValueConverter
    {
        public object FalseValue { get; set; }
        public object TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter.Equals("!"))
                return (bool)value ? FalseValue : TrueValue;

            return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter.Equals("!"))
                return Equals(value, FalseValue);

            return Equals(value, TrueValue);
        }
    }
}