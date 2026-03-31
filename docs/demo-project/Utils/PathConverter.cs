using System;
using System.Globalization;
using System.Windows.Data;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    [ValueConversion(typeof(string), typeof(string))]
    public class PathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;

            if (path == null)
                return "null";
            if (string.IsNullOrEmpty(path))
                return "string.Empty";
            return path;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            switch (path)
            {
                case "null":
                    return null;

                case "string.Empty":
                    return string.Empty;

                default:
                    return path;
            }
        }
    }
}