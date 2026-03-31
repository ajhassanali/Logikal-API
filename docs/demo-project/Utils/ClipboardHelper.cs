using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public static class ClipboardHelper
    {
        public static object Data { get; set; }

        public static void CopyObjectToWindowsClipboard(object data)
        {
            // Only one process can access the win32 clipboard at a time and throws an error otherwise
            // A similar loop is implement in the default implementation of the Windows Forms clipboard

            const uint clipboardCantOpen = 0x800401D0;

            for (var i = 0; i < 10; i++)
            {
                var hadError = false;

                try
                {
                    Clipboard.SetDataObject(data, false);
                }
                catch (COMException exception)
                {
                    hadError = true;
                    if ((uint)exception.ErrorCode != clipboardCantOpen)
                        throw;
                }

                if (hadError)
                    Thread.Sleep(100);
                else
                    break;
            }
        }
    }
}
