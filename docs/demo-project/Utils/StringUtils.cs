using System;
using System.Collections.Generic;
using System.Linq;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public static class StringUtils
    {
        public static string ToDetailedString(IList<IDictionary<string, object>> dictionaryList)
        {
            if (dictionaryList == null)
                return "<null>";

            var str = "";
            foreach (var dict in dictionaryList)
                str += "{" + string.Join(",", dict.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}" + Environment.NewLine;
            return str;
        }
    }
}
