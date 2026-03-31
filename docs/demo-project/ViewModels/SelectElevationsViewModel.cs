using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class SelectElevationsViewModel : ViewModelBase
    {
        public SelectionAdapter<IElevationInfo> Elevations { get; } = new SelectionAdapter<IElevationInfo>();

        public SelectElevationsViewModel(IProject project)
        {
            var elevationInfos = new List<IElevationInfo>();

            foreach (var phaseInfo in project.ChildrenInfos)
                using (var phase = project.GetChild(phaseInfo))
                {
                    elevationInfos.AddRange(phase.CoreObject.ChildrenInfos);
                }

            Elevations.ItemsSource = new ObservableCollection<IElevationInfo>(elevationInfos);
        }
    }
}
