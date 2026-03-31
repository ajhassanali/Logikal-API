namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class ClipboardData<TSender, TContent> : IClipboardDataWithSender<TSender>,
        IClipboardDataWithContent<TContent>
    {
        public TSender Sender { get; set; }
        public TContent Content { get; set; }
    }
}