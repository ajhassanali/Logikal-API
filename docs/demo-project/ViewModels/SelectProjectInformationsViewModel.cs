using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Shared;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class SelectProjectInformationsViewModel : ViewModelBase
    {
        public SelectionAdapter<IProjectInformationType> ProjectInformations { get; } =
            new SelectionAdapter<IProjectInformationType>();

        public SelectProjectInformationsViewModel(IEnumerable<IProjectInformationType> projectInformation)
        {
            ProjectInformations.ItemsSource = new ObservableCollection<IProjectInformationType>(projectInformation);
        }
    }
}
