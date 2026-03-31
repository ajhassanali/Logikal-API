using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Client.Ui;
using Ofcas.Lk.Api.Shared;
using System.Collections.Generic;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ElementTypesViewModel : ViewModelBase
    {
        public IList<IElementType> ElementTypes { get; }

        public ElementTypesViewModel(ILoginScopeUi loginScopeUi)
        {
            var operationInfo = loginScopeUi.CanGetElementTypes();
            if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScopeUi.CanGetElementTypes))) return;

            ElementTypes = loginScopeUi.GetElementTypes().ElementTypes;
        }
    }
}
