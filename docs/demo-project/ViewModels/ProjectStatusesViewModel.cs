using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Ui;
using Ofcas.Lk.Api.Shared;
using System.Collections.Generic;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ProjectStatusesViewModel : ViewModelBase
    {
        public IList<IProjectStatus> ProjectStatuses { get; }

        public ProjectStatusesViewModel(IProjectCenterUi projectCenterUi)
        {
            ProjectStatuses = projectCenterUi.GetProjectStatuses().ProjectStatuses;
        }
    }
}
