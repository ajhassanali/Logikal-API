using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Events;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ProjectViewModel : CoreObjectViewModel, IExtendedDisposable, IApplicationView, IViewModelWithChildren
    {
        private SynchronizationContainerViewModel _synchronizationContainerViewModel;
        private ICoreObjectResult<IProjectUi> _projectUiResult;
        protected ICoreObjectResult<IProjectUi> ProjectUiResult
        {
            get { return _projectUiResult; }
            set { _projectUiResult = value; }
        }
        protected IProjectUi ProjectUi => ProjectUiResult.CoreObject;

        private bool _coreObjectDisposed;
        public bool CoreObjectDisposed
        {
            get { return _coreObjectDisposed; }
            set { _coreObjectDisposed = value; OnPropertyChanged(); }
        }

        private ProjectModel _projectModel;
        public ProjectModel ProjectModel
        {
            get { return _projectModel; }
            set { _projectModel = value; OnPropertyChanged(); }
        }

        public SelectionAdapter<IPhaseInfo> Phases { get; } = new SelectionAdapter<IPhaseInfo>();

        public ICommand EditCommand { get; }
        public ICommand OpenPhaseCommand { get; }
        public ICommand CreatePhaseCommand { get; }
        public ICommand CreatePhaseWithParametersCommand { get; }
        public ICommand DeletePhaseCommand { get; }
        public ICommand OpenAddressContainerCommand { get; }
        public ICommand OpenDocumentsCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand PasteUiCommand { get; }
        public ICommand OpenSynchronizationContainerCommand { get; }
        public ICommand SelectGlassCommand { get; }
        public ICommand RunProgramCommand { get; }
        public ICommand UpdatePartsListsAsyncCommand { get; }
        public ICommand AddProjectCommand { get; }
        public ICommand ChangeElevationAssignmentsCommand { get; }
        public ICommand OpenAllPhasesCommand { get; }
        public ICommand OpenProjectInformationContainerCommand { get; }
        public ICommand ForceRefreshCommand { get; }
        public ICommand OpenInNewWindowCommand { get; }
        public ICommand OpenParentProjectCenterCommand { get; }
        public ICommand GetReportCommand { get; }
        public ICommand GetReportCommandCore { get; }
        public ICommand GetReportAsyncCommand { get; }
        public ICommand OpenReportCommand { get; }
        public ICommand OpenReportAsyncCommand { get; }
        public ICommand FindAndReplaceCommand { get; }
        public ICommand ReorderPhaseCommand { get; }
        public ICommand CopyPhaseForReorderCommand { get; }
        public ICommand ShowElevationsCommand { get; }
        public ICommand ShowElevationsAsyncCommand { get; }
        public ICommand CopyElevationsCommand { get; }
        public ICommand CopyElevationsFromProjectCommand { get; }
        public ICommand SelectElevationCommand { get; }
        public ICommand EditElevationsCommand { get; }
        public ICommand EditElevationsSingleValueCommand { get; }
        public ICommand ShowVersionGuidCommand { get; }
        public ICommand ShowEstimationDataSetCommand { get; }
        public ICommand SetEstimationDataSetCommand { get; }
        public ICommand ShowPhaseCollapsedCommand { get; }
        public ICommand ShowElevationCollapsedCommand { get; }
        public ICommand ShowElevationSelectedForReportsCommand { get; }
        public ICommand EditPhaseCollapsedCommand { get; }
        public ICommand EditElevationCollapsedCommand { get; }
        public ICommand EditElevationSelectedForReportsCommand { get; }

        public event EventHandler<EventArgs> ProjectRefreshed;

        public ProjectViewModel(IViewProvider viewProvider, ICoreObjectResult<IProjectUi> projectUi)
            : base(viewProvider, projectUi)
        {
            Throw.IfNull(projectUi, nameof(projectUi));
            ProjectUiResult = projectUi;

            ProjectUi.Disposed += OnProjectUiDisposed;

            Refresh(false, true);
            RefreshChildren();

            EditCommand = new Command<string>(Edit);
            OpenPhaseCommand = new Command<IPhaseInfo>(OpenPhase, CanOpenPhase);
            CreatePhaseCommand = new Command(CreatePhase);
            CreatePhaseWithParametersCommand = new Command(CreatePhaseWithParameters);
            DeletePhaseCommand = new Command<IPhaseInfo>(DeletePhase);
            OpenAddressContainerCommand = new Command(OpenAddressContainer);
            OpenDocumentsCommand = new Command(OpenDocuments);
            PasteCommand = new Command(Paste);
            PasteUiCommand = new Command(PasteUi);
            OpenSynchronizationContainerCommand = new Command(OpenSynchronizationContainer);
            SelectGlassCommand = new Command(SelectGlass);
            RunProgramCommand = new Command(RunProgram);
            UpdatePartsListsAsyncCommand = new AsyncCommand(UpdatePartsListsAsync);
            AddProjectCommand = new Command(AddProject);
            ChangeElevationAssignmentsCommand = new Command(ChangeElevationAssignments);
            OpenAllPhasesCommand = new Command(OpenAllPhases);
            OpenProjectInformationContainerCommand = new Command(OpenProjectInformationContainer);
            ForceRefreshCommand = new Command(ForceRefresh);
            OpenInNewWindowCommand = new Command(OpenInNewWindow);
            OpenParentProjectCenterCommand = new Command(OpenParentProjectCenter);
            GetReportCommand = new Command(GetReport);
            GetReportCommandCore = new Command(GetReportCore);
            GetReportAsyncCommand = new AsyncCommand(GetReportAsync);
            OpenReportAsyncCommand = new AsyncCommand(OpenReportAsync);
            FindAndReplaceCommand = new Command(FindAndReplace);
            ReorderPhaseCommand = new Command<ReorderChildModel>(ReorderPhase, CanReorderPhase);
            CopyPhaseForReorderCommand = new Command<IPhaseInfo>(CopyPhaseForReorder, CanCopyPhaseForReorder);
            ShowElevationsAsyncCommand = new AsyncCommand(ShowElevationsAsync);
            CopyElevationsCommand = new Command(CopyElevations);
            CopyElevationsFromProjectCommand = new Command(CopyElevationsFromProject);
            SelectElevationCommand = new Command(SelectElevation);
            EditElevationsCommand = new Command(EditElevations);
            EditElevationsSingleValueCommand = new Command(EditElevationsSingleValue);
            ShowVersionGuidCommand = new Command(ShowVersionGuid);
            ShowEstimationDataSetCommand = new Command(ShowEstimationDataSet);
            SetEstimationDataSetCommand = new Command(SetEstimationDataSet);
            ShowPhaseCollapsedCommand = new Command(ShowPhaseCollapsed);
            ShowElevationCollapsedCommand = new Command(ShowElevationCollapsed);
            ShowElevationSelectedForReportsCommand = new Command(ShowElevationSelectedForReports);
            EditPhaseCollapsedCommand = new Command(EditPhaseCollapsed);
            EditElevationCollapsedCommand = new Command(EditElevationCollapsed);
            EditElevationSelectedForReportsCommand = new Command(EditElevationSelectedForReports);
        }

        private void OnProjectUiDisposed(INotifyingDisposable obj)
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
                    case WellKnownEditKey.Project.Name:
                        var nameInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel($"Edit {title}",
                            ProjectUi.Info.Name);
                        if (!ShowDialog(nameInputBoxViewModel)) return;

                        value = nameInputBoxViewModel.GetValue();
                        break;

                    case WellKnownEditKey.Project.JobNumber:
                        var jobNumberInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel($"Edit {title}",
                            ProjectUi.Info.AsProjectInfo().JobNumber);
                        if (!ShowDialog(jobNumberInputBoxViewModel))  return;

                        value = jobNumberInputBoxViewModel.GetValue();
                        break;

                    case WellKnownEditKey.Project.OfferNumber:
                        var offerNumberInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel($"Edit {title}",
                            ProjectUi.Info.AsProjectInfo().OfferNumber);
                        if (!ShowDialog(offerNumberInputBoxViewModel)) return;

                        value = offerNumberInputBoxViewModel.GetValue();
                        break;

                    case WellKnownEditKey.Project.Status:
                        var getProjectStatusesOperationInfo = ProjectUi.Parent.CanGetProjectStatuses();
                        if (!getProjectStatusesOperationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanGetProjectStatuses))) return;

                        var projectStatuses = ProjectUi.Parent.GetProjectStatuses().ProjectStatuses;
                        var projectStatusParameter = new Parameter<IProjectStatus>
                        {
                            IsRequired = false,
                            Key = editKey,
                            Value = projectStatuses.FirstOrDefault(x => x.Id == ProjectUi.Info.AsFabricationLotInfo().Status.Id),
                            RestrictedValues = projectStatuses.ToList()
                        };

                        var projectStatusesParametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(
                            new ObservableCollection<IParameter>
                            {
                                projectStatusParameter
                            });

                        if (!ShowDialog(projectStatusesParametersViewModel))
                            return;

                        Dictionary<string, object> parameters = projectStatusesParametersViewModel.GetParameters();
                        parameters.TryGetValue(WellKnownEditKey.Project.Status, out value);
                        break;

                    default:
                        throw new ArgumentException($"The edit key '{editKey}' is not valid.");
                }

                var operationInfo = ProjectUi.CanEdit(editKey, value);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanEdit))) return;

                ProjectUi.Edit(editKey, value);
                Refresh();
            });
        }

        private void AddProject()
        {
            CatchException(() =>
            {
                var operationInfo = ProjectUi.CanLinkProject();
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanLinkProject))) return;

                var coreInfoResult = ProjectUi.LinkProject();
                if (coreInfoResult.OperationCode == OperationCode.Accepted)
                {
                    string message = "Add project was successful." +
                                     $"{Environment.NewLine}PhaseInfo: {coreInfoResult.CoreInfo.Guid:B}";
                    ShowMessage(message, nameof(AddProject));
                    RefreshChildren();
                }
                else
                {
                    ShowMessage("Add project was cancelled.");
                }
            });
        }

        private void ChangeElevationAssignments()
        {
            CatchException(() =>
            {
                var operationInfo = ProjectUi.CanChangeElevationAssignments();
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanChangeElevationAssignments))) return;

                var coreInfoResult = ProjectUi.ChangeElevationAssignments();
                if (coreInfoResult.OperationCode == OperationCode.Accepted)
                {
                    var message = "Change elevation Assignments was successful.";
                    if (coreInfoResult.CoreInfo != null)
                        message = message + $"{Environment.NewLine}PhaseInfo: {coreInfoResult.CoreInfo.Guid:B}";
                    ShowMessage(message, nameof(ChangeElevationAssignments));
                    RefreshChildren();
                }
                else
                {
                    ShowMessage("Add elevation was cancelled.");
                }
            });
        }

        private bool CanOpenPhase(IPhaseInfo phaseInfo)
        {
            return phaseInfo != null;
        }

        private void OpenPhase(IPhaseInfo phaseInfo)
        {
            CatchException(() =>
            {
                var operationInfo = ProjectUi.CanGetChild(phaseInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanGetChild))) return;

                var phase = ProjectUi.GetChild(phaseInfo);
                var viewModel = ViewProvider.ViewModelFactory.GetPhaseViewModel(phase);
                viewModel.PhaseRefreshed += OnPhaseRefreshed;
                ViewProvider.Show(viewModel);
            });
        }

        private void OpenAllPhases()
        {
            CatchException(() =>
            {
                var operationInfo = ProjectUi.CanGetChildren();
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanGetChildren))) return;

                var results = ProjectUi.GetChildren();
                if(results.OperationCode != OperationCode.Accepted)
                    return;

                foreach (var phaseResult in results.CoreObjectResults)
                {
                    var viewModel = ViewProvider.ViewModelFactory.GetPhaseViewModel(phaseResult);
                    viewModel.PhaseRefreshed += OnPhaseRefreshed;
                    ViewProvider.Show(viewModel);
                }
            });
        }

        private void CreatePhase()
        {
            CatchException(() =>
            {
                var operationInfo = ProjectUi.CanCreateChild();
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanCreateChild))) return;

                ProjectUi.CreateChild();
                RefreshChildren();
            });
        }

        private void CreatePhaseWithParameters()
        {
            CatchException(() =>
            {
                var parameterKeys = new ObservableCollection<IParameter>
                {
                    new Parameter<string>
                    {
                        Key = WellKnownEditKey.Phase.Name,
                        Value = string.Empty
                    },
                    new Parameter<string>
                    {
                        Key = WellKnownEditKey.Phase.Description,
                        Value = string.Empty
                    },
                };

                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(parameterKeys);

                if (!ViewProvider.ShowDialog(parametersViewModel, this).GetValueOrDefault())
                    return;

                Dictionary<string, object> parameters = parametersViewModel.GetParameters();
                var operationInfo = ProjectUi.CanCreateChild(parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanCreateChild))) return;

                ProjectUi.CreateChild(parameters);

                RefreshChildren();
            });
        }

        private void DeletePhase(IPhaseInfo phaseInfo)
        {
            CatchException(() =>
            {
                var operationInfo = ProjectUi.CanDeleteChild(phaseInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanDeleteChild))) return;

                ProjectUi.DeleteChild(phaseInfo);
                RefreshChildren();
            });
        }

        private void OpenAddressContainer()
        {
            CatchException(() =>
            {
                var addressContainer = ProjectUi.AddressContainer;
                var viewModel = new AddressContainerViewModel(ViewProvider, addressContainer);
                Show(viewModel);
            });
        }

        private void OpenDocuments()
        {
            CatchException(() =>
            {
                var documentContainer = ProjectUi.DocumentContainer;
                var viewModel = ViewProvider.ViewModelFactory.GetDocumentContainerViewModel(documentContainer);
                Show(viewModel);
            });
        }

        private void Paste()
        {
            CatchException(() =>
            {
                if (ClipboardHelper.Data == null)
                    throw new InvalidOperationException("Clipboard is empty.");

                var clipboardData = ClipboardHelper.Data as IClipboardDataWithContent<ICoreInfo>;
                if (clipboardData == null)
                    throw new InvalidOperationException(
                        $"Clipboard data is not valid. Clipboard data is of type '{ClipboardHelper.Data.GetType()}'.");

                var coreInfo = clipboardData.Content as ICoreInfoLinkable;

                var operationInfo = ((IProject)ProjectUi).CanCreateLink(coreInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanCreateLink))) return;

                var phaseInfo = ((IProject)ProjectUi).CreateLink(coreInfo).CoreInfo;
                var message = $"PhaseInfo: {phaseInfo.Guid:B}";
                ShowMessage(message, nameof(Paste));

                RefreshChildren();
            });
        }

        private void PasteUi()
        {
            CatchException(() =>
            {
                if (ClipboardHelper.Data == null)
                    throw new InvalidOperationException("Clipboard is empty.");

                var clipboardData = ClipboardHelper.Data as IClipboardDataWithContent<ICoreInfoLinkable>;
                if (clipboardData == null)
                    throw new InvalidOperationException(
                        $"Clipboard data is not valid. Clipboard data is of type '{ClipboardHelper.Data.GetType()}'.");

                var operationInfo = ProjectUi.CanCreateLink(clipboardData.Content);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanCreateLink))) return;

                var dialogResult = ProjectUi.CreateLink(clipboardData.Content);
                var message = $"DialogResult: {dialogResult.OperationCode}";
                if (dialogResult.OperationCode == OperationCode.Accepted)
                    message += $"{Environment.NewLine}PhaseInfo: {dialogResult.CoreInfo.Guid:B}";
                ShowMessage(message, nameof(PasteUi));

                RefreshChildren();
            });
        }

        private void OpenSynchronizationContainer()
        {
            CatchException(() =>
            {
                if (_synchronizationContainerViewModel != null)
                    return;

                _synchronizationContainerViewModel = ViewProvider.ViewModelFactory.GetSynchronizationContainerViewModel(ProjectUi);
                _synchronizationContainerViewModel.SynchronizedEventReceived +=
                    SynchronizationViewModelOnSynchronizedEventReceived;
                Show(_synchronizationContainerViewModel, delegate { OnSynchronizationContainerClosed(); });
            });
        }

        private void SelectGlass()
        {
            CatchException(() =>
            {
                var saveFileDialogViewModel = ViewProvider.ViewModelFactory.GetSaveFileDialogViewModel("XML Files | *.xml", "xml");
                if (!ShowDialog(saveFileDialogViewModel)) return;

                var streamResult = ProjectUi.SelectGlass();
                if (streamResult.OperationCode == OperationCode.Rejected) return;

                using (var glassStream = streamResult.Stream)
                using (var fileStream = File.Create(saveFileDialogViewModel.FileName))
                    glassStream.CopyTo(fileStream);
            });
        }

        private void RunProgram()
        {
            CatchException(() =>
            {
                var projectRunProgramViewModel = ViewProvider.ViewModelFactory.GetProjectProgramsViewModel(ProjectUi);
                Show(projectRunProgramViewModel, delegate { Refresh(true); });
            });
        }

        private void FindAndReplace()
        {
            CatchException(() =>
            {
                var selectElevationsViewModel = ViewProvider.ViewModelFactory.GetSelectElevationsViewModel(ProjectUi);
                if (!ShowDialog(selectElevationsViewModel))
                    return;

                var operationInfo = ProjectUi.CanFindAndReplace(selectElevationsViewModel.Elevations.SelectedItems);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanFindAndReplace))) return;

                ProjectUi.SetApplicationHandle(ViewProvider.GetHandle(this));
                ProjectUi.FindAndReplace(selectElevationsViewModel.Elevations.SelectedItems);
            });
        }

        private async Task UpdatePartsListsAsync()
        {
            await CatchException(async () =>
            {
                var selectElevationsViewModel = ViewProvider.ViewModelFactory.GetSelectElevationsViewModel(ProjectUi);
                if (!ShowDialog(selectElevationsViewModel)) return;

                var operationInfo = ProjectUi.CanBeginUpdatePartsLists(selectElevationsViewModel.Elevations.SelectedItems);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanBeginUpdatePartsLists))) return;

                var synchronizedOperation = ProjectUi.BeginUpdatePartsLists(selectElevationsViewModel.Elevations.SelectedItems).SynchronizedOperation;

                operationInfo = ProjectUi.CanEndUpdatePartsLists(synchronizedOperation);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanEndUpdatePartsLists))) return;

                await Task.Run(() => ProjectUi.EndUpdatePartsLists(synchronizedOperation)).ConfigureAwait(true);
            }).ConfigureAwait(true);
        }

        private void OpenProjectInformationContainer()
        {
            CatchException(() =>
            {
                var operationInfo = ProjectUi.CanGetProjectInformationTypes();
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanGetProjectInformationTypes))) return;

                var projectInformationTypesResult = ProjectUi.GetProjectInformationTypes();
                if(projectInformationTypesResult.OperationCode != OperationCode.Accepted) return;

                var selectProjectInformationsViewModel = ViewProvider.ViewModelFactory.GetSelectProjectInformationsViewModel(projectInformationTypesResult.ProjectInformationTypes);
                if (!ShowDialog(selectProjectInformationsViewModel)) return;

                var selectedProjectInformations = selectProjectInformationsViewModel.ProjectInformations.SelectedItems.ToList();

                foreach (var projectInformationType in selectedProjectInformations)
                {
                    operationInfo = ProjectUi.CanGetProjectInformationContainer(projectInformationType);
                    if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanGetProjectInformationContainer))) return;

                    var projectInformationContainerUi = ProjectUi.GetProjectInformationContainer(projectInformationType).CoreObject;
                    var viewModel = ViewProvider.ViewModelFactory
                        .GetProjectInformationContainerViewModel(projectInformationContainerUi, projectInformationType);
                    ViewProvider.Show(viewModel);
                }
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

        private void OpenInNewWindow()
        {
            CatchException(() =>
            {
                var loginScope = ProjectUi.Parent.LoginScope;
                var coreObjectFactory = loginScope.GetCoreObjectFactory().CoreObjectFactory;

                var project = coreObjectFactory.GetProject(ProjectUi.Info);
                var projectViewModel = ViewProvider.ViewModelFactory.GetProjectViewModel((ICoreObjectResult<IProjectUi>)project);
                Show(projectViewModel);
            });
        }

        private void ShowVersionGuid()
        {
            CatchException(() =>
            {
                var operationInfo = ProjectUi.CanGetVersionGuid();
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanGetVersionGuid))) return;

                var versionGuid = ProjectUi.GetVersionGuid().Guid;
                MessageBox.Show("Version gUID = "+versionGuid.ToString("B"));
            });
        }

        private void OpenParentProjectCenter()
        {
            CatchException(() =>
            {
                var loginScope = ProjectUi.Parent.LoginScope;
                var projectCenter = loginScope.GetProjectCenter(ProjectUi.Parent.Info);
                var projectCenterViewModel = ViewProvider.ViewModelFactory.GetProjectCenterViewModel(projectCenter);
                Show(projectCenterViewModel);
            });
        }

        private bool BeforeReport(bool isExport, out IReportItem reportItem, out IList<ICoreInfoReportable> infoReportables,
            out Dictionary<string, object> parameters)
        {
            reportItem = default(IReportItem);
            infoReportables = default(IList<ICoreInfoReportable>);
            parameters = default(Dictionary<string, object>);

            var operationInfo = ProjectUi.CanGetReports();
            if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanGetReports))) return false;

            var reportItemsResult = ProjectUi.GetReports();
            var selectReportsViewModel = ViewProvider.ViewModelFactory.GetSelectReportsViewModel(reportItemsResult.ReportItems);
            if (!ShowDialog(selectReportsViewModel))
                return false;

            reportItem = selectReportsViewModel.Reports.SelectedItem;
            if (reportItem == null)
                return false;

            var selectElevationsViewModel = ViewProvider.ViewModelFactory.GetSelectElevationsViewModel(ProjectUi);
            if (!ShowDialog(selectElevationsViewModel))
                return false;

            infoReportables = selectElevationsViewModel.Elevations.SelectedItems.Cast<ICoreInfoReportable>().ToList();

            var selectElevationInstancesViewModel = ViewProvider.ViewModelFactory.GetSelectElevationInstancesViewModel(ProjectUi);
            if (!ShowDialog(selectElevationInstancesViewModel))
                return false;

            var selectedElevationInstances = selectElevationInstancesViewModel.Instances.SelectedItems.ToList();
            foreach (var elevationInstanceInfo in selectedElevationInstances)
                infoReportables.Add(elevationInstanceInfo);

            var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(new ObservableCollection<IParameter>());

            if (isExport)
            {
                var exportParameters = GetExportParameters(reportItem);
                foreach (var parameter in exportParameters)
                    parametersViewModel.Parameters.Add(parameter);
            }

            if (!ShowDialog(parametersViewModel))
                return false;

            parameters = parametersViewModel.GetParameters();

            ProjectUi.SetApplicationHandle(ViewProvider.GetHandle(this));

            return true;
        }

        private void GetReport()
        {
            CatchException(() =>
            {
                if (!BeforeReport(true, out var reportItem, out var infoReportables, out var parameters))
                    return;

                var operationInfo = ProjectUi.CanGetReport(reportItem, infoReportables, parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanGetReport))) return;

                var dialogResult = ProjectUi.GetReport(reportItem, infoReportables, parameters);

                AfterGetReport(dialogResult, parameters);
            });
        }

        private void GetReportCore()
        {
            CatchException(() =>
            {
                if (!BeforeReport(true, out var reportItem, out var infoReportables, out var parameters))
                    return;

                var operationInfo = ((IProject)ProjectUi).CanGetReport(reportItem, infoReportables, parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanGetReport))) return;

                var stream = ((IProject)ProjectUi).GetReport(reportItem, infoReportables, parameters).Stream;

                AfterGetReport(stream, parameters);
            });
        }

        private Task GetReportAsync()
        {
            return CatchException(async () =>
            {
                if (!BeforeReport(true, out var reportItem, out var infoReportables, out var parameters))
                    return;

                var operationInfo = ProjectUi.CanBeginGetReport(reportItem, infoReportables, parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanBeginGetReport))) return;

                var synchronizedOperation = ProjectUi.BeginGetReport(reportItem, infoReportables, parameters).SynchronizedOperation;
                var streamResult = await Task.Run(() => ProjectUi.EndGetReport(synchronizedOperation)).ConfigureAwait(true);

                AfterGetReport(streamResult, parameters);
            });
        }

        private void AfterGetReport(Stream reportStream, Dictionary<string, object> parameters)
        {
            var displayName = parameters[WellKnownParameterKey.Project.Report.ExportFormat].ToString();
            var fileExtension = ExportFileFormat.GetFileExtension(displayName);
            var saveFileDialogViewModel = ViewProvider.ViewModelFactory.GetSaveFileDialogViewModel();
            saveFileDialogViewModel.AddFilter(fileExtension, true, displayName);
            saveFileDialogViewModel.FileName = "export";

            if (!ShowDialog(saveFileDialogViewModel))
                return;

            using (var fileStream = File.Create(saveFileDialogViewModel.FileName))
                reportStream.CopyTo(fileStream);
            ShowMessage("Get report was successful.");
        }

        private void AfterGetReport(IStreamResult streamResult, Dictionary<string, object> parameters)
        {
            if (streamResult.OperationCode == OperationCode.Rejected)
            {
                ShowMessage("Get report was cancelled.");
                return;
            }

            AfterGetReport(streamResult.Stream, parameters);
        }

        private Task OpenReportAsync()
        {
            return CatchException(async () =>
            {
                if (BeforeReport(false, out var reportItem, out var infoReportables, out var parameters))
                {
                    var operationInfo = ProjectUi.CanReport(reportItem, infoReportables, parameters);
                    if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanReport))) return;

                    var synchronizedOperation = ProjectUi.BeginReport(reportItem, infoReportables, parameters).SynchronizedOperation;
                    await Task.Run(() => ProjectUi.EndReport(synchronizedOperation)).ConfigureAwait(true);
                }
            });
        }

        private bool CanCopyPhaseForReorder(IPhaseInfo phaseInfo)
        {
            return phaseInfo != null;
        }

        private void CopyPhaseForReorder(IPhaseInfo phaseInfo)
        {
            CatchException(() =>
            {
                ClipboardHelper.Data = new ClipboardData<ProjectViewModel, IPhaseInfo>
                {
                    Sender = this,
                    Content = phaseInfo
                };
            });
        }

        private bool CanReorderPhase(ReorderChildModel reorderChildModel)
        {
            if (reorderChildModel?.TargetCoreInfo == null)
                return false;

            if (ClipboardHelper.Data == null)
                return false;

            var clipboardData = ClipboardHelper.Data as ClipboardData<ProjectViewModel, IPhaseInfo>;
            return clipboardData != null;
        }

        private void ReorderPhase(ReorderChildModel reorderChildModel)
        {
            CatchException(() =>
            {
                if (ClipboardHelper.Data == null)
                    throw new InvalidOperationException("Clipboard is empty.");

                var clipboardData = ClipboardHelper.Data as ClipboardData<ProjectViewModel, IPhaseInfo>;
                if (clipboardData == null)
                    throw new InvalidOperationException(
                        $"Clipboard data is not valid. Clipboard data is of type '{ClipboardHelper.Data.GetType()}'.");

                var sourcePhaseInfo = clipboardData.Content as IPhaseInfo;
                if (sourcePhaseInfo == null)
                    throw new InvalidOperationException($"Cannot place phase. " +
                                                        $"Invalid {nameof(ReorderChildModel.TargetCoreInfo)} of type '{clipboardData.Content.GetType()}'");

                var targetPhaseInfo = reorderChildModel.TargetCoreInfo as IPhaseInfo;
                if (targetPhaseInfo == null)
                    throw new InvalidOperationException($"Cannot place phase. " +
                                                        $"Invalid {nameof(ReorderChildModel.TargetCoreInfo)} of type '{reorderChildModel.TargetCoreInfo.GetType()}'");

                var operationInfo = ProjectUi.CanReorderChild(sourcePhaseInfo, targetPhaseInfo, reorderChildModel.ChildReorderBehavior);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanReorderChild))) return;

                ProjectUi.ReorderChild(sourcePhaseInfo, targetPhaseInfo, reorderChildModel.ChildReorderBehavior);
                RefreshChildren(true);
            });
        }

        private void CopyElevations()
        {
            CatchException(() =>
            {
                var selectElevationsViewModel = ViewProvider.ViewModelFactory.GetSelectElevationsViewModel(ProjectUi);
                if (!ShowDialog(selectElevationsViewModel))
                    return;

                var selectPhaseViewModel = ViewProvider.ViewModelFactory.GetSelectPhasesViewModel(ProjectUi);
                if (!ShowDialog(selectPhaseViewModel))
                    return;

                var selectedElevations = selectElevationsViewModel.Elevations.SelectedItems;
                var selectedPhase = selectPhaseViewModel.Phases.SelectedItem;

                var operationInfo = ProjectUi.CanCopyElevationsFrom(selectedElevations, selectedPhase);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanCopyElevationsFrom))) return;

                var coreInfoListResult = ProjectUi.CopyElevationsFrom(selectedElevations, selectedPhase);
                if (coreInfoListResult.OperationCode == OperationCode.Accepted)
                {
                    IPhaseInfo phaseInfo = coreInfoListResult.CoreInfos[0].ParentInfo;
                    if (ProjectUi.ChildrenInfos.All(x => x.Guid != phaseInfo.Guid))
                        RefreshChildren(true);
                    ShowMessage($"Accepted with phase {phaseInfo.Name}");
                }
                else
                    ShowMessage("Dialog rejected");
            });
        }

        private void CopyElevationsFromProject()
        {
            CatchException(() =>
            {
                var projectResult = ProjectUi.Parent.LoginScope.SelectProject();
                if (projectResult.OperationCode == OperationCode.Rejected)
                    return;

                var project = (IProjectUi)projectResult.CoreObject;
                var selectElevationsViewModel = ViewProvider.ViewModelFactory.GetSelectElevationsViewModel(project);
                if (!ShowDialog(selectElevationsViewModel))
                    return;

                var selectPhaseViewModel = ViewProvider.ViewModelFactory.GetSelectPhasesViewModel(ProjectUi);
                if (!ShowDialog(selectPhaseViewModel))
                    return;

                var selectedElevations = selectElevationsViewModel.Elevations.SelectedItems;
                var selectedPhase = selectPhaseViewModel.Phases.SelectedItem;

                var operationInfo = ProjectUi.CanCopyElevationsFrom(selectedElevations, selectedPhase);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanCopyElevationsFrom))) return;

                var coreInfoListResult = ProjectUi.CopyElevationsFrom(selectedElevations, selectedPhase);
                if (coreInfoListResult.OperationCode == OperationCode.Accepted)
                {
                    IPhaseInfo phaseInfo = coreInfoListResult.CoreInfos[0].ParentInfo;
                    RefreshChildren();
                    ShowMessage($"Accepted with phase {phaseInfo.Name}");
                }
                else
                    ShowMessage("Dialog rejected");
            });
        }

        private async Task ShowElevationsAsync()
        {
            await CatchException(async () =>
            {
                var selectElevationsViewModel = ViewProvider.ViewModelFactory.GetSelectElevationsViewModel(ProjectUi);
                if (!ShowDialog(selectElevationsViewModel))
                    return;

                var selectedElevations = selectElevationsViewModel.Elevations.SelectedItems;
                var elevationEditModes = ProjectUi.GetEditModes(selectedElevations).ElevationEditModes;
                var editModeModels = elevationEditModes.Select(editMode => new ElevationEditModeModel(editMode)).ToList();

                var selectEditModeViewModel =
                    ViewProvider.ViewModelFactory.GetSelectElevationEditModeViewModel(editModeModels);
                if (!ShowDialog(selectEditModeViewModel))
                    return;

                var selectedEditMode = selectEditModeViewModel.ElevationEditMode.Value?.EditMode;
                if (selectedEditMode == null)
                    return;

                var operationInfo = ProjectUi.CanBeginShow(selectedEditMode, selectedElevations);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanBeginShow))) return;

                var synchronizedOperation = ProjectUi.BeginShow(selectedEditMode, selectedElevations).SynchronizedOperation;
                var operationCode = await Task.Run(() => ProjectUi.EndShow(synchronizedOperation).OperationCode).ConfigureAwait(true);

                if (operationCode == OperationCode.Rejected)
                    ShowMessage("Dialog rejected");
            });
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

        private IEnumerable<IParameter> GetExportParameters(IReportItem reportItem)
        {
            var parameters = new List<IParameter>();
            var exportFormatParameter = new Parameter<string>
            {
                Key = WellKnownParameterKey.Project.Report.ExportFormat,
                IsRequired = true
            };
            parameters.Add(exportFormatParameter);

            exportFormatParameter.Value = ExportFileFormat.PDF.DisplayName;
            exportFormatParameter.RestrictedValues = new List<string>
            {
                ExportFileFormat.PDF.DisplayName, ExportFileFormat.SQLite.DisplayName,
                ExportFileFormat.DXF.DisplayName, ExportFileFormat.XLSX.DisplayName
            };

            if (reportItem.Id == WellKnownReports.Fabrication.ProjectPlan && reportItem.Category.Id == WellKnownReports.Fabrication.CategoryId)
            {
                exportFormatParameter.Value = ExportFileFormat.DXF.DisplayName;
            }

            if (reportItem.Id == WellKnownReports.Estimations.UValueExcelExport && reportItem.Category.Id == WellKnownReports.Estimations.CategoryId)
            {
                exportFormatParameter.Value = ExportFileFormat.XLSX.DisplayName;
            }

            if (reportItem.Id == WellKnownReports.Delivery.ErpExport && reportItem.Category.Id == WellKnownReports.Delivery.CategoryId)
            {
                exportFormatParameter.Value = ExportFileFormat.SQLite.DisplayName;
            }

            if ((reportItem.Id == WellKnownReports.Delivery.CadProjectPlan && reportItem.Category.Id == WellKnownReports.Delivery.CategoryId)
                || (reportItem.Id == WellKnownReports.Delivery.CadAssemblyList && reportItem.Category.Id == WellKnownReports.Delivery.CategoryId)
                || (reportItem.Id == WellKnownReports.Delivery.CadBarDrawings && reportItem.Category.Id == WellKnownReports.Delivery.CategoryId)
                || (reportItem.Id == WellKnownReports.Delivery.CadShapedGlass && reportItem.Category.Id == WellKnownReports.Delivery.CategoryId))
            {
                exportFormatParameter.Value = ExportFileFormat.DXF.DisplayName;
                exportFormatParameter.RestrictedValues = new List<string> { ExportFileFormat.DXF.DisplayName, ExportFileFormat.OCD.DisplayName };
                var dxfVersionParameter = new Parameter<DxfVersion>()
                {
                    Key = WellKnownParameterKey.Project.Report.DxfVersion,
                    Value = DxfVersion.R12
                };
                parameters.Add(dxfVersionParameter);
            }

            if (reportItem.Id == WellKnownReports.Delivery.ErpPricesExport && reportItem.Category.Id == WellKnownReports.Delivery.CategoryId)
            {
                exportFormatParameter.Value = ExportFileFormat.SQLite.DisplayName;
            }

            return parameters;
        }

        private void SelectElevation()
        {
            CatchException(() =>
            {
                var operationInfo = ProjectUi.CanSelectElevation();
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanSelectElevation))) return;

                var coreObjectResult = ProjectUi.SelectElevation();
                if (coreObjectResult.OperationCode == OperationCode.Rejected)
                    return;

                ElevationViewModel elevationViewModel = ViewProvider.ViewModelFactory.GetElevationViewModel(coreObjectResult);
                ViewProvider.Show(elevationViewModel);
            });
        }

        public void EditElevations()
        {
            CatchException(() =>
            {
                var nameInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel("Elevations Edit Key", "");
                if (!ShowDialog(nameInputBoxViewModel))
                    return;

                var key = nameInputBoxViewModel.GetValue();

                var selectElevationsViewModel = ViewProvider.ViewModelFactory.GetSelectElevationsViewModel(ProjectUi);
                if (!ShowDialog(selectElevationsViewModel))
                    return;

                var elevations = selectElevationsViewModel.Elevations.SelectedItems;
                if (elevations.Count == 0)
                    return;

                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(new ObservableCollection<IParameter>(), false);
                foreach (var elevation in elevations)
                {
                    parametersViewModel.Parameters.Add(new Parameter<String>
                    {
                        Key = elevation.Name,
                        IsRequired = true
                    });
                }
                if (!ShowDialog(parametersViewModel))
                    return;

                var parameters = parametersViewModel.GetParameters();

                ProjectUi.SetApplicationHandle(ViewProvider.GetHandle(this));

                var operationInfo = ProjectUi.CanEditElevations(key, elevations, parameters.Values);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.EditElevations))) return;

                ProjectUi.EditElevations(key, elevations, parameters.Values);
            });
        }

        public void EditElevationsSingleValue()
        {
            CatchException(() =>
            {
                var selectElevationsViewModel = ViewProvider.ViewModelFactory.GetSelectElevationsViewModel(ProjectUi);
                if (!ShowDialog(selectElevationsViewModel))
                    return;

                var elevations = selectElevationsViewModel.Elevations.SelectedItems;
                if (elevations.Count == 0)
                    return;

                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(new ObservableCollection<IParameter>(), true);
                if (!ShowDialog(parametersViewModel))
                    return;

                var parameters = parametersViewModel.GetParameters();
                if (parameters.Count != 1)
                    return;

                var key = parameters.ElementAt(0).Key;
                var value = parameters.ElementAt(0).Value;

                ProjectUi.SetApplicationHandle(ViewProvider.GetHandle(this));

                var operationInfo = ProjectUi.CanEditElevations(key, elevations, value);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.EditElevations))) return;

                ProjectUi.EditElevations(key, elevations, value);
            });
        }

        private void ShowEstimationDataSet()
        {
            CatchException(() =>
            {
                var operationInfo = ProjectUi.CanGetEstimationDataSet();
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanGetEstimationDataSet))) return;

                var estimationDataSetInfo = ProjectUi.GetEstimationDataSet().CoreInfo;
                var estimationDataSetViewModel = ViewProvider.ViewModelFactory.GetEstimationDataSetViewModel(estimationDataSetInfo);
                Show(estimationDataSetViewModel);
            });
        }

        private void SetEstimationDataSet()
        {
            CatchException(() =>
            {
                var estimationDataSetContainer = ProjectUi.Parent.LoginScope.EstimationDataSetContainer;
                var estimationDataSetContainerViewModel = ViewProvider.ViewModelFactory.GetEstimationDataSetContainerViewModel(estimationDataSetContainer);
                if (ShowDialog(estimationDataSetContainerViewModel))
                {
                    var estimationDataSetInfo = estimationDataSetContainerViewModel.EstimationDataSets.SelectedItem;

                    var operationInfo = ProjectUi.CanSetEstimationDataSet(estimationDataSetInfo);
                    if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanSetEstimationDataSet))) return;

                    ProjectUi.SetEstimationDataSet(estimationDataSetInfo);
                }
            });
        }

        private void SynchronizationViewModelOnSynchronizedEventReceived(object sender,
            SynchronizedEventReceivedEventArgs eventArgs)
        {
            CatchException(() =>
            {
                var synchronizedEvent = eventArgs.SynchronizedEvent;
                switch (synchronizedEvent.Object)
                {
                    case WellKnownSynchronizedEventObject.Phase:
                        RefreshChildren(true);
                        break;

                    case WellKnownSynchronizedEventObject.Project:
                        Refresh(true);
                        break;
                }

                ProjectUi.SynchronizationContainer.SetHandled(synchronizedEvent);
            });
        }

        private void OnSynchronizationContainerClosed()
        {
            _synchronizationContainerViewModel.SynchronizedEventReceived -= SynchronizationViewModelOnSynchronizedEventReceived;
            _synchronizationContainerViewModel = null;
        }

        private void OnPhaseRefreshed(object sender, EventArgs e)
        {
            RefreshChildren();
        }

        protected virtual void Refresh(bool hardRefresh = false, bool suppressEvent = false)
        {
            if (hardRefresh)
                ProjectUi.Refresh();

            ProjectModel = new ProjectModel
            {
                CoreObjectGuid = ProjectUi.Id,
                Guid = ProjectUi.Info.Guid,
                Name = ProjectUi.Info.Name,
                JobNumber = ProjectUi.Info.IsProjectInfo() ? ProjectUi.Info.AsProjectInfo().JobNumber : default(string),
                OfferNumber = ProjectUi.Info.IsProjectInfo() ? ProjectUi.Info.AsProjectInfo().OfferNumber : default(string),
                CreatedDate = ProjectUi.Info.CreatedDateTime,
                LastChangeDate = ProjectUi.Info.LastChangedDateTime,
                Type = ProjectUi.Parent.Info.Type.Id,
                IsCalculated = ProjectUi.Info.IsProjectInfo() ? ProjectUi.Info.AsProjectInfo().IsCalculated : default(bool),
                Status = ProjectUi.Info.IsFabricationLotInfo() ? ProjectUi.Info.AsFabricationLotInfo().Status : default(IProjectStatus),
                CustomerName = ProjectUi.Info.IsProjectInfo() ? ProjectUi.Info.AsProjectInfo().CustomerName : default(string),
                ProvisionDate = ProjectUi.Info.IsFabricationLotInfo() ? ProjectUi.Info.AsFabricationLotInfo().ProvisionDate : default(DateTime),
                FabricationLotNumber = ProjectUi.Info.IsFabricationLotInfo() ? ProjectUi.Info.AsFabricationLotInfo().FabricationLotNumber : default(string),
                IsProject = ProjectUi.Info.IsProjectInfo(),
                IsFabricationLot = ProjectUi.Info.IsFabricationLotInfo()
            };

            if (!suppressEvent)
                ProjectRefreshed?.Invoke(this, EventArgs.Empty);
        }

        public void RefreshChildren(bool hardRefresh = false)
        {
            // no need to refresh when view model was disposed already
            if (ProjectUi == null)
                return;

            if (hardRefresh)
                ProjectUi.RefreshChildren();

            Phases.Load(ProjectUi.ChildrenInfos);
        }

        public void OnLoaded()
        {
            ProjectUi.SetApplicationHandle(ViewProvider.GetHandle(this));
        }

        public bool CanDispose(out string falseReason)
        {
            falseReason = String.Empty;

            if (_synchronizationContainerViewModel == null)
            {
                if (ProjectUi.IsDisposed)
                    return true;
                return CanDispose(_projectUiResult, out falseReason);
            }

            falseReason = $"Synchronization [{ProjectUi.Id}]";
            return false;
        }

        public void Dispose()
        {
            ProjectUiResult.Disposed -= OnProjectUiDisposed;

            if (_projectUiResult == null || ProjectUi.IsDisposed) return;

            _projectUiResult.Dispose();
            _projectUiResult = null;
        }

        private void ShowPhaseCollapsed()
        {
            CatchException(() =>
            {
                var operationInfo = ProjectUi.CanGetCollapsed(ProjectUi.ChildrenInfos);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanGetCollapsed))) return;

                var values = ProjectUi.GetCollapsed(ProjectUi.ChildrenInfos).Values;

                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(new ObservableCollection<IParameter>(), false);
                foreach (var pair in values)
                {
                    parametersViewModel.Parameters.Add(new Parameter<bool>
                    {
                        Key = ((IPhaseInfo)pair.Key).Name,
                        IsRequired = true,
                        Value = pair.Value
                    });
                }
                ShowDialog(parametersViewModel);
            });
        }

        private void ShowElevationCollapsed()
        {
            CatchException(() =>
            {
                var elevationInfos = new List<IElevationInfo>();
                foreach (var phaseInfo in ProjectUi.ChildrenInfos)
                {
                    using (var phaseResult = ProjectUi.GetChild(phaseInfo))
                    {
                        elevationInfos.AddRange(phaseResult.CoreObject.ChildrenInfos);
                    }
                }

                var operationInfo = ProjectUi.CanGetCollapsed(elevationInfos);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanGetCollapsed))) return;

                var values = ProjectUi.GetCollapsed(elevationInfos).Values;

                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(new ObservableCollection<IParameter>(), false);
                foreach (var pair in values)
                {
                    parametersViewModel.Parameters.Add(new Parameter<bool>
                    {
                        Key = ((IElevationInfo)pair.Key).Name,
                        IsRequired = true,
                        Value = pair.Value
                    });
                }
                ShowDialog(parametersViewModel);
            });
        }

        private void ShowElevationSelectedForReports()
        {
            CatchException(() =>
            {
                var elevationInfos = new List<IElevationInfo>();
                foreach (var phaseInfo in ProjectUi.ChildrenInfos)
                {
                    using (var phaseResult = ProjectUi.GetChild(phaseInfo))
                    {
                        elevationInfos.AddRange(phaseResult.CoreObject.ChildrenInfos);
                    }
                }

                var operationInfo = ProjectUi.CanGetSelectedForReports(elevationInfos);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanGetSelectedForReports))) return;

                var values = ProjectUi.GetSelectedForReports(elevationInfos).Values;

                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(new ObservableCollection<IParameter>(), false);
                foreach (var pair in values)
                {
                    parametersViewModel.Parameters.Add(new Parameter<bool>
                    {
                        Key = ((IElevationInfo)pair.Key).Name,
                        IsRequired = true,
                        Value = pair.Value
                    });
                }
                ShowDialog(parametersViewModel);
            });
        }

        private void EditPhaseCollapsed()
        {
            CatchException(() =>
            {
                var selectPhasesViewModel = ViewProvider.ViewModelFactory.GetSelectPhasesViewModel(ProjectUi);
                if (!ShowDialog(selectPhasesViewModel))
                    return;

                if (selectPhasesViewModel.Phases.SelectedItems.Count == 0)
                    return;

                var inputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel("Edit Collapsed", false);
                if (!ShowDialog(inputBoxViewModel))
                    return;

                var operationInfo = ProjectUi.CanSetCollapsed(selectPhasesViewModel.Phases.SelectedItems, inputBoxViewModel.Value);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanSetCollapsed))) return;

                ProjectUi.SetCollapsed(selectPhasesViewModel.Phases.SelectedItems, inputBoxViewModel.Value);
            });
        }

        private void EditElevationCollapsed()
        {
            CatchException(() =>
            {
                var selectElevationsViewModel = ViewProvider.ViewModelFactory.GetSelectElevationsViewModel(ProjectUi);
                if (!ShowDialog(selectElevationsViewModel) || selectElevationsViewModel.Elevations.SelectedItems.Count == 0)
                    return;

                var inputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel("Edit Collapsed", false);
                if (!ShowDialog(inputBoxViewModel))
                    return;

                var operationInfo = ProjectUi.CanSetCollapsed(selectElevationsViewModel.Elevations.SelectedItems, inputBoxViewModel.Value);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanSetCollapsed))) return;

                ProjectUi.SetCollapsed(selectElevationsViewModel.Elevations.SelectedItems, inputBoxViewModel.Value);
            });
        }

        private void EditElevationSelectedForReports()
        {
            CatchException(() =>
            {
                var selectElevationsViewModel = ViewProvider.ViewModelFactory.GetSelectElevationsViewModel(ProjectUi);
                if (!ShowDialog(selectElevationsViewModel) || selectElevationsViewModel.Elevations.SelectedItems.Count == 0)
                    return;

                var inputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel("Edit SelectedForReports", false);
                if (!ShowDialog(inputBoxViewModel))
                    return;

                var operationInfo = ProjectUi.CanSetSelectedForReports(selectElevationsViewModel.Elevations.SelectedItems, inputBoxViewModel.Value);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProject.CanSetSelectedForReports))) return;

                ProjectUi.SetSelectedForReports(selectElevationsViewModel.Elevations.SelectedItems, inputBoxViewModel.Value);
            });
        }
    }
}
