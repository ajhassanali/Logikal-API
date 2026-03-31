using Ofcas.Lk.Api.Client.Core;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class LoginScopeProvider
    {
        private static LoginScopeProvider _instance;
        public static LoginScopeProvider Instance => _instance ?? (_instance = new LoginScopeProvider());

        public ILoginScope LoginScope { get; set; }

        private LoginScopeProvider()
        {
        }
    }
}
