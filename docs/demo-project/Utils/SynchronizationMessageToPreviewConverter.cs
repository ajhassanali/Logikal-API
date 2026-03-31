using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Shared;
using System;
using System.Globalization;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class SynchronizationMessageToPreviewConverter : OneWayValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ISearchResult searchResult)
            {
                if (searchResult.IsProjectInfo())
                {
                    var projectInfo = searchResult.AsProjectInfo();
                    switch (projectInfo)
                    {
                        case IProjectInfo ProjectInfo:
                            return $"{nameof(WellKnownCoreObjectType.Project)} {ProjectInfo.Guid} - {ProjectInfo.Name} - {ProjectInfo.JobNumber} - " +
       $"{ProjectInfo.OfferNumber}";

                        case IFabricationLotInfo fabricationLotInfo:
                            return $"{nameof(WellKnownCoreObjectType.Project)} {fabricationLotInfo.Guid} - {fabricationLotInfo.Name} - {fabricationLotInfo.FabricationLotNumber}";
                    }
                }

                return "<< unavailable >>";
            }

            return string.Empty;
        }
    }
}
