using Ofcas.Lk.Api.Client.Core;
using System.Windows.Controls;

namespace Ofcas.Lk.Api.Client.Demo.Views
{
    public partial class SelectPhasesWindow
    {
        public SelectPhasesWindow()
        {
            InitializeComponent();
        }

        protected override ListView TargetListView => PhasesList;

        protected override bool Filter(object obj, string filterText)
        {
            var phaseInfo = obj as IPhaseInfo;
            if (phaseInfo == null)
                return false;

            if (string.IsNullOrEmpty(filterText))
                return true;

            return DoesContainFilterText(filterText, phaseInfo.Name);
        }
    }
}
