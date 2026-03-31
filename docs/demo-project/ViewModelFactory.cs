using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.ViewModels;
using Ofcas.Lk.Api.Client.Ui;
using Ofcas.Lk.Api.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ofcas.Lk.Api.Client.Demo
{
    public class ViewModelFactory
    {
        public IViewProvider ViewProvider { get; }

        public ViewModelFactory(IViewProvider viewProvider)
        {
            ViewProvider = viewProvider;
        }

        public virtual LoginViewModel GetLoginViewModel()
        {
            return new LoginViewModel(ViewProvider);
        }

        public virtual CreatePhaseViewModel GetCreatePhaseViewModel()
        {
            return new CreatePhaseViewModel();
        }

        public virtual DocumentContainerViewModel GetDocumentContainerViewModel(IDocumentContainer documentContainer)
        {
            return new DocumentContainerViewModel(documentContainer, ViewProvider);
        }

        public virtual ElevationViewModel GetElevationViewModel(ICoreObjectResult<IElevationUi> elevationUi)
        {
            return new ElevationViewModel(ViewProvider, elevationUi);
        }

        public virtual LoginProgramsViewModel GetLoginProgramsViewModel(ILoginScopeUi loginScopeUi)
        {
            return new LoginProgramsViewModel(ViewProvider, loginScopeUi);
        }

        public virtual ProjectCenterViewModel GetProjectCenterViewModel(ICoreObjectResult<IProjectCenterUi> projectCenterUi)
        {
            return new ProjectCenterViewModel(ViewProvider, projectCenterUi);
        }

        public virtual ProjectProgramsViewModel GetProjectProgramsViewModel(IProjectUi project)
        {
            return new ProjectProgramsViewModel(ViewProvider, project);
        }

        public virtual ProjectViewModel GetProjectViewModel(ICoreObjectResult<IProjectUi> project)
        {
            return new ProjectViewModel(ViewProvider, project);
        }

        public virtual RecentProjectsViewModel GetRecentProjectsViewModel(ILoginScopeUi loginScopeUi)
        {
            return new RecentProjectsViewModel(ViewProvider, loginScopeUi);
        }

        public virtual SelectElevationEditModeViewModel GetSelectElevationEditModeViewModel(IEnumerable<ElevationEditModeModel> elevationEditModes)
        {
            return new SelectElevationEditModeViewModel(elevationEditModes);
        }

        public virtual ElevationInstanceViewModel GetElevationInstanceViewModel(ICoreObjectResult<IElevationInstance> elevationInstance)
        {
            return new ElevationInstanceViewModel(ViewProvider, elevationInstance);
        }

        public virtual PhaseViewModel GetPhaseViewModel(ICoreObjectResult<IPhaseUi> phaseUi)
        {
            return new PhaseViewModel(ViewProvider, phaseUi);
        }

        public virtual SynchronizationContainerViewModel GetSynchronizationContainerViewModel(ICoreObjectWithSynchronizations coreObjectWithSynchronizations)
        {
            return new SynchronizationContainerViewModel(ViewProvider, coreObjectWithSynchronizations);
        }

        public virtual ParametersViewModel GetParametersViewModel(bool canAddParams = true)
        {
            return new ParametersViewModel(ViewProvider, canAddParams);
        }

        public virtual ParametersViewModel GetParametersViewModel(ObservableCollection<IParameter> parameters, bool canAddParams = true)
        {
            return new ParametersViewModel(ViewProvider, parameters, canAddParams);
        }

        public virtual SelectElevationsViewModel GetSelectElevationsViewModel(IProject project)
        {
            return new SelectElevationsViewModel(project);
        }

        public virtual SelectElevationInstancesViewModel GetSelectElevationInstancesViewModel(IProject project)
        {
            return new SelectElevationInstancesViewModel(project);
        }

        public virtual SelectPhasesViewModel GetSelectPhasesViewModel(IProject project)
        {
            return new SelectPhasesViewModel(project);
        }

        public virtual SelectProjectInformationsViewModel GetSelectProjectInformationsViewModel(IEnumerable<IProjectInformationType> projectInformation)
        {
            return new SelectProjectInformationsViewModel(projectInformation);
        }

        public virtual ProjectInformationContainerViewModel GetProjectInformationContainerViewModel(IProjectInformationContainerUi projectInformationContainerUi,
            IProjectInformationType projectInformationType)
        {
            return new ProjectInformationContainerViewModel(ViewProvider, projectInformationContainerUi, projectInformationType);
        }

        public virtual ElementPricelistContainerViewModel GetElementPricelistContainerViewModel(IElementPricelistContainer elementPricelistContainer)
        {
            return new ElementPricelistContainerViewModel(ViewProvider, elementPricelistContainer);
        }

        public virtual ElementPricelistViewModel GetElementPricelistViewModel(ICoreObjectResult<IElementPricelist> elementPricelist)
        {
            return new ElementPricelistViewModel(ViewProvider, elementPricelist);
        }

        public virtual CalculatedElementPricelistViewModel GetCalculatedElementPricelistViewModel(ICalculatedElementPricelistResult calculatedElementPricelist)
        {
            return new CalculatedElementPricelistViewModel(calculatedElementPricelist);
        }

        public virtual SettingContainerViewModel GetSettingContainerViewModel(ISettingContainer settingContainer)
        {
            return new SettingContainerViewModel(ViewProvider, settingContainer);
        }

        public virtual ExceptionDetailsViewModel GetExceptionDetailsViewModel(ISynchronizationException synchronizationException)
        {
            return new ExceptionDetailsViewModel(synchronizationException);
        }

        public virtual ExceptionDetailsViewModel GetExceptionDetailsViewModel(Exception exception, string calledMethod)
        {
            return new ExceptionDetailsViewModel(exception, calledMethod);
        }

        public virtual MessageViewModel GetMessageViewModel(string message, string dialogTitle, Action<MessageViewModel> detailsAction,
            Action sendStatusReportAction = null)
        {
            return new MessageViewModel(ViewProvider, message, dialogTitle, detailsAction, sendStatusReportAction);
        }

        public virtual CoreObjectDetailsViewModel GetCoreObjectDetailsViewModel(ICoreObject coreObject)
        {
            return new CoreObjectDetailsViewModel(coreObject);
        }

        public virtual RecentFabricationLotsViewModel GetRecentFabricationLotsViewModel(ILoginScopeUi loginScopeUi)
        {
            return new RecentFabricationLotsViewModel(ViewProvider, loginScopeUi);
        }

        public virtual SelectReportsViewModel GetSelectReportsViewModel(IEnumerable<IReportItem> reportItems)
        {
            return new SelectReportsViewModel(reportItems);
        }

        public virtual LanguagesViewModel GetLanguagesViewModel(ILoginScopeUi loginScopeUi)
        {
            return new LanguagesViewModel(loginScopeUi);
        }

        public virtual UserInformationViewModel GetUserInformationViewModel(ILoginScope loginScope)
        {
            return new UserInformationViewModel(loginScope);
        }

        public virtual RecentCadDrawingsViewModel GetRecentCadDrawingsViewModel(ILoginScopeUi loginScopeUi)
        {
            return new RecentCadDrawingsViewModel(ViewProvider, loginScopeUi);
        }

        public virtual SelectCadDrawingViewModel GetSelectCadDrawingViewModel(ILoginScope loginScope)
        {
            return new SelectCadDrawingViewModel(ViewProvider, loginScope);
        }

        public virtual ProjectStatusesViewModel GetProjectStatusesViewModel(IProjectCenterUi projectCenterUi)
        {
            return new ProjectStatusesViewModel(projectCenterUi);
        }

        public virtual ElementTypesViewModel GetElementTypesViewModel(ILoginScopeUi loginScopeUi)
        {
            return new ElementTypesViewModel(loginScopeUi);
        }

        public virtual ElevationHistoryViewModel GetElevationHistoryViewModel(IElevationUi elevationUi)
        {
            return new ElevationHistoryViewModel(ViewProvider, elevationUi);
        }

        public virtual ProjectSearchViewModel GetProjectSearchViewModel(ISearchAgent searchAgent)
        {
            return new ProjectSearchViewModel(ViewProvider, searchAgent);
        }

        public virtual SelectValueViewModel<T> GetSelectValueViewModel<T>(T defaultValue, IEnumerable<T> validValues)
        {
            return new SelectValueViewModel<T>(defaultValue, validValues);
        }

        public virtual SelectValueViewModel<bool> GetSelectBoolViewModel(string title, bool defaultValue)
        {
            return new SelectValueViewModel<bool>(title, defaultValue, new[] { true, false });
        }

        public virtual InputBoxViewModel<T> GetInputBoxViewModel<T>(string title, T value = default(T))
        {
            return new InputBoxViewModel<T>(title, value);
        }

        public virtual SaveFileDialogViewModel GetSaveFileDialogViewModel()
        {
            return new SaveFileDialogViewModel();
        }

        public virtual SaveFileDialogViewModel GetSaveFileDialogViewModel(string filter, string defaultExtension = null)
        {
            var saveFileDialogViewModel = new SaveFileDialogViewModel
            {
                Filter = filter,
                DefaultExt = defaultExtension
            };
            return saveFileDialogViewModel;
        }

        public virtual OpenFileDialogViewModel GetOpenFileDialogViewModel()
        {
            return new OpenFileDialogViewModel();
        }

        public virtual DocumentViewModel GetDocumentViewModel(ICoreObjectResult<IDocument> document)
        {
            return new DocumentViewModel(ViewProvider, document);
        }

        public ElevationWarningsViewModel GetElevationWarningsViewModel(IElevationUi elevationUi)
        {
            return new ElevationWarningsViewModel(elevationUi);
        }

        public ElevationQuotationTextsViewModel GetElevationQuotationTextsViewModel(
            string elevationName, IEnumerable<IQuotationText> quotationTexts)
        {
            return new ElevationQuotationTextsViewModel(elevationName, quotationTexts);
        }

        public ElevationCommentsViewModel GetElevationCommentsViewModel(
            string elevationName, IEnumerable<IElevationComment> elevationComments)
        {
            return new ElevationCommentsViewModel(elevationName, elevationComments);
        }

        public SelectElevationInstancesViewModel GetSelectElevationInstanceViewModel(IElevation elevation)
        {
            return new SelectElevationInstancesViewModel(elevation);
        }

        public EstimationDataSetContainerViewModel GetEstimationDataSetContainerViewModel(IEstimationDataSetContainer estimationDataSetContainer)
        {
            return new EstimationDataSetContainerViewModel(ViewProvider, estimationDataSetContainer);
        }

        public EstimationDataSetViewModel GetEstimationDataSetViewModel(IEstimationDataSetInfo estimationDataSetInfo)
        {
            return new EstimationDataSetViewModel(ViewProvider, estimationDataSetInfo);
        }
    }
}
