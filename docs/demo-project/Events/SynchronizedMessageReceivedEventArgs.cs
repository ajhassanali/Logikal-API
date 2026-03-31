using Ofcas.Lk.Api.Shared;
using System;

namespace Ofcas.Lk.Api.Client.Demo.Events
{
    public class SynchronizedMessageReceivedEventArgs : EventArgs
    {
        public ISynchronizedMessage SynchronizedMessage { get; }

        public SynchronizedMessageReceivedEventArgs(ISynchronizedMessage synchronizedMessage)
        {
            SynchronizedMessage = synchronizedMessage;
        }
    }
}
