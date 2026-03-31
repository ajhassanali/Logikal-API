using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Shared;
using Ofcas.Lk.Api.Client.Demo.Utils;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class SettingContainerViewModel : CoreObjectViewModel
    {
        private readonly ISettingContainer _settingContainer;

        protected ISettingContainer SettingContainer => _settingContainer;

        private IList<ISetting> _settings;
        public IList<ISetting> Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public ICommand EditCommand { get; }

        public SettingContainerViewModel(IViewProvider viewProvider, ISettingContainer settingContainer)
            : base(viewProvider, settingContainer)
        {
            _settingContainer = settingContainer;

            var operationInfo = _settingContainer.CanGetSettings();
            if (!operationInfo.CheckForAnyRestriction(nameof(ISettingContainer.CanGetSettings))) return;

            Settings = _settingContainer.GetSettings().Settings;
            EditCommand = new Command<string>(Edit, CanEdit);
        }

        private bool CanEdit(string editKey)
        {
            return Settings.Any(settingData => settingData.Key == editKey && settingData.Editable);
        }

        private void Edit(string editKey)
        {
            CatchException(() =>
            {
                foreach (var settingData in Settings)
                    if (settingData.Key == editKey)
                    {
                        var inputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel($"Edit {settingData.Key}",
                            settingData.Value);
                        if (!ShowDialog(inputBoxViewModel)) return;

                        var value = inputBoxViewModel.GetValue();

                        var operationInfo = _settingContainer.CanSetSettingValue(settingData.Key, value);
                        if (!operationInfo.CheckForAnyRestriction(nameof(ISettingContainer.CanSetSettingValue))) return;

                        _settingContainer.SetSettingValue(settingData.Key, value);

                        operationInfo = _settingContainer.CanGetSettings();
                        if (!operationInfo.CheckForAnyRestriction(nameof(ISettingContainer.CanGetSettings))) return;

                        Settings = _settingContainer.GetSettings().Settings;
                        break;
                    }
            });
        }
    }
}
