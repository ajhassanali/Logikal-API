using System.IO;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public static class StreamExtensions
    {
        public static void SetPosition(this Stream stream, long value)
        {
            if (stream.CanSeek)
                stream.Position = value;
        }
    }
}
