using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.ViewModels;
using Ofcas.Lk.Api.Client.Demo.Views;
using Ofcas.Lk.Api.Shared;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Ofcas.Lk.Api.Client.Demo
{
    public partial class App
    {
        public ViewProvider ViewProvider { get; set; } = new ViewProvider();

        private static void RegisterViews(IViewProvider viewProvider)
        {
            viewProvider.Register<LoginViewModel, LoginWindow>();
            viewProvider.Register<CreatePhaseViewModel, CreatePhaseWindow>();
            viewProvider.Register<DocumentContainerViewModel, DocumentContainerWindow>();
            viewProvider.Register<ElevationViewModel, ElevationWindow>();
            viewProvider.Register<LoginProgramsViewModel, LoginProgramsWindow>();
            viewProvider.Register<ProjectCenterViewModel, ProjectCenterWindow>();
            viewProvider.Register<ProjectProgramsViewModel, ProjectProgramsWindow>();
            viewProvider.Register<ProjectViewModel, ProjectWindow>();
            viewProvider.Register<RecentProjectsViewModel, RecentProjectsWindow>();
            viewProvider.Register<SelectElevationEditModeViewModel, SelectElevationEditModeWindow>();
            viewProvider.Register<ElevationInstanceViewModel, ElevationInstanceWindow>();
            viewProvider.Register<PhaseViewModel, PhaseWindow>();
            viewProvider.Register<InputBoxViewModel<int>, InputBoxWindow>();
            viewProvider.Register<InputBoxViewModel<string>, InputBoxWindow>();
            viewProvider.Register<InputBoxViewModel<bool>, InputBoxWindow>();
            viewProvider.Register<SynchronizationContainerViewModel, SynchronizationContainerWindow>();
            viewProvider.Register<SelectValueViewModel<View>, SelectValueWindow>();
            viewProvider.Register<SelectValueViewModel<DatabaseExportType>, SelectValueWindow>();
            viewProvider.Register<ParametersViewModel, ParametersWindow>();
            viewProvider.Register<SelectElevationsViewModel, SelectElevationsWindow>();
            viewProvider.Register<SelectPhasesViewModel, SelectPhasesWindow>();
            viewProvider.Register<InputBoxViewModel<double>, InputBoxWindow>();
            viewProvider.Register<SelectProjectInformationsViewModel, SelectProjectInformationsWindow>();
            viewProvider.Register<ProjectInformationContainerViewModel, ProjectInformationContainerWindow>();
            viewProvider.Register<ElementPricelistContainerViewModel, ElementPricelistContainerWindow>();
            viewProvider.Register<ElementPricelistViewModel, ElementPricelistWindow>();
            viewProvider.Register<CalculatedElementPricelistViewModel, CalculatedElementPricelistWindow>();
            viewProvider.Register<InputBoxViewModel<Guid>, InputBoxWindow>();
            viewProvider.Register<SettingContainerViewModel, SettingContainerWindow>();
            viewProvider.Register<ExceptionDetailsViewModel, ExceptionDetailsWindow>();
            viewProvider.Register<SelectValueViewModel<bool>, SelectValueWindow>();
            viewProvider.Register<MessageViewModel, MessageWindow>();
            viewProvider.Register<CoreObjectDetailsViewModel, CoreObjectDetailsWindow>();
            viewProvider.Register<RecentFabricationLotsViewModel, RecentFabricationLotsWindow>();
            viewProvider.Register<SelectReportsViewModel, SelectReportsWindow>();
            viewProvider.Register<LanguagesViewModel, LanguagesWindow>();
            viewProvider.Register<UserInformationViewModel, UserInformationWindow>();
            viewProvider.Register<RecentCadDrawingsViewModel, RecentCadDrawingsWindow>();
            viewProvider.Register<SelectCadDrawingViewModel, SelectCadDrawingWindow>();
            viewProvider.Register<ProjectStatusesViewModel, ProjectStatusesWindow>();
            viewProvider.Register<ElementTypesViewModel, ElementTypesWindow>();
            viewProvider.Register<ElevationHistoryViewModel, ElevationHistoryWindow>();
            viewProvider.Register<ProjectSearchViewModel, ProjectSearchWindow>();
            viewProvider.Register<AddressContainerViewModel, AddressContainerWindow>();
            viewProvider.Register<AddressViewModel, AddressWindow>();
            viewProvider.Register<AddressTypesViewModel, AddressTypesWindow>();
            viewProvider.Register<ElevationQuotationTextsViewModel, ElevationQuotationTextWindow>();
            viewProvider.Register<DocumentViewModel, DocumentWindow>();
            viewProvider.Register<ElevationWarningsViewModel, ElevationWarningsWindow>();
            viewProvider.Register<SelectValueViewModel<SortChildrenBehavior>, SelectValueWindow>();
            viewProvider.Register<ElevationCommentsViewModel, ElevationCommentsWindow>();
            viewProvider.Register<SelectElevationInstancesViewModel, SelectElevationInstancesWindow>();
            viewProvider.Register<EstimationDataSetContainerViewModel, EstimationDataSetContainerWindow>();
            viewProvider.Register<EstimationDataSetViewModel, EstimationDataSetWindow>();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            RegisterViews(ViewProvider);
            ViewProvider.Show(ViewProvider.ViewModelFactory.GetLoginViewModel());
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
            Environment.Exit(-1);
        }
    }
}
