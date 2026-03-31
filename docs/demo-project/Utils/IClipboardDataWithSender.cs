namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public interface IClipboardDataWithSender<out TSender>
    {
        TSender Sender { get; }
    }
}