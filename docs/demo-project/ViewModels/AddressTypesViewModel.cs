using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class AddressTypesViewModel : ViewModelBase
    {
        public IList<IAddressType> AddressTypes { get; }

        public AddressTypesViewModel(IAddressContainer addressContainer)
        {
            AddressTypes = addressContainer.ChildrenInfos.Select(x => x.Type).ToList();
        }
    }
}
