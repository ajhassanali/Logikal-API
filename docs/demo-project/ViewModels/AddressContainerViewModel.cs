using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class AddressContainerViewModel : CoreObjectViewModel
    {
        private IAddressContainer _addressContainer;

        public SelectionAdapter<IAddressInfo> Addresses { get; } = new SelectionAdapter<IAddressInfo>();

        public ICommand OpenChildCommand { get; }
        public ICommand ShowAddressTypesCommand { get; }

        public AddressContainerViewModel(IViewProvider viewProvider, IAddressContainer addressContainer)
            : base(viewProvider, addressContainer)
        {
            Throw.IfNull(addressContainer, nameof(addressContainer));
            _addressContainer = addressContainer;

            RefreshChildren();

            OpenChildCommand = new Command<IAddressInfo>(OpenChild);
            ShowAddressTypesCommand = new Command(ShowAddressTypes);
        }

        private void OpenChild(IAddressInfo addressInfo)
        {
            CatchException(() =>
            {
                if (addressInfo == null) return;

                var operationInfo = _addressContainer.CanGetChild(addressInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IAddressContainer.CanGetChild))) return;

                var address = _addressContainer.GetChild(addressInfo);
                var addressViewModel = new AddressViewModel(ViewProvider, address);
                Show(addressViewModel);
            });
        }

        private void ShowAddressTypes()
        {
            CatchException(() =>
            {
                var viewModel = new AddressTypesViewModel(_addressContainer);
                ViewProvider.Show(viewModel);
            });
        }

        private void RefreshChildren(bool hardRefresh = false)
        {
            if (hardRefresh)
                _addressContainer.RefreshChildren();

            var childrenInfos = _addressContainer.ChildrenInfos;

            Addresses.ItemsSource = new ObservableCollection<IAddressInfo>(childrenInfos);
        }
    }
}
