using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public static class RichtTextBoxExtensions
    {
        public static readonly DependencyProperty ContentStreamProperty = DependencyProperty.RegisterAttached(
            "ContentStream", typeof(Stream), typeof(RichtTextBoxExtensions),
            new PropertyMetadata(default(Stream), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs eventArgs)
        {
            var richTextBox = dependencyObject as RichTextBox;
            if (richTextBox == null) return;

            if (eventArgs.NewValue == null)
            {
                richTextBox.SelectAll();
                richTextBox.Selection.Text = "";
            }
            else
            {
                var contentStream = eventArgs.NewValue as Stream;
                if (contentStream == null) return;
                if (contentStream.Length == 0) return;

                richTextBox.SelectAll();
                richTextBox.Selection.Load(contentStream, DataFormats.Rtf);
                contentStream.Position = 0;
            }
        }

        public static void SetContentStream(RichTextBox richTextBox, Stream value)
        {
            richTextBox.SetValue(ContentStreamProperty, value);
        }

        public static Stream GetContentStream(RichTextBox richTextBox)
        {
            return (Stream)richTextBox.GetValue(ContentStreamProperty);
        }
    }
}