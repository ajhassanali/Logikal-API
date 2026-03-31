using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Shared;
using System.Windows;
using System.Windows.Controls;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class ElevationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StandardElevation { get; set; }
        public DataTemplate UndefinedElevation { get; set; }
        public DataTemplate TextElevation { get; set; }
        public DataTemplate MaterialElevation { get; set; }
        public DataTemplate BaseElementElevation { get; set; }
        public DataTemplate SegmentElevation { get; set; }
        public DataTemplate InsertionElevation { get; set; }
        public DataTemplate LayerElevation { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var elevationModel = item as ElevationModel;
            if (elevationModel == null)
                return null;

            if (elevationModel.State.Id == WellKnownElevationFinishedState.EmptyCreated)
                return UndefinedElevation;

            if (elevationModel.IsTextElevation)
                return TextElevation;

            if (elevationModel.IsMaterialElevation)
                return MaterialElevation;

            if (elevationModel.IsBaseElementElevation)
                return BaseElementElevation;

            if (elevationModel.IsSegmentElevation)
                return SegmentElevation;

            if (elevationModel.IsInsertionElevation)
                return InsertionElevation;

            if (elevationModel.IsLayerElevation)
                return LayerElevation;

            return StandardElevation;
        }
    }
}
