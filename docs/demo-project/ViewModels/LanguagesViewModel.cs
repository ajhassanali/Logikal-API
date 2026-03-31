using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Client.Ui;
using Ofcas.Lk.Api.Shared;
using System.Collections.Generic;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class LanguagesViewModel : ViewModelBase
    {
        public IList<ILanguage> Languages { get; }

        public LanguagesViewModel(ILoginScopeUi loginScopeUi)
        {
            var operationInfo = loginScopeUi.CanGetLanguages();
            if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanGetLanguages))) return;

            Languages = loginScopeUi.GetLanguages().Languages;
        }
    }
}
