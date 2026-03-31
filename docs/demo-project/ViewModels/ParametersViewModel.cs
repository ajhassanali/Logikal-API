using System;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ParametersViewModel : ParentViewModelBase
    {
        private readonly bool _canAddParameters;

        public ObservableCollection<IParameter> Parameters { get; }

        public ICommand AddParameterCommand { get; }
        public ICommand RemoveParameterCommand { get; }
        public ICommand OpenParameterInputHelperCommand { get; }

        public string Title { get; set; } = "Insert parameters";

        public ParametersViewModel(IViewProvider viewProvider, bool canAddParameters = true)
            : this(viewProvider, new ObservableCollection<IParameter>(), canAddParameters)
        {
        }

        public ParametersViewModel(IViewProvider viewProvider, ObservableCollection<IParameter> parameters,
            bool canAddParameters = true)
            : base(viewProvider)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            Parameters = parameters;
            _canAddParameters = canAddParameters;

            AddParameterCommand = new Command(AddParameter, CanAddParameter);
            RemoveParameterCommand = new Command<IParameter>(RemoveParameter, CanRemoveParameter);
            OpenParameterInputHelperCommand = new Command<IParameter>(OpenParameterInputHelper);
        }

        public Dictionary<string, object> GetParameters()
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var parameter in Parameters)
                dictionary.Add(parameter.Key, parameter.Value);

            return dictionary;
        }

        /// <summary>
        /// Sets the values for the parameters from the provided dictionary.
        /// </summary>
        /// <param name="providedParameters">The dictionary with the provided values.</param>
        /// <param name="createMissingParameters">If set to false only the values for already existing parameters are set.</param>
        public void SetParameters(Dictionary<string, object> providedParameters, bool createMissingParameters)
        {
            foreach (var providedParameter in providedParameters)
            {
                var parameter = Parameters.FirstOrDefault(x => x.Key == providedParameter.Key);
                if (parameter == null && createMissingParameters)
                {
                    parameter = new Parameter<object>
                    {
                        Key = providedParameter.Key,
                        Value = providedParameter.Value
                    };
                    Parameters.Add(parameter);
                    continue;
                }

                if (parameter != null)
                    parameter.Value = providedParameter.Value;
            }
        }

        private void OpenParameterInputHelper(IParameter parameter)
        {
            parameter.Value = parameter.InputHelper.Invoke();
        }

        private bool CanAddParameter()
        {
            return _canAddParameters;
        }

        private void AddParameter()
        {
            CatchException(() =>
            {
                Parameters.Add(new Parameter<string>
                {
                    IsRequired = false,
                    Key = ""
                });
            });
        }

        private bool CanRemoveParameter(IParameter parameter)
        {
            return !parameter.IsRequired;
        }

        private void RemoveParameter(IParameter parameter)
        {
            CatchException(() =>
            {
                Parameters.Remove(parameter);
            });
        }
    }
}
