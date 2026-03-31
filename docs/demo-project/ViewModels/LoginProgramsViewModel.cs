using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Client.Ui;
using Ofcas.Lk.Api.Shared;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class LoginProgramsViewModel : ParentViewModelBase
    {
        private readonly ILoginScopeUi _loginScopeUi;

        public SelectionAdapter<ILoginRunnableProgram> Programs { get; } = new SelectionAdapter<ILoginRunnableProgram>();

        public ICommand ShowAsyncCommand { get; }

        public LoginProgramsViewModel(IViewProvider viewProvider, ILoginScopeUi loginScopeUi)
            : base(viewProvider)
        {
            _loginScopeUi = loginScopeUi;

            var operationInfo = _loginScopeUi.CanGetRunnablePrograms();
            if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScope.CanGetProgramRuntimeInformation))) return;

            var runnablePrograms = _loginScopeUi.GetRunnablePrograms().LoginRunnablePrograms;
            Programs.ItemsSource = new ObservableCollection<ILoginRunnableProgram>(runnablePrograms);

            ShowAsyncCommand = new AsyncCommand<ILoginRunnableProgram>(ShowAsync);
        }

        private async Task ShowAsync(ILoginRunnableProgram loginRunnableProgram)
        {
            await CatchException(async () =>
            {
                var parameters = GetParameters(loginRunnableProgram);
                if (parameters == null) return;
                
                if (!SetCadDrawing(loginRunnableProgram, parameters))
                    return;

                var operationInfo = _loginScopeUi.CanSetApplicationHandle(ViewProvider.GetHandle(this));
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanSetApplicationHandle))) return;

                _loginScopeUi.SetApplicationHandle(ViewProvider.GetHandle(this));

                operationInfo = _loginScopeUi.CanBeginShow(loginRunnableProgram, parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanBeginShow))) return;

                var synchronizedOperation = _loginScopeUi.BeginShow(loginRunnableProgram, parameters).SynchronizedOperation;

                operationInfo = _loginScopeUi.CanEndShow(synchronizedOperation);
                if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanEndShow))) return;

                var result = await Task.Run(() => _loginScopeUi.EndShow(synchronizedOperation))
                    .ConfigureAwait(true);

                if (result.OperationCode == OperationCode.Accepted)
                    ShowReturnValues(result);
            }).ConfigureAwait(true);
        }

        private Dictionary<string, object> GetParameters(ILoginRunnableProgram loginRunnableProgram)
        {
            var viewModel = ViewProvider.ViewModelFactory.GetParametersViewModel();

            if (loginRunnableProgram.Category.Id == WellKnownLoginPrograms.ProfileAndCurtainWallExport.CategoryId)
            {
                viewModel.Parameters.Add(new Parameter<string>
                {
                    Key = WellKnownParameterKey.LoginScope.Program.FileName,
                    Value = "",
                    IsRequired = true
                });
                viewModel.Parameters.Add(new Parameter<string>
                {
                    Key = WellKnownParameterKey.LoginScope.Program.ExportFormat,
                    Value = "",

                });

                viewModel.Parameters.Add(new Parameter<DxfVersion>
                {
                    Key = WellKnownParameterKey.LoginScope.Program.DxfVersion,
                    Value = DxfVersion.R12
                });
            }

            return !ShowDialog(viewModel) ? null : viewModel.GetParameters();
        }

        private bool SetCadDrawing(ILoginRunnableProgram loginRunnableProgram, Dictionary<string, object> parameters)
        {
            if (loginRunnableProgram.Id == WellKnownLoginPrograms.Programs.Cad &&
                loginRunnableProgram.Category.Id == WellKnownLoginPrograms.Programs.CategoryId)
            {
                var selectCadDrawing = ViewProvider.ViewModelFactory.GetSelectCadDrawingViewModel(_loginScopeUi);
                if (!ShowDialog(selectCadDrawing))
                    return false;

                if (selectCadDrawing.CadDrawings.SelectedItem != null)
                {
                    var cadDrawing = selectCadDrawing.CadDrawings.SelectedItem;
                    parameters.Add(WellKnownParameterKey.LoginScope.Program.CadFileUri, cadDrawing.Uri);
                }
            }
            return true;
        }

        private void ShowReturnValues(ILoginProgramResult result)
        {
            var returnParameters = new ObservableCollection<IParameter>();
            foreach (var keyValue in result.Values)
            {
                returnParameters.Add(new Parameter<string>
                {
                    IsRequired = true,
                    Key = keyValue.Key,
                    Value = keyValue.Value
                });
            }

            var viewModel = new ParametersViewModel(ViewProvider, returnParameters, false);
            ViewProvider.ShowDialog(viewModel, this);
        }
    }
}
