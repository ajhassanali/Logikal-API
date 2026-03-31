using Ofcas.Lk.Api.Client.Demo.ViewModels;
using Ofcas.Lk.Api.Shared;
using System;
using System.IO;
using System.Windows.Media;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class ElevationModel
    {
        public Guid CoreObjectId { get; set; }
        public Guid Guid { get; set; }
        public Guid VersionGuid { get; set; }
        public string Name { get; set; }
        public IElevationProcessingStatus ProcessingStatus { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ChangedDate { get; set; }
        public string CreatedByUser { get; set; }
        public string ChangedByUser { get; set; }
        public string UValue { get; set; }
        public ImageSource Thumbnail { get; set; }
        public IElevationFinishedState State { get; set; }
        public IElementType ElementType { get; set; }
        public bool IsAlternative { get; set; }
        public int Type { get; set; }
        public string UserDescription { get; set; }
        public string ModelDescription { get; set; }
        public string AutomaticDescription { get; set; }
        public string SystemDescription { get; set; }
        public double Amount { get; set; }
        public string Unit { get; set; }
        public Stream RtfContent { get; set; }
        public bool IsStandardElevation { get; set; }
        public bool IsSubSectionElevation { get; set; }
        public bool IsTextElevation { get; set; }
        public bool IsMaterialElevation { get; set; }
        public bool IsBaseElementElevation { get; set; }
        public bool IsSegmentElevation { get; set; }
        public bool IsInsertionElevation { get; set; }
        public bool IsLayerElevation { get; set; }
        public Guid MainGuid { get; set; }
        public Guid ElementPricelistGuid { get; set; }
        public bool IsElementPricelistElevation { get; set; }
        public string SurfaceBaseName { get; set; }
        public string SurfaceBaseColors { get; set; }
        public string SurfaceFrameName { get; set; }
        public string SurfaceFrameColors { get; set; }
        public string Comment { get; set; }
        public IElevationWarningLevel WarningLevel { get; set; }
        public int ElevationWarningsCount { get; set; }
        public bool IsIncludedInEPD { get; set; }

        public ElevationViewModel Parent { get; }

        public ElevationModel(ElevationViewModel elevationViewModel)
        {
            Parent = elevationViewModel;
        }
    }
}
