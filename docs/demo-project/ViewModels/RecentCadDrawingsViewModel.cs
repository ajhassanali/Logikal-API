using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Client.Ui;
using System.Collections.ObjectModel;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class RecentCadDrawingsViewModel : ParentViewModelBase
    {
        private readonly ILoginScopeUi _loginScopeUi;

        public ObservableCollection<FileDataModel> CadDrawings { get; }

        public RecentCadDrawingsViewModel(IViewProvider viewProvider, ILoginScopeUi loginScopeUi) : base(viewProvider)
        {
            Throw.IfNull(loginScopeUi, nameof(loginScopeUi));
            _loginScopeUi = loginScopeUi;

            var operationInfo = _loginScopeUi.CanGetRecentCadDrawings();
            if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanGetRecentCadDrawings))) return;

            CadDrawings = new ObservableCollection<FileDataModel>();
            foreach (var cadDrawing in _loginScopeUi.GetRecentCadDrawings().FileDataList)
                CadDrawings.Add(new FileDataModel
                {
                    Name = cadDrawing.Name,
                    Path = cadDrawing.Path,
                    LastChangedDate = UiTools.UnixToDateTime(cadDrawing.LastChangedDate)
                });
        }
    }
}
