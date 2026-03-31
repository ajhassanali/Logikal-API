using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Shared;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class ReorderChildModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var coreInfo = value as ICoreInfo;
            System.Enum.TryParse(parameter.ToString(), out ChildReorderBehavior childReorderBehavior);
            return new ReorderChildModel { TargetCoreInfo = coreInfo, ChildReorderBehavior = childReorderBehavior };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
