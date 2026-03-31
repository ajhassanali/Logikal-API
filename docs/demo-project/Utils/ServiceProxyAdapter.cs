using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Client.Ui;
using System;
using System.Collections.Generic;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    // Adapter used for internal purposes.
    internal class ServiceProxyAdapter : IServiceProxyAdapter
    {
        private IServiceProxyUi _serviceProxy;

        public ServiceProxyAdapter(IServiceProxyUi serviceProxy)
        {
            _serviceProxy = serviceProxy;
        }

        public IResult Start()
        {
            return _serviceProxy.Start();
        }

        public IResult Stop()
        {
            return _serviceProxy.Stop();
        }

        public IProgramInformationResult GetProgramInformation()
        {
            var operationInfo = _serviceProxy.CanGetProgramInformation();
            if (!operationInfo.CheckForAnyRestriction(nameof(IServiceProxyUi.CanGetProgramInformation))) return default(IProgramInformationResult);

            return _serviceProxy.GetProgramInformation();
        }

        public ICoreObjectResult<ILoginScopeUi> Login(IDictionary<string, object> parameters)
        {
            var operationInfo = _serviceProxy.CanLogin(parameters);
            if (!operationInfo.CheckForAnyRestriction(nameof(IServiceProxyUi.CanLogin))) return default(ICoreObjectResult<ILoginScopeUi>);

            return _serviceProxy.Login(parameters);
        }

        public void Dispose()
        {
            _serviceProxy.Dispose();
        }
    }
}
