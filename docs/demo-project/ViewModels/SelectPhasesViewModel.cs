using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using System.Collections.ObjectModel;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class SelectPhasesViewModel : ViewModelBase
    {
        public SelectionAdapter<IPhaseInfo> Phases { get; } = new SelectionAdapter<IPhaseInfo>();

        public SelectPhasesViewModel(IProject project)
        {
            Phases.ItemsSource = new ObservableCollection<IPhaseInfo>(project.ChildrenInfos);
            if (Phases.ItemsSource.Count > 0)
                Phases.SelectedItem = Phases.ItemsSource[0];
        }
    }
}
