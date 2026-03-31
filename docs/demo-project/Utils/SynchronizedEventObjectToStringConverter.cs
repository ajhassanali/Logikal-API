using Ofcas.Lk.Api.Shared;
using System;
using System.Globalization;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class SynchronizedEventObjectToStringConverter : OneWayValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case WellKnownSynchronizedEventObject.Unknown:
                    return nameof(WellKnownSynchronizedEventObject.Unknown);

                case WellKnownSynchronizedEventObject.Document:
                    return nameof(WellKnownSynchronizedEventObject.Document);

                case WellKnownSynchronizedEventObject.LoginScope:
                    return nameof(WellKnownSynchronizedEventObject.LoginScope);

                case WellKnownSynchronizedEventObject.Elevation:
                    return nameof(WellKnownSynchronizedEventObject.Elevation);

                case WellKnownSynchronizedEventObject.ElevationInstance:
                    return nameof(WellKnownSynchronizedEventObject.ElevationInstance);

                case WellKnownSynchronizedEventObject.Phase:
                    return nameof(WellKnownSynchronizedEventObject.Phase);

                case WellKnownSynchronizedEventObject.Project:
                    return nameof(WellKnownSynchronizedEventObject.Project);

                case WellKnownSynchronizedEventObject.ProjectCenter:
                    return nameof(WellKnownSynchronizedEventObject.ProjectCenter);

                default:
                    return nameof(WellKnownSynchronizedEventObject.Unknown);
            }
        }
    }
}
