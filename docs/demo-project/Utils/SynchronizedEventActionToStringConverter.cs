using Ofcas.Lk.Api.Shared;
using System;
using System.Globalization;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class SynchronizedEventActionToStringConverter : OneWayValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case WellKnownSynchronizedEventAction.Added:
                    return nameof(WellKnownSynchronizedEventAction.Added);
                case WellKnownSynchronizedEventAction.Changed:
                    return nameof(WellKnownSynchronizedEventAction.Changed);
                case WellKnownSynchronizedEventAction.Exception:
                    return nameof(WellKnownSynchronizedEventAction.Exception);
                case WellKnownSynchronizedEventAction.Removed:
                    return nameof(WellKnownSynchronizedEventAction.Removed);
                default:
                    return nameof(WellKnownSynchronizedEventAction.Unknown);
            }
        }
    }
}
