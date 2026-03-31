using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Client.Ui;
using Ofcas.Lk.Api.Shared;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ElevationHistoryViewModel : ParentViewModelBase, IApplicationView
    {
        private IElevationUi _elevationUi;

        public SelectionAdapter<IElevationInfo> History { get; } = new SelectionAdapter<IElevationInfo>();

        public ICommand EditElevationCommentCommand { get; }

        public ElevationHistoryViewModel(IViewProvider viewProvider, IElevationUi elevationUi) : base(viewProvider)
        {
            Throw.IfNull(elevationUi, nameof(elevationUi));
            _elevationUi = elevationUi;

            RefreshHistory();

            EditElevationCommentCommand = new Command<IElevationInfo>(EditElevationComment, CanEditElevationCommand);
        }

        public void OnLoaded()
        {
            _elevationUi.SetApplicationHandle(ViewProvider.GetHandle(this));
        }

        private void RefreshHistory()
        {
            History.ItemsSource = new ObservableCollection<IElevationInfo>();
            foreach (var elevationInfo in _elevationUi.GetInfoHistory().CoreInfos)
                History.ItemsSource.Add(elevationInfo);
        }

        private bool CanEditElevationCommand(IElevationInfo elevationInfo)
        {
            return elevationInfo != null;
        }

        private void EditElevationComment(IElevationInfo elevationInfo)
        {
            CatchException(() =>
            {
                var operationInfo = _elevationUi.CanEditComment(elevationInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevationUi.CanEditComment))) return;

                var elevationInfoResult = _elevationUi.EditComment(elevationInfo);

                if (elevationInfoResult.OperationCode == OperationCode.Accepted)
                {
                    ShowMessage($"new comment: {elevationInfoResult.CoreInfo.Comment}");
                    RefreshHistory();
                }
            });
        }
    }
}
