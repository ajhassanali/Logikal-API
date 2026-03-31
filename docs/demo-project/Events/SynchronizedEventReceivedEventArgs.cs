using Ofcas.Lk.Api.Shared;
using System;

namespace Ofcas.Lk.Api.Client.Demo.Events
{
    public class SynchronizedEventReceivedEventArgs : EventArgs
    {
        public ISynchronizedEvent SynchronizedEvent { get; }

        public SynchronizedEventReceivedEventArgs(ISynchronizedEvent synchronizedEvent)
        {
            SynchronizedEvent = synchronizedEvent;
        }
    }
}