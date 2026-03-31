using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class StreamToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Stream stream)
                using (var streamReader = new StreamReader(stream))
                    return streamReader.ReadToEnd();

            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This is a one-way converter.");
        }
    }
}
