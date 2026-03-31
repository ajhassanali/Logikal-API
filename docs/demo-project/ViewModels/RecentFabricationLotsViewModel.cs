using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Client.Ui;
using Ofcas.Lk.Api.Shared;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class RecentFabricationLotsViewModel : ParentViewModelBase
    {
        private readonly ILoginScopeUi _loginScopeUi;

        public ObservableCollection<IBaseProjectInfo> FabricationLotInfos { get; }
        public ICommand OpenFabricationLotCommand { get; }

        public RecentFabricationLotsViewModel(IViewProvider viewProvider, ILoginScopeUi loginScopeUi)
            : base(viewProvider)
        {
            Throw.IfNull(loginScopeUi, nameof(loginScopeUi));
            _loginScopeUi = loginScopeUi;

            var operationInfo = _loginScopeUi.CanGetRecentProjects(WellKnownProjectType.FabricationLot);
            if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScope.CanGetRecentProjects))) return;

            FabricationLotInfos = new ObservableCollection<IBaseProjectInfo>(_loginScopeUi.GetRecentProjects(WellKnownProjectType.FabricationLot).CoreInfos);

            OpenFabricationLotCommand = new Command<IBaseProjectInfo>(OpenFabricationLot);
        }

        private void OpenFabricationLot(IBaseProjectInfo projectInfo)
        {
            CatchException(() =>
            {
                if (projectInfo == null) return;

                var project = _loginScopeUi.GetProject(projectInfo);
                var projectViewModel = ViewProvider.ViewModelFactory.GetProjectViewModel(project);
                ViewProvider.Show(projectViewModel);
            });
        }
    }
}
