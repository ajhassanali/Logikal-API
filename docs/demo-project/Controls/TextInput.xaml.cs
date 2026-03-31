using System.Collections;
using System.Windows;

namespace Ofcas.Lk.Api.Client.Demo.Controls
{
    public partial class TextInput
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(TextInput), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header), typeof(string), typeof(TextInput), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty EntriesProperty = DependencyProperty.Register(
            nameof(Entries), typeof(IEnumerable), typeof(TextInput), new PropertyMetadata(default(IEnumerable)));

        public IEnumerable Entries
        {
            get { return (IEnumerable)GetValue(EntriesProperty); }
            set { SetValue(EntriesProperty, value); }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public TextInput()
        {
            InitializeComponent();
        }
    }
}