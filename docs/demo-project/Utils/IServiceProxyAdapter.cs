using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Ui;
using System;
using System.Collections.Generic;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public interface IServiceProxyAdapter : IDisposable
    {
        IResult Start();

        IResult Stop();

        IProgramInformationResult GetProgramInformation();

        ICoreObjectResult<ILoginScopeUi> Login(IDictionary<string, object> parameters);
    }
}
