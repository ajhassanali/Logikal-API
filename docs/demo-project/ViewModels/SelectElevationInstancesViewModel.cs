using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class SelectElevationInstancesViewModel : ViewModelBase
    {
        public SelectionAdapter<IElevationInstanceInfo> Instances { get; } = new SelectionAdapter<IElevationInstanceInfo>();

        public SelectElevationInstancesViewModel(IProject project)
        {
            var instanceInfos = new List<IElevationInstanceInfo>();
            foreach (var phaseInfo in project.ChildrenInfos)
                using (var phase = project.GetChild(phaseInfo))
                    foreach (var elevationInfo in phase.CoreObject.ChildrenInfos)
                        using (var elevation = phase.CoreObject.GetChild(elevationInfo))
                            instanceInfos.AddRange(elevation.CoreObject.ChildrenInfos);
            Instances.ItemsSource = new ObservableCollection<IElevationInstanceInfo>(instanceInfos);
        }

        public SelectElevationInstancesViewModel(IElevation elevation)
        {
            var instanceInfos = new List<IElevationInstanceInfo>();
            instanceInfos.AddRange(elevation.ChildrenInfos);
            Instances.ItemsSource = new ObservableCollection<IElevationInstanceInfo>(instanceInfos);
        }
    }
}
