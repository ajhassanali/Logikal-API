using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Client.Ui;
using Ofcas.Lk.Api.Shared;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class RecentProjectsViewModel : ParentViewModelBase
    {
        private readonly ILoginScopeUi _loginScopeUi;

        public ObservableCollection<IBaseProjectInfo> ProjectInfos { get; }
        public ICommand OpenProjectCommand { get; }

        public RecentProjectsViewModel(IViewProvider viewProvider, ILoginScopeUi loginScopeUi)
            : base(viewProvider)
        {
            Throw.IfNull(loginScopeUi, nameof(loginScopeUi));
            _loginScopeUi = loginScopeUi;

            var operationInfo = _loginScopeUi.CanGetRecentProjects(WellKnownProjectType.Project);
            if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScope.CanGetRecentProjects))) return;

            ProjectInfos = new ObservableCollection<IBaseProjectInfo>(_loginScopeUi.GetRecentProjects(WellKnownProjectType.Project).CoreInfos);

            OpenProjectCommand = new Command<IBaseProjectInfo>(OpenProject);
        }

        private void OpenProject(IBaseProjectInfo projectInfo)
        {
            CatchException(() =>
            {
                if (projectInfo == null) return;

                var project = ((ILoginScopeUi)_loginScopeUi).GetProject(projectInfo);
                var projectViewModel = ViewProvider.ViewModelFactory.GetProjectViewModel(project);
                ViewProvider.Show(projectViewModel);
            });
        }
    }
}
