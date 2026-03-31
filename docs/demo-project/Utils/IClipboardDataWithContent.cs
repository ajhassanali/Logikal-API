namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public interface IClipboardDataWithContent<out TContent>
    {
        TContent Content { get; }
    }
}