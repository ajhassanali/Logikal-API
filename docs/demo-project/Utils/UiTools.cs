using System;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class UiTools
    {
        public static DateTime UnixToDateTime(ulong stamp)
            => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(stamp).ToUniversalTime();
    }
}
