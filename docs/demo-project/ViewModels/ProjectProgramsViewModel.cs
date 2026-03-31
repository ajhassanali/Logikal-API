using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Client.Ui;
using Ofcas.Lk.Api.Shared;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ProjectProgramsViewModel : ParentViewModelBase
    {
        private readonly IProjectUi _projectUi;

        public SelectionAdapter<IProjectRunnableProgram> Programs { get; } = new SelectionAdapter<IProjectRunnableProgram>();

        public ICommand ShowAsyncCommand { get; }

        public ProjectProgramsViewModel(IViewProvider viewProvider, IProjectUi projectUi)
            : base(viewProvider)
        {
            _projectUi = projectUi;

            var runnablePrograms = projectUi.GetRunnablePrograms().ProjectRunnablePrograms;
            Programs.ItemsSource = new ObservableCollection<IProjectRunnableProgram>(runnablePrograms);

            ShowAsyncCommand = new AsyncCommand<IProjectRunnableProgram>(ShowAsync);
        }

        private async Task ShowAsync(IProjectRunnableProgram projectRunnableProgram)
        {
            await CatchException(async () =>
            {
                var parameters = GetParameters(projectRunnableProgram);
                if (parameters == null) return;

                var operationInfo = _projectUi.CanBeginShow(projectRunnableProgram, parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectUi.CanBeginShow))) return;

                var synchronizedOperation = _projectUi.BeginShow(projectRunnableProgram, parameters).SynchronizedOperation;

                _projectUi.SetApplicationHandle(ViewProvider.GetHandle(this));
                await Task.Run(() => { _projectUi.EndShow(synchronizedOperation); }).ConfigureAwait(false);
            }).ConfigureAwait(true);
        }

        private IList<IElevationInfo> SelectElevations()
        {
            var selectElevationsViewModel = ViewProvider.ViewModelFactory.GetSelectElevationsViewModel(_projectUi);
            if (!ShowDialog(selectElevationsViewModel))
                return null;

            return selectElevationsViewModel.Elevations.SelectedItems;
        }

        private Dictionary<string, object> GetParameters(IProjectRunnableProgram projectRunnableProgram)
        {
            const int crossSectionId = 41;

            if (projectRunnableProgram.Id == crossSectionId)
            {
                var selectedElevations = SelectElevations();
                if (selectedElevations.Count > 0)
                {
                    return new Dictionary<string, object>
                    {
                        [WellKnownParameterKey.Project.Program.ObjectGuid] = selectedElevations.First().Guid
                    };
                }
            }
            else if ((projectRunnableProgram.Id == WellKnownProjectPrograms.Preferences.ReportSettings &&
                projectRunnableProgram.Category.Id == WellKnownProjectPrograms.Preferences.CategoryId) ||
                (projectRunnableProgram.Id == WellKnownProjectPrograms.Datasafe.UploadToDatasafe &&
                projectRunnableProgram.Category.Id == WellKnownProjectPrograms.Datasafe.CategoryId))
            {
                var selectedElevations = SelectElevations();
                var elevationsParameter = new Parameter<string>
                {
                    Key = WellKnownParameterKey.Project.Program.SelectedElevations,
                    Value = string.Join(",", selectedElevations.Select(x => x.Guid.ToString("B"))),
                };

                var parameters = new ObservableCollection<IParameter>();
                parameters.Add(elevationsParameter);

                var viewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(parameters, true);
                if (ViewProvider.ShowDialog(viewModel, this).GetValueOrDefault())
                    return viewModel.GetParameters();
                return null;
            }
            else if (projectRunnableProgram.Id == WellKnownProjectPrograms.ProjectItems.ProjectComment &&
                projectRunnableProgram.Category.Id == WellKnownProjectPrograms.ProjectItems.CategoryId)
            {
                var parametersViewModel = new ParametersViewModel(ViewProvider, new ObservableCollection<IParameter>
                {
                    new Parameter<int>
                    {
                        Key = WellKnownParameterKey.Project.Program.ProjectCommentId,
                        RestrictedValues = Enumerable.Range(1,5).ToList()
                    },
                });
                if (!ViewProvider.ShowDialog(parametersViewModel, this).GetValueOrDefault())
                    return null;

                return parametersViewModel.GetParameters();
            }

            return new Dictionary<string, object>(0);
        }
    }
}
