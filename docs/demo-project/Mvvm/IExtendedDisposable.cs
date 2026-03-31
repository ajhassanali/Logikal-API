using System;

namespace Ofcas.Lk.Api.Client.Demo.Mvvm
{
    public interface IExtendedDisposable : IDisposable
    {
        bool CanDispose(out string falseReason);
    }
}