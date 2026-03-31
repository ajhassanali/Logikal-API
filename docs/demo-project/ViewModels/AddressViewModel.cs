using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Shared;
using System;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class AddressViewModel : CoreObjectViewModel, IExtendedDisposable
    {
        private ICoreObjectResult<IAddress> AddressResult;
        private IAddress Address => AddressResult.CoreObject;

        private AddressModel _selectedAddress;

        public AddressModel SelectedAddress
        {
            get { return _selectedAddress; }
            set { _selectedAddress = value; OnPropertyChanged(); }
        }

        public event EventHandler AddressRefreshed;

        public ICommand EditCommand { get; }

        public AddressViewModel(IViewProvider viewProvider, ICoreObjectResult<IAddress> address)
            : base(viewProvider, address)
        {
            Throw.IfNull(address, nameof(address));
            AddressResult = address;

            EditCommand = new Command<string>(Edit);

            Refresh(true, true);
        }

        private void Edit(string key)
        {
            CatchException(() =>
            {
                var current = SelectedAddress;
                var oldValue = GetValueFromAddress(current, key);
                var viewModel = new InputBoxViewModel<string>("Edit value", oldValue);
                if (!ViewProvider.ShowDialog(viewModel, this).GetValueOrDefault(false))
                    return;

                var operationInfo = Address.CanEdit(key, viewModel.Value.Value);
                if (!operationInfo.CheckForAnyRestriction(nameof(IAddress.CanEdit))) return;

                Address.Edit(key, viewModel.Value.Value);

                Refresh(true, true);
            });
        }

        private void Refresh(bool hardRefresh = false, bool suppressEvent = false)
        {
            if (hardRefresh)
                Address.Refresh();

            SelectedAddress = new AddressModel
            {
                CoreObjectId = Address.Id,
                Type = Address.Info.Type.Name,
                Guid = Address.Info.Guid,
                Condition = Address.Info.Condition,
                Country = Address.Info.Country,
                CustomerNo = Address.Info.CustomerNo,
                CustomerRef = Address.Info.CustomerRef,
                EMail = Address.Info.EMail,
                FAO = Address.Info.FAO,
                Fax = Address.Info.Fax,
                Name1 = Address.Info.Name1,
                Name2 = Address.Info.Name2,
                Phone1 = Address.Info.Phone1,
                Phone2 = Address.Info.Phone2,
                Salutation = Address.Info.Salutation,
                Street = Address.Info.Street,
                City = Address.Info.City,
                ZipCode = Address.Info.ZipCode,
                FreeText1 = Address.Info.FreeText1,
                FreeText2 = Address.Info.FreeText2,
                FreeText3 = Address.Info.FreeText3,
                IsEmpty = Address.Info.IsEmpty
            };

            if (!suppressEvent)
                AddressRefreshed?.Invoke(this, EventArgs.Empty);
        }

        public bool CanDispose(out string falseReason)
        {
            return CanDispose(AddressResult, out falseReason);
        }

        public void Dispose()
        {
            AddressResult.Dispose();
            AddressResult = null;
        }

        private string GetValueFromAddress(AddressModel address, string key)
        {
            switch (key)
            {
                case WellKnownEditKey.Address.Salutation:
                    return address.Salutation;

                case WellKnownEditKey.Address.CustomerNo:
                    return address.CustomerNo;

                case WellKnownEditKey.Address.Name1:
                    return address.Name1;

                case WellKnownEditKey.Address.Name2:
                    return address.Name2;

                case WellKnownEditKey.Address.Street:
                    return address.Street;

                case WellKnownEditKey.Address.City:
                    return address.City;

                case WellKnownEditKey.Address.ZipCode:
                    return address.ZipCode;

                case WellKnownEditKey.Address.Phone1:
                    return address.Phone1;

                case WellKnownEditKey.Address.Phone2:
                    return address.Phone2;

                case WellKnownEditKey.Address.Fax:
                    return address.Fax;

                case WellKnownEditKey.Address.EMail:
                    return address.EMail;

                case WellKnownEditKey.Address.FAO:
                    return address.FAO;

                case WellKnownEditKey.Address.CustomerRef:
                    return address.CustomerRef;

                case WellKnownEditKey.Address.Condition:
                    return address.Condition;

                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, "Edit is not valid.");
            }
        }
    }
}
