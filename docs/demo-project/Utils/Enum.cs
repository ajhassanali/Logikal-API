using System.Collections.Generic;
using System.Linq;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public static class Enum
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return System.Enum.GetValues(typeof(T)).OfType<T>();
        }
    }
}