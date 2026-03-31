using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Core.Utils;
using Ofcas.Lk.Api.Client.Demo.Events;
using Ofcas.Lk.Api.Client.Demo.Exceptions;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Client.Ui;
using Ofcas.Lk.Api.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Ofcas.Lk.Api.Shared.Utils;
using Enum = Ofcas.Lk.Api.Client.Demo.Utils.Enum;
using System.Text;
using System.Threading.Tasks;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class PhaseViewModel : CoreObjectViewModel, IExtendedDisposable, IApplicationView, IViewModelWithChildren
    {
        private SynchronizationContainerViewModel _synchronizationContainerViewModel;
        private ICoreObjectResult<IPhaseUi> _phaseUiResult;
        private ICoreObjectResult<IPhaseUi> PhaseUiResult
        {
            get { return _phaseUiResult; }
            set { _phaseUiResult = value; }
        }

        protected IPhaseUi PhaseUi => PhaseUiResult?.CoreObject;

        private bool _coreObjectDisposed;
        public bool CoreObjectDisposed
        {
            get { return _coreObjectDisposed; }
            set { _coreObjectDisposed = value; OnPropertyChanged(); }
        }

        private PhaseModel _phaseModel;

        public PhaseModel PhaseModel
        {
            get { return _phaseModel; }
            set { _phaseModel = value; OnPropertyChanged(); }
        }

        public SelectionAdapter<IElevationInfo> Elevations { get; } = new SelectionAdapter<IElevationInfo>();

        public ICommand EditCommand { get; }
        public ICommand OpenElevationCommand { get; }
        public ICommand CreateElevationCommand { get; }
        public ICommand CreateElevationWithParametersCommand { get; }
        public ICommand CreateElevationWithParametersAndStreamCommand { get; }
        public AsyncCommand CreateElevationAsyncCommand { get; }
        public ICommand DeleteElevationCommand { get; }
        public ICommand DeleteElevationsCommand { get; }
        public ICommand ForceDeleteElevationCommand { get; }
        public ICommand ForceDeleteElevationsCommand { get; }
        public ICommand CopyElevationCommand { get; }
        public ICommand PasteElevationCommand { get; }
        public ICommand PasteElevationUiCommand { get; }
        public ICommand MoveElevationCommand { get; }
        public ICommand MoveElevationUiCommand { get; }
        public ICommand RestoreElevationCommand { get; }
        public ICommand OpenSynchronizationContainerCommand { get; }
        public ICommand OpenAllElevationsCommand { get; }
        public ICommand ForceRefreshCommand { get; }
        public ICommand OpenInNewWindowCommand { get; }
        public ICommand ReorderElevationCommand { get; }
        public ICommand SortElevationsCommand { get; }
        public ICommand ImportChildCommand { get; set; }
        public ICommand SplitElevationCommand { get; }

        public event EventHandler PhaseRefreshed;

        public PhaseViewModel(IViewProvider viewProvider, ICoreObjectResult<IPhaseUi> phaseUi)
            : base(viewProvider, phaseUi)
        {
            Throw.IfNull(phaseUi, nameof(phaseUi));
            PhaseUiResult = phaseUi;

            PhaseUi.Disposed += OnPhaseUiDisposed;

            Refresh(false, true);
            RefreshChildren();

            EditCommand = new Command<string>(Edit);
            OpenElevationCommand = new Command<IElevationInfo>(OpenElevation, CanOpenElevation);
            CreateElevationCommand = new Command(CreateElevation);
            CreateElevationWithParametersCommand = new Command(CreateElevationWithParameters);
            CreateElevationWithParametersAndStreamCommand = new Command(CreateElevationWithParametersAndStream);
            CreateElevationAsyncCommand = new AsyncCommand(CreateElevationAsync);
            DeleteElevationCommand = new Command<IElevationInfo>(DeleteElevation, CanDeleteElevation);
            DeleteElevationsCommand = new Command(DeleteElevations, CanDeleteElevations);
            ForceDeleteElevationCommand = new Command<IElevationInfo>(ForceDeleteElevation);
            ForceDeleteElevationsCommand = new Command(ForceDeleteElevations);
            CopyElevationCommand = new Command<ICoreInfo>(CopyElevation, CanCopyElevation);
            PasteElevationCommand = new Command(PasteElevation, CanPasteElevation);
            PasteElevationUiCommand = new Command(PasteElevationUi, CanPasteElevationUi);
            MoveElevationCommand = new Command(MoveElevation, CanMoveElevation);
            MoveElevationUiCommand = new Command(MoveElevationUi, CanMoveElevationUi);
            RestoreElevationCommand = new Command<IElevationInfo>(RestoreElevation, CanRestoreElevation);
            OpenSynchronizationContainerCommand = new Command(OpenSynchronizationContainer);
            OpenAllElevationsCommand = new Command(OpenAllElevations);
            ForceRefreshCommand = new Command(ForceRefresh);
            OpenInNewWindowCommand = new Command(OpenInNewWindow);
            ReorderElevationCommand = new Command<ReorderChildModel>(ReorderElevation, CanReorderElevation);
            SortElevationsCommand = new Command(SortElevations);
            ImportChildCommand = new Command(ImportChild);
            SplitElevationCommand = new Command<IElevationInfo>(SplitElevation);
        }

        private void OnPhaseUiDisposed(INotifyingDisposable obj)
        {
            CoreObjectDisposed = true;
        }

        private void Edit(string editKey)
        {
            CatchException(() =>
            {
                var title = $"Edit {editKey}";
                object value;
                switch (editKey)
                {
                    case WellKnownEditKey.Phase.Name:
                        var nameInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel(title, PhaseUi.Info.Name);
                        if (!ShowDialog(nameInputBoxViewModel)) return;

                        value = nameInputBoxViewModel.GetValue();
                        break;

                    case WellKnownEditKey.Phase.Description:
                        var descriptionInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel(title, PhaseUi.Info.Description);
                        if (!ShowDialog(descriptionInputBoxViewModel)) return;

                        value = descriptionInputBoxViewModel.GetValue();
                        break;

                    default:
                        throw new ArgumentException($"The edit key '{editKey}' is not valid.");
                }

                var operationInfo = PhaseUi.CanEdit(editKey, value);
                if (!operationInfo.CheckForAnyRestriction(nameof(IPhase.CanEdit))) return;

                PhaseUi.Edit(editKey, value);
                Refresh();
            });
        }

        private bool CanOpenElevation(IElevationInfo elevationInfo)
        {
            return elevationInfo != null;
        }

        private void ShowElevation(ICoreObjectResult<IElevationUi> elevation)
        {
            var elevationViewModel = ViewProvider.ViewModelFactory.GetElevationViewModel(elevation);
            elevationViewModel.ElevationRefreshed += OnElevationRefreshed;
            Show(elevationViewModel, viewModel =>
            {
                viewModel.ElevationRefreshed -= OnElevationRefreshed;
            });
        }

        private void OpenElevation(IElevationInfo elevationInfo)
        {
            CatchException(() =>
            {
                var operationInfo = PhaseUi.CanGetChild(elevationInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IPhase.CanGetChild))) return;

                var elevation = PhaseUi.GetChild(elevationInfo);
                ShowElevation(elevation);
            });
        }

        private void OpenAllElevations()
        {
            CatchException(() =>
            {
                var operationInfo = PhaseUi.CanGetChildren();
                if (!operationInfo.CheckForAnyRestriction(nameof(IPhase.CanGetChildren))) return;

                var results = PhaseUi.GetChildren();
                if (results.OperationCode != OperationCode.Accepted)
                    return;

                foreach (var elevationResult in results.CoreObjectResults)
                    ShowElevation(elevationResult);
            });
        }

        private void CreateElevation()
        {
            CatchException(() =>
            {
                var operationInfo = PhaseUi.CanCreateChild();
                if (!operationInfo.CheckForAnyRestriction(nameof(IPhase.CanCreateChild))) return;

                PhaseUi.CreateChild();

                RefreshChildren();
            });
        }

        private ObservableCollection<IParameter> CreateCommonParametersForElevationCreation()
        {
            return ParameterGenerator.CreateCommonParametersForElevationCreation(PhaseUi.Parent);
        }

        private Dictionary<string, object> ShowElevationCreationParameters(ObservableCollection<IParameter> parameterKeys)
        {
            var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(parameterKeys);
            if (!ViewProvider.ShowDialog(parametersViewModel, this).GetValueOrDefault())
                return null;

            Dictionary<string, object> parameters = parametersViewModel.GetParameters();
            if (parameters.TryGetValue(WellKnownEditKey.Elevation.ElementType, out var value))
                if (value is IElementType elementType)
                    parameters[WellKnownEditKey.Elevation.ElementType] = elementType.Id;

            return parameters;
        }

        private void CreateElevationWithParameters()
        {
            CatchException(() =>
            {
                ObservableCollection<IParameter> parameterKeys = CreateCommonParametersForElevationCreation();
                if (parameterKeys == null)
                    return;

                Dictionary<string, object> parameters = ShowElevationCreationParameters(parameterKeys);
                if (parameters == null) return;

                var operationInfo = PhaseUi.CanCreateChild(parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(IPhase.CanCreateChild))) return;

                PhaseUi.CreateChild(parameters);

                RefreshChildren();
            });
        }

        private void CreateElevationWithParametersAndStream()
        {
            const string streamParameter = "FILEPATH_STREAM";

            CatchException(() =>
            {
                var parameterKeys = CreateCommonParametersForElevationCreation();
                if (parameterKeys == null)
                    return;

                parameterKeys.Add(new Parameter<string>
                {
                    Key = streamParameter,
                    Value = string.Empty
                });

                Dictionary<string, object> parameters = ShowElevationCreationParameters(parameterKeys);
                if (parameters == null) return;

                var content = Stream.Null;
                if (parameters.HasParameterValue(streamParameter))
                {
                    var filePath = (string)parameters[streamParameter];
                    content = File.OpenRead(filePath);
                    parameters.Remove(streamParameter);
                }

                using (content)
                {
                    var operationInfo = PhaseUi.CanCreateChild(parameters, content);
                    if (!operationInfo.CheckForAnyRestriction(nameof(IPhase.CanCreateChild))) return;
                    PhaseUi.CreateChild(parameters, content);
                }

                RefreshChildren();
            });
        }

        private async Task CreateElevationAsync()
        {
            const string streamParameter = "FILEPATH_STREAM";

            await CatchException(async () =>
            {
                // parameters
                ObservableCollection<IParameter> parameterKeys = ParameterGenerator.CreateCommonParametersForElevationCreation(PhaseUi.Parent);
                if (parameterKeys == null)
                    return;

                parameterKeys.Add(new Parameter<string>
                {
                    Key = streamParameter,
                    Value = string.Empty
                });

                Dictionary<string, object> parameters = ShowElevationCreationParameters(parameterKeys);
                if (parameters == null) return;

                var content = Stream.Null;
                if (parameters.TryGetValue(streamParameter, out var streamFilePath) && !string.IsNullOrWhiteSpace((string)streamFilePath))
                {
                    content = File.OpenRead((string)streamFilePath);
                    parameters.Remove(streamParameter);
                }

                using(content) {
                    var operationInfo = PhaseUi.CanBeginCreateChild(parameters, content);
                    if (!operationInfo.CheckForAnyRestriction(nameof(IPhaseUi.CanBeginCreateChild))) return;

                    content.SetPosition(0);

                    var synchronizedOperation = PhaseUi.BeginCreateChild(parameters, content);
                    var dialogResult = await Task.Run(() => PhaseUi.EndCreateChild(synchronizedOperation)).ConfigureAwait(true);

                    if (dialogResult.OperationCode == OperationCode.Accepted)
                    {
                        RefreshChildren();
                        ShowMessage($"Dialog accepted");
                    }
                    else
                        ShowMessage("Dialog rejected");
                }
            });
        }

        private bool CanDeleteElevation(IElevationInfo elevationInfo)
        {
            return elevationInfo != null;
        }

        private bool CanDeleteElevations()
        {
            return true;
        }

        private void DeleteElevation(IElevationInfo elevationInfo)
        {
            CatchException(() =>
            {
                var operationInfo = PhaseUi.CanDeleteChild(elevationInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IPhase.CanDeleteChild))) return;
                PhaseUi.DeleteChild(elevationInfo);

                RefreshChildren();
            });
        }

        private void ForceDeleteElevation(IElevationInfo elevationInfo)
        {
            CatchException(() =>
            {
                var operationInfo = PhaseUi.CanForceDeleteChild(elevationInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IPhase.CanForceDeleteChild))) return;
                PhaseUi.ForceDeleteChild(elevationInfo);

                RefreshChildren();
            });
        }

        private void DeleteElevations()
        {
            DeleteElevations(false);
        }

        private void ForceDeleteElevations()
        {
            DeleteElevations(true);
        }

        private void DeleteElevations(bool forceDelete)
        {
            CatchException(() =>
            {
                var project = PhaseUi.Parent;
                var selectElevationsViewModel = ViewProvider.ViewModelFactory.GetSelectElevationsViewModel(project);
                if (!ShowDialog(selectElevationsViewModel))
                    return;

                if (selectElevationsViewModel.Elevations.SelectedItems.Count() == 0)
                    return;

                IOperationInfo operationInfo = forceDelete
                    ? PhaseUi.CanForceDeleteChildren(selectElevationsViewModel.Elevations.SelectedItems)
                    : PhaseUi.CanDeleteChildren(selectElevationsViewModel.Elevations.SelectedItems);
                if (!operationInfo.CheckForAnyRestriction(nameof(IPhase.CanForceDeleteChild))) return;

                IDeleteChildrenResult result = forceDelete
                    ? PhaseUi.ForceDeleteChildren(selectElevationsViewModel.Elevations.SelectedItems)
                    : PhaseUi.DeleteChildren(selectElevationsViewModel.Elevations.SelectedItems);
                StringBuilder message = new StringBuilder();
                message.AppendLine("Result of the delete operation:");
                message.AppendLine("Skipped children:");
                foreach (var skippedGuid in result.InRecycleBin)
                    message.AppendLine("\t" + skippedGuid.ToString("B"));

                message.AppendLine("Children in recycle bin : ");
                foreach (var recycleBinGuid in result.InRecycleBin)
                    message.AppendLine("\t" + recycleBinGuid.ToString("B"));

                message.AppendLine("Permanently deleted children: ");
                foreach (var permanentlyDeletedGuid in result.PermanentlyDeleted)
                    message.AppendLine("\t" + permanentlyDeletedGuid.ToString("B"));

                ShowMessage(message.ToString(), "Delete Children Result");
                RefreshChildren();
            });
        }

        private bool CanCopyElevation(ICoreInfo coreInfo)
        {
            return coreInfo != null;
        }

        private void CopyElevation(ICoreInfo coreInfo)
        {
            CatchException(() =>
            {
                ClipboardHelper.Data = new ClipboardData<PhaseViewModel, ICoreInfo>
                {
                    Sender = this,
                    Content = coreInfo
                };
            });
        }

        private bool CanPasteElevation()
        {
            return ClipboardHelper.Data != null;
        }

        private void PasteElevation()
        {
            CatchException(() =>
            {
                MoveOrCopy(PhaseUi.CanCopyFrom, ((IPhase)PhaseUi).CopyFrom);
            });
        }

        private bool CanPasteElevationUi()
        {
            return ClipboardHelper.Data != null;
        }

        private void PasteElevationUi()
        {
            CatchException(() =>
            {
                var parameterKeys = new ObservableCollection<IParameter>
                {
                    new Parameter<bool>
                    {
                        Key = WellKnownParameterKey.Phase.ShowCopyDialog,
                        Value = false
                    },
                };

                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(parameterKeys, false);
                if (!ViewProvider.ShowDialog(parametersViewModel, this).GetValueOrDefault())
                    return;

                Dictionary<string, object> parameters = parametersViewModel.GetParameters();

                var clipboardData = ClipboardHelper.Data as ClipboardData<PhaseViewModel, ICoreInfo>;
                if (clipboardData == null)
                    throw new InvalidOperationException(
                        $"Clipboard data is not valid. Clipboard data is of type '{ClipboardHelper.Data.GetType()}'.");

                var elevationInfo = (IElevationInfo)clipboardData.Content;
                var operationInfo = PhaseUi.CanCopyFrom(elevationInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IPhase.CanCopyFrom))) return;

                var newElevationInfo = PhaseUi.CopyFrom(elevationInfo, parameters);
                if (newElevationInfo == null)
                    throw new InfoIsNullException();

                clipboardData.Sender.RefreshChildren(true);
                RefreshChildren();
            });
        }

        private bool CanMoveElevation()
        {
            return ClipboardHelper.Data != null;
        }

        private void MoveElevation()
        {
            CatchException(() =>
            {
                MoveOrCopy(PhaseUi.CanMoveFrom, ((IPhase)PhaseUi).MoveFrom);
            });
        }

        private bool CanMoveElevationUi()
        {
            return ClipboardHelper.Data != null;
        }

        private void MoveElevationUi()
        {
            CatchException(() =>
            {
                MoveOrCopy(PhaseUi.CanMoveFrom, PhaseUi.MoveFrom);
            });
        }

        private void MoveOrCopy(Func<IElevationInfo, IOperationInfo> canExecuteOperation,
            Func<IElevationInfo, ICoreInfoResult<IElevationInfo>> operation)
        {
            CatchException(() =>
            {
                if (ClipboardHelper.Data == null)
                    throw new InvalidOperationException("Clipboard is empty.");

                var clipboardData = ClipboardHelper.Data as ClipboardData<PhaseViewModel, ICoreInfo>;
                if (clipboardData == null)
                    throw new InvalidOperationException(
                        $"Clipboard data is not valid. Clipboard data is of type '{ClipboardHelper.Data.GetType()}'.");

                var elevationInfo = (IElevationInfo)clipboardData.Content;
                var operationInfo = canExecuteOperation(elevationInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(canExecuteOperation))) return;

                var newElevationInfo = operation(elevationInfo);
                if (newElevationInfo == null)
                    throw new InfoIsNullException();

                clipboardData.Sender.RefreshChildren(true);
                RefreshChildren();
            });
        }

        private bool CanRestoreElevation(IElevationInfo elevationInfo)
        {
            return elevationInfo != null;
        }

        private void RestoreElevation(IElevationInfo elevationInfo)
        {
            CatchException(() =>
            {
                var newElevationInfo = PhaseUi.RestoreChild(elevationInfo);
                if (newElevationInfo == null)
                    throw new InfoIsNullException();

                RefreshChildren();
            });
        }

        private bool CanReorderElevation(ReorderChildModel reorderChildModel)
        {
            if (reorderChildModel == null || reorderChildModel.TargetCoreInfo == null)
                return false;

            if (ClipboardHelper.Data == null)
                return false;

            return ClipboardHelper.Data is ClipboardData<PhaseViewModel, IElevationInfo>;
        }

        private void ReorderElevation(ReorderChildModel reorderChildModel)
        {
            CatchException(() =>
            {
                if (ClipboardHelper.Data == null)
                    throw new InvalidOperationException("Clipboard is empty.");

                var clipboardData = ClipboardHelper.Data as ClipboardData<PhaseViewModel, IElevationInfo>;
                if (clipboardData == null)
                    throw new InvalidOperationException(
                        $"Clipboard data is not valid. Clipboard data is of type '{ClipboardHelper.Data.GetType()}'.");

                var sourceElevationInfo = clipboardData.Content as IElevationInfo;
                if (sourceElevationInfo == null)
                    throw new InvalidOperationException($"Cannot place elevation. " +
                        $"Invalid {nameof(ReorderChildModel.TargetCoreInfo)} of type '{sourceElevationInfo.GetType()}'");

                var targetElevationInfo = reorderChildModel.TargetCoreInfo as IElevationInfo;
                if (targetElevationInfo == null)
                    throw new InvalidOperationException($"Cannot place elevation. " +
                        $"Invalid {nameof(ReorderChildModel.TargetCoreInfo)} of type '{targetElevationInfo.GetType()}'");

                var operationInfo = PhaseUi.CanReorderChild(sourceElevationInfo, targetElevationInfo,
                    reorderChildModel.ChildReorderBehavior);
                if (!operationInfo.CheckForAnyRestriction(nameof(IPhase.CanReorderChild))) return;

                PhaseUi.ReorderChild(sourceElevationInfo, targetElevationInfo, reorderChildModel.ChildReorderBehavior);
                RefreshChildren(true);
            });
        }

        private void SortElevations()
        {
            CatchException(() =>
            {
                var selectValueViewModel = new SelectValueViewModel<SortChildrenBehavior>("Select reorder strategy",
                    SortChildrenBehavior.NameAscending, Enum.GetValues<SortChildrenBehavior>());

                if (ViewProvider.ShowDialog(selectValueViewModel, this).GetValueOrDefault())
                {
                    var operationInfo = PhaseUi.CanSortChildren(selectValueViewModel.Selected.Value);
                    if (!operationInfo.CheckForAnyRestriction(nameof(IPhase.SortChildren))) return;

                    PhaseUi.SortChildren(selectValueViewModel.Selected.Value);
                    RefreshChildren(true);
                }
            });
        }

        private void ImportChild()
        {
            CatchException(() =>
            {
                var operationInfo = PhaseUi.CanImportChild();
                if (!operationInfo.CheckForAnyRestriction(nameof(IPhaseUi.CanImportChild))) return;

                PhaseUi.ImportChild();
                RefreshChildren();
            });
        }

        private void SplitElevation(IElevationInfo elevationInfo)
        {
            CatchException(() =>
            {
                var operationInfo = PhaseUi.CanSplitChild(elevationInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IPhaseUi.CanSplitChild))) return;

                var result = PhaseUi.SplitChild(elevationInfo);
                if (result.OperationCode == OperationCode.Accepted)
                {
                    var message = result.OperationCode.ToString() + Environment.NewLine + result.CoreInfos.Count();
                    ShowMessage(message);
                    RefreshChildren();
                }
                else
                {
                    ShowMessage("Split elevation was cancelled.");
                }
            });
        }

        private void OpenSynchronizationContainer()
        {
            CatchException(() =>
            {
                if (_synchronizationContainerViewModel != null) return;

                _synchronizationContainerViewModel = ViewProvider.ViewModelFactory.GetSynchronizationContainerViewModel(PhaseUi);
                _synchronizationContainerViewModel.SynchronizedEventReceived += SynchronizationViewModelOnSynchronizedEventReceived;
                ViewProvider.Show(_synchronizationContainerViewModel, delegate { OnSynchronizationContainerClosed(); });
            });
        }

        private void ForceRefresh()
        {
            CatchException(() =>
            {
                Refresh(true);
                RefreshChildren(true);
            });
        }

        public void OpenInNewWindow()
        {
            CatchException(() =>
            {
                var loginScope = PhaseUi.Parent.Parent.LoginScope;
                var coreObjectFactory = loginScope.GetCoreObjectFactory().CoreObjectFactory;

                var phase = coreObjectFactory.GetPhase(PhaseUi.Info);
                var phaseViewModel = ViewProvider.ViewModelFactory.GetPhaseViewModel((ICoreObjectResult<IPhaseUi>)phase);
                Show(phaseViewModel);
            });
        }

        private void SynchronizationViewModelOnSynchronizedEventReceived(object sender, SynchronizedEventReceivedEventArgs eventArgs)
        {
            CatchException(() =>
            {
                var synchronizedEvent = eventArgs.SynchronizedEvent;
                switch (synchronizedEvent.Object)
                {
                    case WellKnownSynchronizedEventObject.Elevation:
                        RefreshChildren(true);
                        break;

                    case WellKnownSynchronizedEventObject.Phase:
                    {
                        switch (synchronizedEvent.ActionKey)
                        {
                            case WellKnownActionKey.Phase.ElevationMoved:
                                RefreshChildren(true);
                                break;
                            default:
                                Refresh(true);
                                break;
                        }
                        break;
                    }
                }

                PhaseUi.SynchronizationContainer.SetHandled(synchronizedEvent);
            });
        }

        private void OnSynchronizationContainerClosed()
        {
            _synchronizationContainerViewModel.SynchronizedEventReceived -= SynchronizationViewModelOnSynchronizedEventReceived;
            _synchronizationContainerViewModel = null;
        }

        private void OnElevationRefreshed(object sender, EventArgs eventArgs)
        {
            RefreshChildren();
        }

        private void Refresh(bool hardRefresh = false, bool suppressEvent = false)
        {
            if (PhaseUi == null)
                return;

            if (hardRefresh)
                PhaseUi.Refresh();

            PhaseModel = new PhaseModel
            {
                CoreObjectId = PhaseUi.Id,
                Guid = PhaseUi.Info.Guid,
                Name = PhaseUi.Info.Name,
                Description = PhaseUi.Info.Description,
                Type = PhaseUi.Parent.Parent.Info.Type.Id
            };

            if (!suppressEvent)
                PhaseRefreshed?.Invoke(this, EventArgs.Empty);
        }

        public void RefreshChildren(bool hardRefresh = false)
        {
            // no need to refresh when view model was disposed already
            if (PhaseUi == null)
                return;
 
            if (hardRefresh)
                PhaseUi.RefreshChildren();

            Elevations.ItemsSource = new ObservableCollection<IElevationInfo>(PhaseUi.ChildrenInfos);
        }

        public void OnLoaded()
        {
            PhaseUi.SetApplicationHandle(ViewProvider.GetHandle(this));
        }

        public bool CanDispose(out string falseReason)
        {
            if (_synchronizationContainerViewModel == null)
                return CanDispose(_phaseUiResult, out falseReason);

            falseReason = _synchronizationContainerViewModel.Title;
            return false;
        }

        public void Dispose()
        {
            if (_phaseUiResult == null) return;

            _phaseUiResult.Dispose();
            _phaseUiResult = null;
        }
    }
}
