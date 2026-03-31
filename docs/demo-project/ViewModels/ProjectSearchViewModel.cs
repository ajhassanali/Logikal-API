using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Events;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Shared;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ProjectSearchViewModel : CoreObjectViewModel
    {
        private SynchronizationContainerViewModel _synchronizationContainerViewModel;
        private ISearchAgent _searchAgent;

        private bool _searchInProgress;

        public string Query { get; set; }

        public ICommand OpenSynchronizationContainerCommand { get; }
        public ICommand BeginSearchCommand { get; }

        public ProjectSearchViewModel(IViewProvider viewProvider, ISearchAgent searchAgent)
            : base(viewProvider, searchAgent)
        {
            Throw.IfNull(searchAgent, nameof(searchAgent));
            _searchAgent = searchAgent;

            OpenSynchronizationContainerCommand = new Command(OpenSynchronizationContainer);
            BeginSearchCommand = new Command(BeginSearch, CanBeginSearch);
        }

        private void OpenSynchronizationContainer()
        {
            CatchException(() =>
            {
                if (_synchronizationContainerViewModel != null) return;

                _synchronizationContainerViewModel = ViewProvider.ViewModelFactory.GetSynchronizationContainerViewModel(_searchAgent);
                _synchronizationContainerViewModel.SynchronizedEventReceived +=
                    SynchronizationViewModelOnSynchronizedEventReceived;
                Show(_synchronizationContainerViewModel, delegate { OnSynchronizationContainerClosed(); });
            });
        }

        private void SynchronizationViewModelOnSynchronizedEventReceived(object sender,
            SynchronizedEventReceivedEventArgs eventArgs)
        {
            CatchException(() =>
            {
                var synchronizedEvent = eventArgs.SynchronizedEvent;
                // nothing to do yet
                _searchAgent.SynchronizationContainer.SetHandled(synchronizedEvent);
            });
        }

        private void OnSynchronizationContainerClosed()
        {
            _synchronizationContainerViewModel.SynchronizedEventReceived -= SynchronizationViewModelOnSynchronizedEventReceived;
            _synchronizationContainerViewModel = null;
        }

        private bool CanBeginSearch()
        {
            return !_searchInProgress;
        }

        private Parameter<string> GetSearchTypesParameters()
        {
            var parameter = new Parameter<string>
            {
                Key = WellKnownParameterKey.SearchAgent.Type,
                IsRequired = true
            };

            parameter.Value = SearchAgentParameters.Types.Project.DisplayName;
            parameter.RestrictedValues = new List<string>
            {
                SearchAgentParameters.Types.Unknown.DisplayName,
                SearchAgentParameters.Types.Project.DisplayName
            };

            return parameter;
        }

        private bool BeforeSearch(out Dictionary<string, object> parameters)
        {
            parameters = default(Dictionary<string, object>);

            var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(new ObservableCollection<IParameter>());

            var searchTypeParameters = GetSearchTypesParameters();
            parametersViewModel.Parameters.Add(searchTypeParameters);
            parametersViewModel.Parameters.Add(
                new Parameter<string>
                {
                    IsRequired = false,
                    Key = WellKnownParameterKey.SearchAgent.StartFolder,
                });

            if (!ShowDialog(parametersViewModel))
                return false;

            parameters = parametersViewModel.GetParameters();

            return true;
        }

        private void BeginSearch()
        {
            CatchException(() =>
            {
                Dictionary<string, object> parameters;

                if (!BeforeSearch(out parameters))
                    return;

                parameters.Add(WellKnownParameterKey.SearchAgent.Query, Query);

                var operationInfo = _searchAgent.CanBeginFind(parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(ISearchAgent.CanBeginFind))) return;

                var operation = _searchAgent.BeginFind(parameters).SynchronizedOperation;
                _searchInProgress = true;
                OpenSynchronizationContainer();

                new Thread(() =>
                {
                    CatchException(() =>
                    {
                        operationInfo = _searchAgent.CanEndFind(operation);
                        if (!operationInfo.CheckForAnyRestriction(nameof(ISearchAgent.CanEndFind))) return;

                        _searchAgent.EndFind(operation);
                        Application.Current.Dispatcher.Invoke(() => { ShowMessage("Search finished."); });
                    }, nameof(BeginSearch), true);
                }).Start();
            });
            _searchInProgress = false;
        }
    }
}
