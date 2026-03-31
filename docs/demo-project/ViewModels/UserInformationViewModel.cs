using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class UserInformationViewModel : ViewModelBase
    {
        public IUserInformationResult UserInformation { get; }

        public UserInformationViewModel(ILoginScope loginScope)
        {
            var operationInfo = loginScope.CanGetUserInformation();
            if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScope.CanGetUserInformation))) return;

            UserInformation = loginScope.GetUserInformation();
        }
    }
}
