using Ofcas.Lk.Api.Client.Core;
using System.Windows.Controls;

namespace Ofcas.Lk.Api.Client.Demo.Views
{
    public partial class SelectElevationsWindow
    {
        public SelectElevationsWindow()
        {
            InitializeComponent();
        }

        protected override ListView TargetListView => ElevationsList;

        protected override bool Filter(object obj, string filterText)
        {
            var elevationInfo = obj as IElevationInfo;
            if (elevationInfo == null)
                return false;

            if (string.IsNullOrEmpty(filterText))
                return true;

            return DoesContainFilterText(filterText, elevationInfo.Name);
        }
    }
}
