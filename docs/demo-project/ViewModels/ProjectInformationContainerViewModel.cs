using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Client.Ui;
using System.Windows.Input;
using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;
using Ofcas.Lk.Api.Shared;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ProjectInformationContainerViewModel : CoreObjectViewModel
    {
        private IProjectInformationContainerUi _projectInformationContainerUi;
        private readonly IProjectInformationType _projectInformationType;

        private ProjectInformationContainerModel _projectInformationContainerModel;

        public ProjectInformationContainerModel ProjectInformationContainerModel
        {
            get { return _projectInformationContainerModel; }
            set { _projectInformationContainerModel = value; OnPropertyChanged(); }
        }

        public SelectionAdapter<IProjectInformationInfo> ProjectInformations { get; } =
            new SelectionAdapter<IProjectInformationInfo>();

        public ICommand ShowModalCommand { get; }
        public ICommand CopyProjectInformationCommand { get; }
        public ICommand CalculateCommand { get; }

        public ProjectInformationContainerViewModel(IViewProvider viewProvider,
            IProjectInformationContainerUi projectInformationContainerUi,
            IProjectInformationType projectInformationType) : base(viewProvider, projectInformationContainerUi)
        {
            Throw.IfNull(projectInformationContainerUi, nameof(projectInformationContainerUi));
            Throw.IfNull(projectInformationType, nameof(projectInformationType));

            _projectInformationContainerUi = projectInformationContainerUi;
            _projectInformationType = projectInformationType;

            Refresh();
            RefreshChildren();

            ShowModalCommand = new Command(ShowModal);
            CopyProjectInformationCommand = new Command(CopyProjectInformation);
            CalculateCommand = new Command(Calculate);
        }

        private void ShowModal()
        {
            CatchException(() =>
            {
                var parameters = GetParameters();
                if (parameters == null)
                    return;

                var operationInfo = _projectInformationContainerUi.CanShow(parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectInformationContainerUi.CanShow))) return;

                _projectInformationContainerUi.ShowModal(parameters);

                Refresh();
                RefreshChildren(true);
            });
        }

        private Dictionary<string, object> GetParameters()
        {
            if (_projectInformationType.Id == WellKnownProjectInformationType.Comment)
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

            return new Dictionary<string, object>();
        }

        private void Calculate()
        {
            CatchException(() =>
            {
                var operationInfo = _projectInformationContainerUi.CanCalculate();
                if (!operationInfo.CheckForAnyRestriction(nameof(IProjectInformationContainer.CanCalculate))) return;

                _projectInformationContainerUi.Calculate();

                Refresh();
                RefreshChildren(true);
            });
        }

        private void CopyProjectInformation()
        {
            var text = string.Empty;
            foreach (var projectInformationInfo in _projectInformationContainerUi.ChildrenInfos)
            {
                text += projectInformationInfo.Id + Environment.NewLine;
                foreach (var setting in projectInformationInfo.Settings)
                    text += "\t" + setting.Key + ": " + setting.Value + Environment.NewLine;
            }

            ClipboardHelper.CopyObjectToWindowsClipboard(text);
        }

        private void RefreshChildren(bool hardRefresh = false)
        {
            if (hardRefresh)
                _projectInformationContainerUi.RefreshChildren();

            ProjectInformations.Load(_projectInformationContainerUi.ChildrenInfos);
        }

        private void Refresh(bool hardRefresh = false)
        {
            if (_projectInformationContainerUi == null)
                return;

            if (hardRefresh)
                _projectInformationContainerUi.RefreshChildren();

            var operationInfo = _projectInformationContainerUi.CanGetCalculationRequired();
            if (!operationInfo.CheckForAnyRestriction(nameof(IProjectInformationContainer.CanGetCalculationRequired))) return;

            var calculationRequired = _projectInformationContainerUi.GetCalculationRequired();
            ProjectInformationContainerModel = new ProjectInformationContainerModel
            {
                CoreObjectId = _projectInformationContainerUi.Id,
                Type = _projectInformationType,
                CalculationRequired = calculationRequired.Value
            };
        }
    }
}
