using System;
using System.Runtime.Serialization;

namespace Ofcas.Lk.Api.Client.Demo.Exceptions
{
    public class InfoIsNullException : Exception
    {
        private const string DefaultMessage = "Returned info is null.";

        public InfoIsNullException() : base(DefaultMessage)
        {
        }

        public InfoIsNullException(string message) : base(message)
        {
        }

        public InfoIsNullException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InfoIsNullException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}