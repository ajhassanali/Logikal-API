using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class ParameterObjectToStringConverter : OneWayValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "Not set";

            if (value is IList<IDictionary<string, object>> dictionaryList)
                return StringUtils.ToDetailedString(dictionaryList);

            return value.ToString();
        }
    }
}
