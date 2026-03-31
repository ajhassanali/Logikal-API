using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Events;
using Ofcas.Lk.Api.Client.Demo.Exceptions;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Client.Ui;
using Ofcas.Lk.Api.Shared;
using IBaseProjectInfo = Ofcas.Lk.Api.Client.Core.IBaseProjectInfo;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ProjectCenterViewModel : CoreObjectViewModel, IExtendedDisposable, IApplicationView, IViewModelWithChildren
    {
        private const string ExportTypeLob = "LOB";
        private const string ExportTypeL2B = "L2B";

        private SynchronizationContainerViewModel _synchronizationContainerProjectViewModel;
        private SynchronizationContainerViewModel _synchronizationContainerProjectCenterViewModel;

        protected ICoreObjectResult<IProjectCenterUi> ProjectCenterUiResult { get; set; }
        protected IProjectCenterUi ProjectCenterUi => ProjectCenterUiResult.CoreObject;
        protected IProjectCenterUi _projectCenterUi => ProjectCenterUiResult.CoreObject;

        private bool _coreObjectDisposed;
        public bool CoreObjectDisposed
        {
            get { return _coreObjectDisposed; }
            set { _coreObjectDisposed = value; OnPropertyChanged(); }
        }

        private ProjectCenterModel _projectCenterModel = new ProjectCenterModel();
        public ProjectCenterModel ProjectCenterModel
        {
            get { return _projectCenterModel; }
            set { _projectCenterModel = value; OnPropertyChanged(); }
        }

        public SelectionAdapter<IBaseProjectInfo> Projects { get; } = new SelectionAdapter<IBaseProjectInfo>();
        public SelectionAdapter<IProjectCenterInfo> ProjectCenters { get; } = new SelectionAdapter<IProjectCenterInfo>();

        public ICommand OpenProjectCommand { get; }
        public ICommand OpenProjectCenterCommand { get; }
        public ICommand CreateProjectUiCommand { get; }
        public ICommand CreateProjectWithParametersCommand { get; set; }
        public ICommand CreateProjectSilentCommand { get; }
        public ICommand ImportProjectCommand { get; }
        public ICommand DeleteProjectCommand { get; }
        public ICommand DeleteProjectUiCommand { get; }
        public ICommand ForceDeleteProjectCommand { get; }
        public ICommand ForceDeleteProjectUiCommand { get; }
        public ICommand DeleteProjectsCommand { get; }
        public ICommand DeleteProjectsUiCommand { get; }
        public ICommand ForceDeleteProjectsCommand { get; }
        public ICommand ForceDeleteProjectsUiCommand { get; }

        public ICommand CopyProjectCommand { get; }
        public ICommand PasteProjectCommand { get; }
        public ICommand MoveProjectCommand { get; }
        public ICommand CreateProjectCenterCommand { get; }
        public ICommand DeleteProjectCenterCommand { get; }
        public ICommand CopyProjectCenterCommand { get; }
        public ICommand MoveProjectCenterCommand { get; }
        public ICommand ExportChildCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand OpenInNewWindowCommand { get; }
        public ICommand ExportChildWithUiCommand { get; }
        public ICommand ShowProjectStatusesCommand { get; }
        public ICommand ForceRefreshCommand { get; }
        public ICommand ForceRefreshProjectsCommand { get; }
        public ICommand ForceRefreshProjectCentersCommand { get; }
        public ICommand ExportChildrenCommand { get; }
        public ICommand OpenProjectSynchronizationContainerCommand { get; }
        public ICommand OpenProjectCenterSynchronizationContainerCommand { get; }
        public ICommand OpenAllProjectCentersCommand { get; }


        public event EventHandler ProjectCenterRefreshed;

        public ProjectCenterViewModel(IViewProvider viewProvider, ICoreObjectResult<IProjectCenterUi> projectCenterUiResult) : base(viewProvider, projectCenterUiResult)
        {
            Throw.IfNull(projectCenterUiResult, nameof(projectCenterUiResult));
            ProjectCenterUiResult = projectCenterUiResult;

            ProjectCenterUi.Disposed += OnProjectCenterUiDisposed;

            Refresh();
            RefreshChildren();

            EditCommand = new Command<string>(Edit);
            OpenProjectCommand = new Command<IBaseProjectInfo>(OpenProject);
            OpenProjectCenterCommand = new Command<IProjectCenterInfo>(OpenProjectCenter);
            CreateProjectUiCommand = new Command(CreateProjectUi);
            CreateProjectWithParametersCommand = new Command(CreateProjectWithParameters);
            CreateProjectSilentCommand = new Command(CreateProjectSilent);
            ImportProjectCommand = new Command(ImportProject);
            DeleteProjectCommand = new Command<IBaseProjectInfo>(DeleteProject, o => HasSelectedProject());
            DeleteProjectUiCommand = new Command<IBaseProjectInfo>(DeleteProjectUi, o => HasSelectedProject());
            DeleteProjectsCommand = new Command(DeleteProjects, () => HasSelectedProjects());
            DeleteProjectsUiCommand = new Command(DeleteProjectsUi, () => HasSelectedProjects());
            ForceDeleteProjectCommand = new Command<IBaseProjectInfo>(ForceDeleteProject, o => HasSelectedProject());
            ForceDeleteProjectUiCommand = new Command<IBaseProjectInfo>(ForceDeleteProjectUi, o => HasSelectedProject());
            ForceDeleteProjectsCommand = new Command(ForceDeleteProjects, () => HasSelectedProjects());
            ForceDeleteProjectsUiCommand = new Command(ForceDeleteProjectsUi, () => HasSelectedProjects());
            CopyProjectCommand = new Command<ICoreInfo>(CopyProject, o => HasSelectedProject());
            PasteProjectCommand = new Command(PasteProject, () => ClipboardHelper.Data != null);
            MoveProjectCommand = new Command(MoveProject, () => ClipboardHelper.Data != null);
            CreateProjectCenterCommand = new Command(CreateProjectCenter);
            DeleteProjectCenterCommand = new Command<IProjectCenterInfo>(DeleteProjectCenter);
            CopyProjectCenterCommand = new Command(CopyProjectCenter, CanCopyProjectCenter);
            MoveProjectCenterCommand = new Command(MoveProjectCenter, CanMoveProjectCenter);
            ExportChildCommand = new Command<IBaseProjectInfo>(ExportChild, o => HasSelectedProject());
            OpenInNewWindowCommand = new Command(OpenInNewWindow);
            ExportChildWithUiCommand = new Command<IBaseProjectInfo>(ExportChildWithUi, o => HasSelectedProject());
            ShowProjectStatusesCommand = new Command(ShowProjectStatuses);
            ForceRefreshCommand = new Command(ForceRefresh);
            ForceRefreshProjectsCommand = new Command(ForceRefreshProjects);
            ForceRefreshProjectCentersCommand = new Command(ForceRefreshProjectCenters);
            ExportChildrenCommand = new Command(ExportChildren);
            OpenProjectSynchronizationContainerCommand = new Command(OpenProjectSynchronizationContainer);
            OpenProjectCenterSynchronizationContainerCommand = new Command(OpenProjectCenterSynchronizationContainer);
            OpenAllProjectCentersCommand = new Command(OpenAllProjectCenters);
        }

        private void OnProjectCenterUiDisposed(INotifyingDisposable obj)
        {
            CoreObjectDisposed = true;
        }

        private void OpenAllProjectCenters()
        {
            _ = CatchException(() =>
              {
                  var operationInfo = _projectCenterUi.ProjectCenterContainer.CanGetChildren();
                  if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenterContainer.CanGetChildren))) return;

                  var projectCenters = _projectCenterUi.ProjectCenterContainer.GetChildren().CoreObjectResults;
                  foreach (var projectCenter in projectCenters)
                  {
                      var projectCenterViewModel = ViewProvider.ViewModelFactory.GetProjectCenterViewModel(projectCenter);
                      projectCenterViewModel.ProjectCenterRefreshed += OnProjectCenterRefreshed;
                      Show(projectCenterViewModel, x => x.ProjectCenterRefreshed -= OnProjectCenterRefreshed);
                  }
              });
        }

        protected bool HasSelectedProject()
        {
            return Projects.SelectedItem != null;
        }

        protected bool HasSelectedProjects()
        {
            return Projects.SelectedItems != null;
        }

        private void Edit(string editKey)
        {
            CatchException(() =>
            {
                switch (editKey)
                {
                    case WellKnownEditKey.ProjectCenter.DIRECTORY_NAME:
                        var inputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel(
                            "Rename project center (set a new directory name)",
                            _projectCenterUi.Info.DirectoryName);
                        if (!ShowDialog(inputBoxViewModel)) return;

                        var value = inputBoxViewModel.GetValue();

                        var operationInfo = _projectCenterUi.CanEdit(editKey, value);
                        if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanEdit))) return;

                        _projectCenterUi.Edit(editKey, value);
                        break;

                    default:
                        throw new ArgumentException($"The edit key '{editKey}' is not valid.");
                }

                Refresh();
                RefreshChildrenProjectCenters();
            });
        }

        private void OpenProject(IBaseProjectInfo projectInfo)
        {
            CatchException(() =>
            {
                if (projectInfo == null) return;

                var operationInfo = _projectCenterUi.CanGetChild(projectInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanGetChild))) return;

                var project = _projectCenterUi.GetChild(projectInfo);
                var projectViewModel = ViewProvider.ViewModelFactory.GetProjectViewModel(project);
                projectViewModel.ProjectRefreshed += OnProjectRefreshed;
                Show(projectViewModel);
            });
        }

        private void OpenProjectCenter(IProjectCenterInfo projectCenterInfo)
        {
            CatchException(() =>
            {
                if (projectCenterInfo == null) return;

                var operationInfo = _projectCenterUi.ProjectCenterContainer.CanGetChild(projectCenterInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenterContainer.CanGetChild))) return;

                var projectCenter = _projectCenterUi.ProjectCenterContainer.GetChild(projectCenterInfo);

                var projectCenterViewModel = ViewProvider.ViewModelFactory.GetProjectCenterViewModel(projectCenter);
                projectCenterViewModel.ProjectCenterRefreshed += OnProjectCenterRefreshed;
                Show(projectCenterViewModel, x => x.ProjectCenterRefreshed -= OnProjectCenterRefreshed);
            });
        }

        private void ImportProject()
        {
            CatchException(() =>
            {
                var createChildParameter = new ObservableCollection<IParameter>()
                {
                    new Parameter<Guid>
                    {
                        Key = WellKnownParameterKey.ProjectCenter.CreateChild.ProjectGuid,
                        Value = Guid.Empty
                    }
                };
                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(createChildParameter, true);
                if (!ShowDialog(parametersViewModel))
                    return;
                var parameters = parametersViewModel.GetParameters();

                var viewModel = ViewProvider.ViewModelFactory.GetOpenFileDialogViewModel();
                viewModel.Filter = "Import files|*.xml;*.b2l;*.sab;*.lob|All files|*.*";
                if (!ShowDialog(viewModel))
                    return;

                ICoreInfoResult<IBaseProjectInfo> coreInfoResult;
                using (var fileStream = File.OpenRead(viewModel.FileName))
                {
                    var operationInfo = _projectCenterUi.CanImportChild(fileStream, parameters);
                    if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenterUi.CanImportChild))) return;

                    fileStream.Position = 0;
                    _projectCenterUi.SetApplicationHandle(ViewProvider.GetHandle(this));
                    coreInfoResult = _projectCenterUi.ImportChild(fileStream, parameters);
                }

                if (coreInfoResult.OperationCode == OperationCode.Rejected)
                    return;

                RefreshChildrenProjects();

                if (coreInfoResult is IImportProjectResult importProjectResult)
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendLine("Imported to project: " + importProjectResult.CoreInfo.Guid.ToString("B"));
                    message.AppendLine("The following phases and elevations have been imported:");
                    foreach (var grouping in importProjectResult.ImportedElevations.GroupBy(x => x.ParentInfo))
                    {
                        message.AppendLine(grouping.Key.Guid.ToString("B"));
                        foreach (var elevationInfo in grouping)
                        {
                            message.AppendLine("\t" + elevationInfo.Guid.ToString("B"));
                        }
                    }

                    ShowMessage(message.ToString(), "Imported Phases and Elevations");
                }
            });
        }

        private void CreateProjectUi()
        {
            CatchException(() =>
            {
                var operationInfo = _projectCenterUi.CanCreateChild();
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanCreateChild))) return;

                _projectCenterUi.CreateChild();
                RefreshChildrenProjects();
            });
        }

        private void CreateProjectWithParameters()
        {
            CatchException(() =>
            {
                var createChildNameParameter = new ObservableCollection<IParameter>()
                {
                    new Parameter<string>
                    {
                        Key = WellKnownParameterKey.ProjectCenter.CreateChild.Name
                    }
                };

                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(createChildNameParameter, false);

                if (!ShowDialog(parametersViewModel)) return;

                var parameters = parametersViewModel.GetParameters();

                parameters.TryGetValue(WellKnownParameterKey.ProjectCenter.CreateChild.Name, out var projectName);
                IOperationInfo operationInfo;
                if (string.IsNullOrWhiteSpace((string)projectName))
                {
                    operationInfo = _projectCenterUi.CanCreateChild();
                    if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanCreateChild))) return;
                    ((IProjectCenter)_projectCenterUi).CreateChild();
                }
                else
                {
                    operationInfo = _projectCenterUi.CanCreateChild(parameters);
                    if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanCreateChild))) return;
                    ((IProjectCenter)_projectCenterUi).CreateChild(parameters);
                }

                RefreshChildrenProjects();
            });
        }

        private void CreateProjectSilent()
        {
            CatchException(() =>
            {
                var operationInfo = _projectCenterUi.CanCreateChild();
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanCreateChild))) return;

                ((IProjectCenter)_projectCenterUi).CreateChild();

                RefreshChildrenProjects();
            });
        }

        private void DeleteProject(IBaseProjectInfo projectInfo)
        {
            _ = CatchException(() =>
              {
                  var operationInfo = _projectCenterUi.CanDeleteChild(projectInfo);
                  if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanDeleteChild))) return;
                  ((IProjectCenter)_projectCenterUi).DeleteChild(projectInfo);

                  RefreshChildrenProjects();
              });
        }

        private void DeleteProjectUi(IBaseProjectInfo projectInfo)
        {
            CatchException(() =>
            {
                var operationInfo = _projectCenterUi.CanDeleteChild(projectInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanDeleteChild))) return;
                _projectCenterUi.DeleteChild(projectInfo);

                RefreshChildrenProjects();
            });
        }

        private void ForceDeleteProject(IBaseProjectInfo projectInfo)
        {
            _ = CatchException(() =>
            {
                var operationInfo = _projectCenterUi.CanDeleteChild(projectInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanDeleteChild))) return;
                ((IProjectCenter)_projectCenterUi).ForceDeleteChild(projectInfo);

                RefreshChildren();
            });
        }

        private void ForceDeleteProjectUi(IBaseProjectInfo projectInfo)
        {
            CatchException(() =>
            {
                var operationInfo = _projectCenterUi.CanDeleteChild(projectInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanDeleteChild))) return;
                _projectCenterUi.ForceDeleteChild(projectInfo);

                RefreshChildren();
            });
        }

        private void DeleteProjects(bool useCoreObject, bool forceDelete)
        {
            CatchException(() =>
            {
                var operationInfo = _projectCenterUi.CanDeleteChildren(Projects.SelectedItems);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanDeleteChildren))) return;

                var deleteChildrenResult = default(IDeleteChildrenResult);
                StringBuilder message = new StringBuilder();

                if (useCoreObject)
                {
                    deleteChildrenResult = forceDelete
                        ? ((IProjectCenter)_projectCenterUi).ForceDeleteChildren(Projects.SelectedItems)
                        : ((IProjectCenter)_projectCenterUi).DeleteChildren(Projects.SelectedItems);
                }
                else
                {
                    deleteChildrenResult = forceDelete
                        ? _projectCenterUi.ForceDeleteChildren(Projects.SelectedItems)
                        : _projectCenterUi.DeleteChildren(Projects.SelectedItems);

                    if (deleteChildrenResult.OperationCode == OperationCode.Accepted)
                    {
                        message.AppendLine("Deleting projects succeded");
                    }
                    else
                    {
                        message.AppendLine("Deleting projects aborted");
                    }
                    message.AppendLine();
                }

                RefreshChildrenProjects(true);

                message.AppendLine("PermanentlyDeleted:");
                foreach (var elevationGuid in deleteChildrenResult.PermanentlyDeleted)
                {
                    message.AppendLine("\t" + elevationGuid.ToString("B"));
                }
                message.AppendLine("Recycle Bin:");
                foreach (var elevationGuid in deleteChildrenResult.InRecycleBin)
                {
                    message.AppendLine("\t" + elevationGuid.ToString("B"));
                }
                message.AppendLine("Skipped:");
                foreach (var elevationGuid in deleteChildrenResult.Skipped)
                {
                    message.AppendLine("\t" + elevationGuid.ToString("B"));
                }

                ShowMessage(message.ToString(), "Delete projects");
            });
        }

        private void DeleteProjects()
        {
            DeleteProjects(true, false);
        }

        private void DeleteProjectsUi()
        {
            DeleteProjects(false, false);
        }
        private void ForceDeleteProjects()
        {
            DeleteProjects(true, true);
        }

        private void ForceDeleteProjectsUi()
        {
            DeleteProjects(false, true);
        }

        private void CopyProject(ICoreInfo coreInfo)
        {
            CatchException(() =>
            {
                ClipboardHelper.Data = new ClipboardData<ProjectCenterViewModel, ICoreInfo>
                {
                    Sender = this,
                    Content = coreInfo
                };
            });
        }

        private void PasteProject()
        {
            CatchException(() => { MoveOrCopy(_projectCenterUi.CanCopyFrom, _projectCenterUi.CopyFrom); });
        }

        private void MoveProject()
        {
            CatchException(() => { MoveOrCopy(_projectCenterUi.CanMoveFrom, _projectCenterUi.MoveFrom); });
        }

        private void MoveOrCopy(Func<IBaseProjectInfo, IOperationInfo> canExecuteOperation,
            Func<IBaseProjectInfo, ICoreInfoResult<IBaseProjectInfo>> operation)
        {
            CatchException(() =>
            {
                if (ClipboardHelper.Data == null)
                    throw new InvalidOperationException("Clipboard is empty.");

                var clipboardData = ClipboardHelper.Data as ClipboardData<ProjectCenterViewModel, ICoreInfo>;
                if (clipboardData == null)
                    throw new InvalidOperationException(
                        $"Clipboard data is not valid. Clipboard data is of type '{ClipboardHelper.Data.GetType()}'.");

                var projectInfo = (IBaseProjectInfo)clipboardData.Content;
                var operationInfo = canExecuteOperation(projectInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(canExecuteOperation))) return;

                var newProjectInfo = operation(projectInfo);
                if (newProjectInfo == null)
                    throw new InfoIsNullException();

                clipboardData.Sender.RefreshChildrenProjects(true);
                RefreshChildrenProjects();
            });
        }

        private void CreateProjectCenter()
        {
            CatchException(() =>
            {
                var inputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel<string>("Create project center (set a new directory name)");
                if (!ShowDialog(inputBoxViewModel)) return;

                var parameters = new Dictionary<string, object>();
                parameters.Add(WellKnownEditKey.ProjectCenter.DIRECTORY_NAME, inputBoxViewModel.GetValue());

                var operationInfo = _projectCenterUi.ProjectCenterContainer.CanCreateChild(parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenterContainer.CanCreateChild))) return;

                var projectCenterInfo = _projectCenterUi.ProjectCenterContainer.CreateChild(parameters);
                if (projectCenterInfo == null)
                    throw new NullReferenceException("Return info is null.");

                RefreshChildrenProjectCenters();
            });
        }

        private void DeleteProjectCenter(IProjectCenterInfo projectCenterInfo)
        {
            if (projectCenterInfo == null)
                return;

            CatchException(() =>
            {
                var operationInfo = _projectCenterUi.ProjectCenterContainer.CanDeleteChild(projectCenterInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenterContainer.CanDeleteChild))) return;

                _projectCenterUi.ProjectCenterContainer.DeleteChild(projectCenterInfo);
                RefreshChildrenProjectCenters();
            });
        }

        private bool CanCopyProjectCenter()
        {
            return ProjectCenters.SelectedItem != null;
        }

        private void CopyProjectCenter()
        {
            CatchException(() =>
            {
                ClipboardHelper.Data = new ClipboardData<ProjectCenterViewModel, IProjectCenterInfo>
                {
                    Sender = this,
                    Content = ProjectCenters.SelectedItem
                };
            });
        }

        private bool CanMoveProjectCenter()
        {
            return ClipboardHelper.Data != null;
        }

        private void MoveProjectCenter()
        {
            CatchException(() =>
            {
                if (ClipboardHelper.Data == null)
                    throw new InvalidOperationException("Clipboard is empty.");

                var clipboardData = ClipboardHelper.Data as ClipboardData<ProjectCenterViewModel, IProjectCenterInfo>;
                if (clipboardData == null)
                    throw new InvalidOperationException(
                        $"Clipboard data is not valid. Clipboard data is of type '{ClipboardHelper.Data.GetType()}'.");

                var operationInfo = _projectCenterUi.ProjectCenterContainer.CanMoveFrom(clipboardData.Content);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenterContainer.CanMoveFrom))) return;

                var newProjectCenterInfo = _projectCenterUi.ProjectCenterContainer.MoveFrom(clipboardData.Content);
                if (newProjectCenterInfo == null)
                    throw new InfoIsNullException();

                clipboardData.Sender.RefreshChildrenProjectCenters(true);
                RefreshChildrenProjectCenters();
            });
        }

    private void ExportChild(IBaseProjectInfo projectInfo)
        {
            CatchException(() =>
            {
                var (fileName, elevations, parameters) = GetExportChildParams(projectInfo);
                if (string.IsNullOrWhiteSpace(fileName))
                    return;

                var operationInfo = _projectCenterUi.CanExportChild(projectInfo, elevations, parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanExportChild))) return;

                var exportStream = _projectCenterUi.ExportChild(projectInfo, elevations, parameters).Stream;
                using (exportStream)
                {
                    using (var fileStream = new FileStream(fileName, FileMode.Create))
                    {
                        exportStream.CopyTo(fileStream);
                    }
                }

                ShowMessage("Export succeeded.");
            });
        }

        private void ExportChildWithUi(IBaseProjectInfo projectInfo)
        {
            CatchException(() =>
            {
                var saveFileDialogViewModel = ViewProvider.ViewModelFactory.GetSaveFileDialogViewModel();
                saveFileDialogViewModel.FileName = projectInfo.Name;
                saveFileDialogViewModel.AddFilter(ExportTypeLob, true);
                saveFileDialogViewModel.AddFilter(ExportTypeL2B);

                if (!ShowDialog(saveFileDialogViewModel))
                    return;

                string fileExtension = saveFileDialogViewModel.GetExtension(false);
                bool isL2BExport = fileExtension.Equals(ExportTypeL2B, StringComparison.InvariantCultureIgnoreCase);
                var parameters = new Dictionary<string, object>
                {
                    {WellKnownParameterKey.ProjectCenter.Export.Type, isL2BExport ? ExportTypeL2B : ExportTypeLob}
                };

                var operationInfo = _projectCenterUi.CanExportChild(projectInfo, parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenter.CanExportChild))) return;

                var streamResult = _projectCenterUi.ExportChild(projectInfo, parameters);

                if (streamResult.OperationCode == OperationCode.Rejected)
                {
                    ShowMessage("Export failed.");
                    return;
                }

                using (var resultStream = streamResult.Stream)
                using (var fileStream = new FileStream(saveFileDialogViewModel.FileName, FileMode.Create))
                {
                    resultStream.CopyTo(fileStream);
                }
            });
        }

        private void ForceRefresh()
        {
            Refresh(true, true);
            RefreshChildren(true);
        }

        private void ForceRefreshProjects()
        {
            Refresh(true, true);
            RefreshChildrenProjects(true);
        }

        private void ForceRefreshProjectCenters()
        {
            Refresh(true, true);
            RefreshChildrenProjectCenters(true);
        }

        private (string fileName, IEnumerable<IElevationInfo> elevations, Dictionary<string, object> parameters) GetExportChildParams(
            IBaseProjectInfo projectInfo)
        {
            var saveFileDialogViewModel = ViewProvider.ViewModelFactory.GetSaveFileDialogViewModel();
            saveFileDialogViewModel.FileName = projectInfo.Name;
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            saveFileDialogViewModel.FileName = string.Join("_", saveFileDialogViewModel.FileName.Split(invalidFileNameChars));
            saveFileDialogViewModel.AddFilter(ExportTypeLob, true);
            saveFileDialogViewModel.AddFilter(ExportTypeL2B);

            if (!ShowDialog(saveFileDialogViewModel))
                return (null, null, null);

            var fileExtension = saveFileDialogViewModel.GetExtension(false);
            var isL2BExport = fileExtension.Equals(ExportTypeL2B, StringComparison.InvariantCultureIgnoreCase);

            IEnumerable<IElevationInfo> elevationInfos;
            using (var project = _projectCenterUi.GetChild(projectInfo))
            {
                var selectElevationsViewModel = ViewProvider.ViewModelFactory.GetSelectElevationsViewModel(project.CoreObject);
                if (!ShowDialog(selectElevationsViewModel))
                    return (null, null, null);

                elevationInfos = selectElevationsViewModel.Elevations.SelectedItems;
            }

            var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(new ObservableCollection<IParameter>()
            {
                new Parameter<string>
                {
                    Key = WellKnownParameterKey.ProjectCenter.Export.Type,
                    IsRequired = true,
                    Value = isL2BExport ? ExportTypeL2B : ExportTypeLob,
                    RestrictedValues = new List<string> {ExportTypeLob, ExportTypeL2B}
                }
            });

            if (!isL2BExport)
            {
                parametersViewModel.Parameters.Add(new Parameter<bool>
                {
                    Key = WellKnownParameterKey.ProjectCenter.Export.WithDocuments,
                    IsRequired = true
                });

                parametersViewModel.Parameters.Add(new Parameter<bool>
                {
                    Key = WellKnownParameterKey.ProjectCenter.Export.WithHistory,
                    IsRequired = true
                });

                parametersViewModel.Parameters.Add(new Parameter<bool>
                {
                    Key = WellKnownParameterKey.ProjectCenter.Export.WithCalculationData,
                    IsRequired = true
                });

                parametersViewModel.Parameters.Add(new Parameter<bool>
                {
                    Key = WellKnownParameterKey.ProjectCenter.Export.WithOfferPrices,
                    IsRequired = true
                });

                parametersViewModel.Parameters.Add(new Parameter<bool>
                {
                    Key = WellKnownParameterKey.ProjectCenter.Export.WithFabricationLots,
                    IsRequired = true
                });
            }

            if (!ShowDialog(parametersViewModel))
                return (null, null, null);

            var parameters = parametersViewModel.GetParameters();

            return (saveFileDialogViewModel.FileName, elevationInfos, parameters);
        }

        private void ExportChildren()
        {
            CatchException(() =>
            {
                var observableCollection = new ObservableCollection<IParameter>();
                observableCollection.Add(new Parameter<string>
                {
                    Key = WellKnownParameterKey.ProjectCenter.Export.Type,
                    IsRequired = true,
                    Value = ExportTypeLob,
                    RestrictedValues = new List<string> { ExportTypeLob, ExportTypeL2B }
                });
                observableCollection.Add(new Parameter<string>
                {
                    Key = WellKnownParameterKey.ProjectCenter.Export.DestinationDirectory,
                    Value = ""
                });
                observableCollection.Add(new Parameter<bool>
                {
                    Key = WellKnownParameterKey.ProjectCenter.Export.SelectElevations,
                    Value = false
                });

                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(observableCollection);
                if (!ShowDialog(parametersViewModel))
                    return;

                var operationInfo = ProjectCenterUi.CanExportChildren(Projects.SelectedItems, parametersViewModel.GetParameters());
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectCenterUi.CanExportChildren))) return;

                var result = ProjectCenterUi.ExportChildren(Projects.SelectedItems, parametersViewModel.GetParameters());

                if (result.OperationCode == OperationCode.Accepted)
                    ShowMessage("Export succeded");
            });
        }

        private void OpenInNewWindow()
        {
            CatchException(() =>
            {
                var loginScope = _projectCenterUi.LoginScope;
                var coreObjectFactory = loginScope.GetCoreObjectFactory().CoreObjectFactory;

                var projectCenter = (ICoreObjectResult<IProjectCenterUi>)coreObjectFactory.GetProjectCenter(_projectCenterUi.Info);
                var projectCenterViewModel = ViewProvider.ViewModelFactory.GetProjectCenterViewModel(projectCenter);
                Show(projectCenterViewModel);
            });
        }

        private void OnProjectRefreshed(object sender, EventArgs e)
        {
            RefreshChildrenProjects();
        }

        private void OnProjectCenterRefreshed(object sender, EventArgs e)
        {
            RefreshChildrenProjectCenters(true);
        }

        private void Refresh(bool hardRefresh = false, bool suppressEvent = false)
        {
            if (hardRefresh)
                _projectCenterUi.Refresh();

            ProjectCenterModel = new ProjectCenterModel
            {
                CoreObjectId = _projectCenterUi.Id,
                DirectoryName = _projectCenterUi.Info.DirectoryName,
                TypeAsName = _projectCenterUi.Info.Type.Name,
                TypeAsId = _projectCenterUi.Info.Type.Id,
                IsRecycleBin = _projectCenterUi.Info.IsRecycleBin
            };

            if (!suppressEvent)
                ProjectCenterRefreshed?.Invoke(this, EventArgs.Empty);
        }

        private void ShowProjectStatuses()
        {
            CatchException(() =>
            {
                var viewModel = ViewProvider.ViewModelFactory.GetProjectStatusesViewModel(_projectCenterUi);
                ViewProvider.Show(viewModel);
            });
        }

        private void OpenProjectSynchronizationContainer()
        {
            CatchException(() =>
            {
                if (_synchronizationContainerProjectViewModel != null) return;

                _synchronizationContainerProjectViewModel = ViewProvider.ViewModelFactory.GetSynchronizationContainerViewModel(ProjectCenterUi);
                _synchronizationContainerProjectViewModel.SynchronizedEventReceived +=
                    SynchronizationViewModelOnProjectSynchronizedEventReceived;
                Show(_synchronizationContainerProjectViewModel, delegate { OnProjectSynchronizationContainerClosed(); });
            });
        }

        private void SynchronizationViewModelOnProjectSynchronizedEventReceived(object sender,
            SynchronizedEventReceivedEventArgs eventArgs)
        {
            CatchException(() =>
            {
                var synchronizedEvent = eventArgs.SynchronizedEvent;
                switch (synchronizedEvent.Object)
                {
                    case WellKnownSynchronizedEventObject.Project:
                        RefreshChildrenProjects(true);
                        break;
                }

                ProjectCenterUi.SynchronizationContainer.SetHandled(synchronizedEvent);
            });
        }

        private void OnProjectSynchronizationContainerClosed()
        {
            _synchronizationContainerProjectViewModel.SynchronizedEventReceived -= SynchronizationViewModelOnProjectSynchronizedEventReceived;
            _synchronizationContainerProjectViewModel = null;
        }

        private void OpenProjectCenterSynchronizationContainer()
        {
            CatchException(() =>
            {
                if (_synchronizationContainerProjectCenterViewModel != null) return;

                _synchronizationContainerProjectCenterViewModel = ViewProvider.ViewModelFactory.GetSynchronizationContainerViewModel(
                    ProjectCenterUi.ProjectCenterContainer);
                _synchronizationContainerProjectCenterViewModel.SynchronizedEventReceived +=
                    SynchronizationViewModelOnProjectCenterSynchronizedEventReceived;
                Show(_synchronizationContainerProjectCenterViewModel, delegate { OnProjectCenterSynchronizationContainerClosed(); });
            });
        }

        private void SynchronizationViewModelOnProjectCenterSynchronizedEventReceived(object sender,
            SynchronizedEventReceivedEventArgs eventArgs)
        {
            CatchException(() =>
            {
                var synchronizedEvent = eventArgs.SynchronizedEvent;
                switch (synchronizedEvent.Object)
                {
                    case WellKnownSynchronizedEventObject.ProjectCenter:
                        switch (synchronizedEvent.Action)
                        {
                            case WellKnownSynchronizedEventAction.Changed:
                                {
                                    if (synchronizedEvent.ActionKey == WellKnownActionKey.ProjectCenter.ChildRenamed)
                                        RefreshChildrenProjectCenters(true);
                                    else
                                        Refresh(true);
                                }
                                break;
                            case WellKnownSynchronizedEventAction.Added:
                            case WellKnownSynchronizedEventAction.Removed:
                                RefreshChildrenProjectCenters(true);
                                break;
                        }
                        break;
                }

                ProjectCenterUi.ProjectCenterContainer.SynchronizationContainer.SetHandled(synchronizedEvent);
            });
        }

        private void OnProjectCenterSynchronizationContainerClosed()
        {
            _synchronizationContainerProjectCenterViewModel.SynchronizedEventReceived -= SynchronizationViewModelOnProjectCenterSynchronizedEventReceived;
            _synchronizationContainerProjectCenterViewModel = null;
        }


        private void RefreshChildrenProjects(bool hardRefresh = false)
        {
            // no need to refresh when view model was disposed already
            if (_projectCenterUi == null)
                return;

            if (hardRefresh)
                _projectCenterUi.RefreshChildren();

            Projects.ItemsSource = new ObservableCollection<IBaseProjectInfo>(_projectCenterUi.ChildrenInfos);
        }

        private void RefreshChildrenProjectCenters(bool hardRefresh = false)
        {
            // no need to refresh when view model was disposed already
            if (_projectCenterUi == null)
                return;

            if (hardRefresh)
                _projectCenterUi.ProjectCenterContainer.RefreshChildren();

            ProjectCenters.ItemsSource = new ObservableCollection<IProjectCenterInfo>(_projectCenterUi.ProjectCenterContainer.ChildrenInfos);
        }

        public void RefreshChildren(bool hardRefresh = false)
        {
            RefreshChildrenProjects(hardRefresh);
            RefreshChildrenProjectCenters(hardRefresh);
        }

        public void Dispose()
        {
            ProjectCenterUiResult.Disposed -= OnProjectCenterUiDisposed;

            if (ProjectCenterUiResult == null || ProjectCenterUi.IsDisposed) return;

            ProjectCenterUiResult.Dispose();
            ProjectCenterUiResult = null;
        }

        public void OnLoaded()
        {
            _projectCenterUi.SetApplicationHandle(ViewProvider.GetHandle(this));
        }

        public bool CanDispose(out string falseReason)
        {
            return CanDispose(ProjectCenterUiResult, out falseReason);
        }
    }
}
