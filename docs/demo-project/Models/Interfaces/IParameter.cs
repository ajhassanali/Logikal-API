using System;
using System.Collections;

namespace Ofcas.Lk.Api.Client.Demo.Models.Interfaces
{
    public interface IParameter
    {
        bool IsRequired { get; }
        string Key { get; set; }
        object Value { get; set; }
        IList RestrictedValues { get; }
        Func<object> InputHelper { get; }
    }
}
