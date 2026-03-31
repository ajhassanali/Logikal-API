using Ofcas.Lk.Api.Client.Demo.Mvvm;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class InputBoxViewModel<T> : ViewModelBase
    {
        public ObservableObject<string> Title { get; }
        public ObservableObject<T> Value { get; }

        public InputBoxViewModel(string title, T value = default(T))
        {
            Title = new ObservableObject<string>(title);
            Value = new ObservableObject<T>(value);
        }

        public T GetValue()
        {
            return Value;
        }
    }
}