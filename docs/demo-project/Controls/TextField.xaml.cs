using System.Windows;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.Controls
{
    public partial class TextField
    {
        public static readonly DependencyProperty EditCommandProperty = DependencyProperty.Register(
            nameof(EditCommand), typeof(ICommand), typeof(TextField), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty EditKeyProperty = DependencyProperty.Register(
            nameof(EditKey), typeof(string), typeof(TextField), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header), typeof(string), typeof(TextField), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(TextField), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty EditContentProperty = DependencyProperty.Register(
            nameof(EditContent), typeof(string), typeof(TextField), new PropertyMetadata("Edit"));

        public ICommand EditCommand
        {
            get { return (ICommand)GetValue(EditCommandProperty); }
            set { SetValue(EditCommandProperty, value); }
        }

        public string EditKey
        {
            get { return (string)GetValue(EditKeyProperty); }
            set { SetValue(EditKeyProperty, value); }
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

        public string EditContent
        {
            get => (string)GetValue(EditContentProperty);
            set => SetValue(EditContentProperty, value);
        }

        public TextField()
        {
            InitializeComponent();
        }
    }
}
