using Ofcas.Lk.Api.Client.Core;
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
using System.Text;
using System.Windows;
using System.Windows.Input;
using Ofcas.Lk.Api.Client.Demo.Events;
using Enum = Ofcas.Lk.Api.Client.Demo.Utils.Enum;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class LoginViewModel : CoreObjectViewModel, IExtendedDisposable
    {
        private SynchronizationContainerViewModel _synchronizationContainerViewModel;
        private EnvironmentModel _environmentModel = new EnvironmentModel();
        private ICoreObjectResult<ILoginScopeUi> _loginScopeUiResult;
        private ILoginScopeUi _loginScopeUi;
        protected IServiceProxyAdapter ServiceProxy;

        protected ICoreObjectResult<ILoginScopeUi> LoginScopeUiResult
        {
            get { return _loginScopeUiResult; }
            set
            {
                _loginScopeUiResult = value;
                _loginScopeUi = _loginScopeUiResult.CoreObject;
                LoginScopeProvider.Instance.LoginScope = _loginScopeUi;
            }
        }
        protected ILoginScopeUi LoginScopeUi => _loginScopeUi;

        private string _loginWindowTitle = "LogiKal - API (v3)";

        public string LoginWindowTitle
        {
            get { return _loginWindowTitle; }
            private set { _loginWindowTitle = value; OnPropertyChanged(); }
        }

        public EnvironmentModel EnvironmentModel
        {
            get { return _environmentModel; }
            set { _environmentModel = value; OnPropertyChanged(); }
        }

        public bool EnableEventSynchronization { get; private set; } = true;
        public SelectionAdapter<IProjectCenterInfo> ProjectCenters { get; } = new SelectionAdapter<IProjectCenterInfo>();

        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand RunProgramCommand { get; }
        public ICommand OpenProjectCenterCommand { get; }
        public ICommand ShowRecentProjectsCommand { get; }
        public ICommand SelectProjectCommand { get; }
        public ICommand SelectElevationCommand { get; }
        public ICommand ExportDatabaseCommand { get; }
        public ICommand GetGlassPriceCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenElementPricelistContainer { get; }
        public ICommand ClearDemoUserSettingsCommand { get; }
        public ICommand OpenProgramInformationCommand { get; }
        public ICommand ShowRecentFabricationLotsCommand { get; }
        public ICommand ShowLanguagesCommand { get; }
        public ICommand ShowUserInformationCommand { get; }
        public ICommand ShowSearchCommand { get; }

        public ICommand OpenProgramRuntimeInformationCommand { get; }
        public ICommand ShowRecentCadDrawingsCommand { get; }
        public ICommand OpenProjectFromElevationCommand { get; }
        public ICommand OpenProjectByGuidCommand { get; }

        public ICommand CreateProjectCommand { get; }
        public ICommand ShowElementTypesCommand { get; }

        public ICommand ShowEstimationDataSetsCommand { get; }

        public ICommand OpenSynchronizationContainerCommand { get; }
        public ICommand EnableEventSynchronizationCommand { get; }

        public LoginViewModel(IViewProvider viewProvider) : base(viewProvider)
        {
            EnvironmentModel = EnvironmentSettings.Load();

            LoginCommand = new Command(Login, CanLogin);
            LogoutCommand = new Command(Logout, IsLoggedIn);
            RunProgramCommand = new Command(RunProgram, CanRunProgram);
            OpenProjectCenterCommand = new Command<IProjectCenterInfo>(OpenProjectCenter, CanOpenProjectCenter);
            ShowRecentProjectsCommand = new Command(ShowRecentProjects, CanShowRecentProjects);
            SelectProjectCommand = new Command(SelectProject, CanSelectProject);
            SelectElevationCommand = new Command(SelectElevation, CanSelectElevation);
            ExportDatabaseCommand = new Command(ExportDatabase, CanExportDatabase);
            GetGlassPriceCommand = new Command(GetGlassPrice, CanGetGlassPrice);
            OpenSettingsCommand = new Command(OpenSettings, IsLoggedIn);
            OpenElementPricelistContainer = new Command(OpenElementPricelist, IsLoggedIn);
            ClearDemoUserSettingsCommand = new Command(ClearDemoUserSettings);
            OpenProgramInformationCommand = new Command(OpenProgramInformation);
            ShowRecentFabricationLotsCommand = new Command(ShowRecentFabricationLots, CanShowRecentFabricationLots);
            OpenProjectFromElevationCommand = new Command(OpenProjectFromElevation, IsLoggedIn);
            OpenProjectByGuidCommand = new Command(OpenProjectByGuid, IsLoggedIn);
            ShowLanguagesCommand = new Command(ShowLanguages, CanShowLanguages);
            ShowUserInformationCommand = new Command(ShowUserInformation, CanShowUserInformaton);
            OpenProgramRuntimeInformationCommand = new Command(OpenProgramRuntimeInformation, IsLoggedIn);
            ShowRecentCadDrawingsCommand = new Command(ShowRecentCadDrawings, IsLoggedIn);
            CreateProjectCommand = new Command(CreateProject, IsLoggedIn);
            ShowElementTypesCommand = new Command(ShowElementTypes, CanShowElementTypes);
            ShowSearchCommand = new Command(ShowSearch, CanShowSearch);
            OpenSynchronizationContainerCommand = new Command(OpenSynchronizationContainer, IsLoggedIn);
            ShowEstimationDataSetsCommand = new Command(OpenEstimationDataSetContainer, IsLoggedIn);
            EnableEventSynchronizationCommand = new Command(() => EnableEventSynchronization = !EnableEventSynchronization, CanLogin);
        }

        private void OpenEstimationDataSetContainer()
        {
            CatchException(() =>
            {
                var estimationDataSetContainer = _loginScopeUi.EstimationDataSetContainer;
                var estimationDataSetContainerViewModel = ViewProvider.ViewModelFactory.GetEstimationDataSetContainerViewModel(estimationDataSetContainer);
                ShowDialog(estimationDataSetContainerViewModel);
            });
        }

        protected virtual void InitializeServiceProxy()
        {
            if (ServiceProxy == null)
            {
                var serviceProxy = ServiceProxyUiFactory.CreateServiceProxy(EnvironmentModel.LauncherPath, Environment.CommandLine).ServiceProxyUi;
                ServiceProxy = new ServiceProxyAdapter(serviceProxy);
                try
                {
                    ServiceProxy.Start();
                }
                catch
                {
                    ServiceProxy = null;
                    throw;
                }
            }
        }

        private void OpenProgramInformation()
        {
            CatchException(() =>
            {
                InitializeServiceProxy();

                var programInformation = ServiceProxy.GetProgramInformation();
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendFormat("Distribution: {0} ({1})", programInformation.DistributionName,
                    programInformation.DistributionId);
                messageBuilder.AppendLine();
                messageBuilder.AppendFormat("Edition: {0} ({1})", programInformation.EditionName,
                    programInformation.EditionId);
                messageBuilder.AppendLine();
                messageBuilder.Append(programInformation.ProductVersion.Description);
                messageBuilder.AppendLine();
                messageBuilder.Append(programInformation.ProductVersion.Type);
                foreach (var version in programInformation.ComponentVersions)
                {
                    messageBuilder.AppendLine();
                    messageBuilder.Append(version.Description);
                    messageBuilder.AppendLine();
                    messageBuilder.Append(version.Type);
                }
                MessageBox.Show(messageBuilder.ToString());
            });
        }

        private void OpenProgramRuntimeInformation()
        {
            CatchException(() =>
            {
                InitializeServiceProxy();

                var operationInfo = _loginScopeUi.CanGetProgramRuntimeInformation();
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScope.CanGetProgramRuntimeInformation))) return;

                var programRuntimeInformation = _loginScopeUi.GetProgramRuntimeInformation();
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendFormat("IsVcsClient : {0}", programRuntimeInformation.IsVcsClient);
                messageBuilder.AppendLine();
                messageBuilder.AppendFormat("IsRdpModeActive : {0}", programRuntimeInformation.IsRdpModeActive);
                messageBuilder.AppendLine();
                messageBuilder.AppendFormat("IsRemoteAppActive : {0}", programRuntimeInformation.IsRemoteAppActive);
                messageBuilder.AppendLine();
                messageBuilder.AppendFormat("IsHostedRemoteAppActive : {0}", programRuntimeInformation.IsHostedRemoteAppActive);
                MessageBox.Show(messageBuilder.ToString());
            });
        }

        private bool CanLogin()
        {
            return !IsLoggedIn();
        }

        private void Login()
        {
            CatchException(() =>
            {
                InitializeServiceProxy();

                InitializeVersionInformation();

                var parameters = new Dictionary<string, object>();
                parameters.Add(WellKnownParameterKey.Login.ProgramMode, EnvironmentModel.ProgramMode);
                parameters.Add(WellKnownParameterKey.Login.ApplicationHandle, ViewProvider.GetHandle(this));
                parameters.Add(WellKnownParameterKey.Login.EnableEventSynchronization, EnableEventSynchronization);
                var coreObjectResult = ServiceProxy.Login(parameters);
                if (coreObjectResult == null)
                    return;
                if (coreObjectResult.OperationCode == OperationCode.Rejected)
                {
                    Reset();
                    return;
                }

                LoginScopeUiResult = coreObjectResult;
                CoreObject = LoginScopeUiResult.CoreObject;
                ProjectCenters.ItemsSource = new ObservableCollection<IProjectCenterInfo>(LoginScopeUi.ProjectCenterInfos);

                EnvironmentModel.AddModeIfDoesNotExist(EnvironmentModel.ProgramMode);
                EnvironmentModel.AddPathIfDoesNotExist(EnvironmentModel.LauncherPath);

                EnvironmentSettings.Save(EnvironmentModel);
            });
        }

        private void Logout()
        {
            CatchException(() =>
            {
                var operationInfo = _loginScopeUi.CanLogout();
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScope.CanLogout))) return;

                ViewProvider.ForceCloseAll();
                _loginScopeUi.Logout();
                Reset();
            });
        }

        private bool CanRunProgram()
        {
            return IsLoggedIn();
        }

        private void RunProgram()
        {
            CatchException(() =>
            {
                var loginRunProgramViewModel = ViewProvider.ViewModelFactory.GetLoginProgramsViewModel(LoginScopeUi);
                ViewProvider.Show(loginRunProgramViewModel);
            });
        }

        private bool CanOpenProjectCenter(IProjectCenterInfo obj)
        {
            return IsLoggedIn();
        }

        private void OpenProjectCenter(IProjectCenterInfo projectCenterInfo)
        {
            CatchException(() =>
            {
                if (projectCenterInfo == null) return;

                var operationInfo = LoginScopeUi.CanGetProjectCenter(projectCenterInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScope.CanGetProjectCenter))) return;

                var projectCenter = LoginScopeUi.GetProjectCenter(projectCenterInfo);
                var projectCenterViewModel = ViewProvider.ViewModelFactory.GetProjectCenterViewModel(projectCenter);
                ViewProvider.Show(projectCenterViewModel);
            });
        }

        private bool CanShowRecentProjects()
        {
            return IsLoggedIn();
        }

        private void ShowRecentProjects()
        {
            CatchException(() =>
            {
                var viewModel = ViewProvider.ViewModelFactory.GetRecentProjectsViewModel(LoginScopeUi);
                ViewProvider.Show(viewModel);
            });
        }

        private bool CanShowRecentFabricationLots()
        {
            return IsLoggedIn();
        }

        private void ShowRecentFabricationLots()
        {
            CatchException(() =>
            {
                var viewModel = ViewProvider.ViewModelFactory.GetRecentFabricationLotsViewModel(LoginScopeUi);
                ViewProvider.Show(viewModel);
            });
        }

        private bool CanSelectProject()
        {
            return IsLoggedIn();
        }

        private void SelectProject()
        {
            CatchException(() =>
            {
                var operationInfo = _loginScopeUi.CanSetApplicationHandle(ViewProvider.GetHandle(this));
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanSetApplicationHandle))) return;

                _loginScopeUi.SetApplicationHandle(ViewProvider.GetHandle(this));

                operationInfo = LoginScopeUi.CanSelectProject();
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanSelectProject))) return;

                var coreObjectResult = LoginScopeUi.SelectProject();
                if (coreObjectResult.OperationCode == OperationCode.Rejected) return;

                var projectViewModel = ViewProvider.ViewModelFactory.GetProjectViewModel(coreObjectResult);
                ViewProvider.Show(projectViewModel);
            });
        }

        private bool CanSelectElevation()
        {
            return IsLoggedIn();
        }

        private void SelectElevation()
        {
            CatchException(() =>
            {
                var operationInfo = _loginScopeUi.CanSetApplicationHandle(ViewProvider.GetHandle(this));
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanSetApplicationHandle))) return;

                _loginScopeUi.SetApplicationHandle(ViewProvider.GetHandle(this));

                operationInfo = LoginScopeUi.CanSelectElevation();
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanSelectElevation))) return;

                var result = LoginScopeUi.SelectElevation();
                if (result.OperationCode == OperationCode.Rejected) return;

                var elevationViewModel = ViewProvider.ViewModelFactory.GetElevationViewModel(result);
                ViewProvider.Show(elevationViewModel);
            });
        }

        private bool CanExportDatabase()
        {
            return IsLoggedIn();
        }

        private void ExportDatabase()
        {
            CatchException(() =>
            {
                var operationInfo = _loginScopeUi.CanSetApplicationHandle(ViewProvider.GetHandle(this));
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanSetApplicationHandle))) return;

                _loginScopeUi.SetApplicationHandle(ViewProvider.GetHandle(this));

                var databaseExportTypeParameter = new Parameter<DatabaseExportType>
                {
                    IsRequired = true,
                    Key = "DATABASE_EXPORT_TYPE",
                    RestrictedValues = Enum.GetValues<DatabaseExportType>().ToList()
                };

                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(new ObservableCollection<IParameter>
                {
                    databaseExportTypeParameter
                });

                if (!ShowDialog(parametersViewModel)) return;

                operationInfo = LoginScopeUi.CanIsExportNeeded(databaseExportTypeParameter.Value);
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScope.CanIsExportNeeded))) return;

                if (!LoginScopeUi.IsExportNeeded(databaseExportTypeParameter.Value).Value)
                {
                    var messageBoxResult = ShowMessage(
                        $"Export for '{databaseExportTypeParameter.Value}' is not needed.{Environment.NewLine}" +
                        "Do you still want to continue?", "Confirmation", MessageBoxButton.YesNo);

                    if (messageBoxResult != MessageBoxResult.Yes) return;
                }

                var saveFileDialogViewModel = ViewProvider.ViewModelFactory.GetSaveFileDialogViewModel("MDB Files | *.mdb|XML Files | *.xml | SQLite Files | *.sqlite", "MDB");
                if (!ShowDialog(saveFileDialogViewModel)) return;

                var parameters = new Dictionary<string, object>()
                {
                    [WellKnownParameterKey.LoginScope.Export.Type] = ExportType.Database,
                    [WellKnownParameterKey.LoginScope.Export.Format] = saveFileDialogViewModel.GetExtension(false),
                    [WellKnownParameterKey.LoginScope.Export.DatabaseExportType] = databaseExportTypeParameter.Value
                };

                operationInfo = LoginScopeUi.CanGetExport(parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScope.CanGetExport))) return;

                using (var exportDataStream = LoginScopeUi.GetExport(parameters).Stream)
                {
                    using (var fileStream = new FileStream(saveFileDialogViewModel.FileName, FileMode.Create))
                    {
                        exportDataStream.CopyTo(fileStream);
                    }
                }
            });
        }

        private bool CanGetGlassPrice()
        {
            return IsLoggedIn();
        }

        private void GetGlassPrice()
        {
            CatchException(() =>
            {
                var operationInfo = _loginScopeUi.CanSetApplicationHandle(ViewProvider.GetHandle(this));
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanSetApplicationHandle))) return;

                _loginScopeUi.SetApplicationHandle(ViewProvider.GetHandle(this));

                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(new ObservableCollection<IParameter>
                {
                    new Parameter<string>
                    {
                        IsRequired = true,
                        Key = "GLASSNAME"
                    },
                    new Parameter<GlassType>
                    {
                        IsRequired = true,
                        Key = "GLASSTYPE",
                        RestrictedValues = Enum.GetValues<GlassType>().ToList()
                    },
                    new Parameter<double>
                    {
                        IsRequired = true,
                        Key = "WIDTH"
                    },
                    new Parameter<double>
                    {
                        IsRequired = true,
                        Key = "HEIGHT"
                    },
                    new Parameter<int>
                    {
                        IsRequired = true,
                        Key = "THICKNESS"
                    },
                    new Parameter<GlassOrigin>
                    {
                        IsRequired = true,
                        Key = "GLASSORIGIN",
                        RestrictedValues = Enum.GetValues<GlassOrigin>().ToList()
                    },
                    new Parameter<Guid>
                    {
                        IsRequired = true,
                        Key = "PROJECTGUID"
                    },
                    new Parameter<string>
                    {
                        IsRequired = true,
                        Key = "SPECIALINSIDE"
                    },
                    new Parameter<string>
                    {
                        IsRequired = true,
                        Key = "SPECIALOUTSIDE"
                    },
                    new Parameter<int>
                    {
                        IsRequired = true,
                        Key = "MODELTYPE"
                    }
                });

                if (!ShowDialog(parametersViewModel)) return;

                operationInfo = LoginScopeUi.CanGetGlassPrice(parametersViewModel.GetParameters());
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScope.CanGetGlassPrice))) return;

                var glassPrice = LoginScopeUi.GetGlassPrice(parametersViewModel.GetParameters());
                ShowMessage($"Glass price: {glassPrice.Value}");
            });
        }

        protected virtual void OpenSettings()
        {
            CatchException(() =>
            {
                var viewModel = ViewProvider.ViewModelFactory.GetSettingContainerViewModel(_loginScopeUi.SettingContainer);

                Show(viewModel);
            });
        }

        private void OpenElementPricelist()
        {
            CatchException(() =>
            {
                var elementPricelistContainer = LoginScopeUi.ElementPricelistContainer;
                var elementPricelistContainerViewModel = ViewProvider.ViewModelFactory.GetElementPricelistContainerViewModel(elementPricelistContainer);
                Show(elementPricelistContainerViewModel);
            });
        }

        private void ClearDemoUserSettings()
        {
            CatchException(() =>
            {
                EnvironmentSettings.Clear();
                EnvironmentModel = EnvironmentSettings.Load();
            });
        }

        public void Shutdown()
        {
            ServiceProxy?.Stop();
        }

        protected bool IsLoggedIn()
        {
            return ServiceProxy != null && LoginScopeUi != null;
        }

        private void Reset()
        {
            if (_loginScopeUiResult != null)
            {
                _loginScopeUiResult.Dispose();
                _loginScopeUiResult = null;
            }

            if (ServiceProxy != null)
            {
                ServiceProxy.Stop();
                ServiceProxy.Dispose();
                ServiceProxy = null;
            }

            ProjectCenters.ItemsSource?.Clear();
        }

        public bool CanDispose(out string falseReason)
        {
            falseReason = "";
            if (_loginScopeUiResult == null)
                return true;

            var canDispose = CanDispose(_loginScopeUiResult, out falseReason);
            if (canDispose)
                return true;

            var messageBoxResult = MessageBox.Show($"{falseReason}{Environment.NewLine}Do want to force exit?", "Warning",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            return messageBoxResult == MessageBoxResult.Yes;
        }

        public void Dispose()
        {
            if (_loginScopeUiResult != null && _loginScopeUiResult.CanDispose().Value)
            {
                ViewProvider.ForceCloseAll();
                Logout();
            }
            else if (ServiceProxy != null)
            {
                ServiceProxy.Stop();
                Environment.Exit(0);
            }
        }

        public void OpenProjectFromElevation()
        {
            CatchException(() =>
            {
                var operationInfo = _loginScopeUi.CanSetApplicationHandle(ViewProvider.GetHandle(this));
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanSetApplicationHandle))) return;

                _loginScopeUi.SetApplicationHandle(ViewProvider.GetHandle(this));

                var inputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel("Enter Elevation Guid", Guid.Empty);
                if (!ShowDialog(inputBoxViewModel))
                    return;
                var elevationGuid = inputBoxViewModel.GetValue();

                operationInfo = LoginScopeUi.CanGetProjectFromElevation(elevationGuid);
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanGetProjectFromElevation))) return;

                var project = LoginScopeUi.GetProjectFromElevation(elevationGuid);

                var projectViewModel = ViewProvider.ViewModelFactory.GetProjectViewModel(project);
                ViewProvider.Show(projectViewModel);
            });
        }

        public void OpenProjectByGuid()
        {
            CatchException(() =>
            {
                var operationInfo = _loginScopeUi.CanSetApplicationHandle(ViewProvider.GetHandle(this));
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanSetApplicationHandle))) return;

                _loginScopeUi.SetApplicationHandle(ViewProvider.GetHandle(this));

                var inputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel("Enter Project Guid", Guid.Empty);
                if (!ShowDialog(inputBoxViewModel))
                    return;

                var projectGuid = inputBoxViewModel.GetValue();

                operationInfo = LoginScopeUi.CanGetProjectByGuid(projectGuid);
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanGetProjectByGuid))) return;

                var project = LoginScopeUi.GetProjectByGuid(projectGuid);

                var projectViewModel = ViewProvider.ViewModelFactory.GetProjectViewModel(project);
                ViewProvider.Show(projectViewModel);
            });
        }

        private bool CanShowLanguages()
        {
            return IsLoggedIn();
        }

        private void ShowLanguages()
        {
            CatchException(() =>
            {
                var languagesViewModel = ViewProvider.ViewModelFactory.GetLanguagesViewModel(LoginScopeUi);
                ViewProvider.Show(languagesViewModel);
            });
        }

        private bool CanShowUserInformaton()
        {
            return IsLoggedIn();
        }

        private void ShowUserInformation()
        {
            CatchException(() =>
            {
                var userInformationViewModel = ViewProvider.ViewModelFactory.GetUserInformationViewModel(LoginScopeUi);
                ViewProvider.Show(userInformationViewModel);
            });
        }

        private bool CanShowSearch()
        {
            return IsLoggedIn();
        }

        private void ShowSearch()
        {
            CatchException(() =>
            {
                var searchAgent = LoginScopeUi.SearchAgent;
                var projectSearchViewModel = ViewProvider.ViewModelFactory.GetProjectSearchViewModel(searchAgent);
                ViewProvider.Show(projectSearchViewModel);
            });
        }

        private void InitializeVersionInformation()
        {
            var programInformation = ServiceProxy.GetProgramInformation();
            if (programInformation == null)
                return;
            ApplicationHelper.Instance.VersionInformation = programInformation.ProductVersion.Description;
        }

        private void ShowRecentCadDrawings()
        {
            CatchException(() =>
            {
                var viewModel = ViewProvider.ViewModelFactory.GetRecentCadDrawingsViewModel(_loginScopeUi);
                ViewProvider.Show(viewModel);
            });
        }

        private void CreateProject()
        {
            CatchException(() =>
            {
                var operationInfo = _loginScopeUi.CanSetApplicationHandle(ViewProvider.GetHandle(this));
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanSetApplicationHandle))) return;

                _loginScopeUi.SetApplicationHandle(ViewProvider.GetHandle(this));

                operationInfo = LoginScopeUi.CanCreateProject();
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanCreateProject))) return;

                var coreObjectResult = LoginScopeUi.CreateProject();

                if (coreObjectResult.OperationCode == OperationCode.Rejected)
                    return;

                var project = coreObjectResult;

                var projectViewModel = ViewProvider.ViewModelFactory.GetProjectViewModel(project);
                ViewProvider.Show(projectViewModel);
            });
        }

        public bool CanShowElementTypes()
        {
            return IsLoggedIn();
        }

        private void ShowElementTypes()
        {
            CatchException(() =>
            {
                var elementTypesViewModel = ViewProvider.ViewModelFactory.GetElementTypesViewModel(LoginScopeUi);
                Show(elementTypesViewModel);
            });
        }

        private void OpenSynchronizationContainer()
        {
            CatchException(() =>
            {
                if (_synchronizationContainerViewModel != null) return;

                _synchronizationContainerViewModel = ViewProvider.ViewModelFactory.GetSynchronizationContainerViewModel(_loginScopeUi);
                _synchronizationContainerViewModel.SynchronizedEventReceived +=
                    SynchronizationViewModelOnSynchronizedEventReceived;
                Show(_synchronizationContainerViewModel, delegate { _synchronizationContainerViewModel = null; });
            });
        }

        private void SynchronizationViewModelOnSynchronizedEventReceived(object sender,
            SynchronizedEventReceivedEventArgs eventArgs)
        {
            CatchException(() =>
            {
                var synchronizedEvent = eventArgs.SynchronizedEvent;
                _loginScopeUi.SynchronizationContainer.SetHandled(synchronizedEvent);
            });
        }
    }
}
