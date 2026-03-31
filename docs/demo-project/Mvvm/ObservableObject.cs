using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ofcas.Lk.Api.Client.Demo.Mvvm
{
    public sealed class ObservableObject<TypeOfObject> : INotifyPropertyChanged
    {
        public ObservableObject(TypeOfObject value, ObservableObjectValueChanged onValueChanged) : base()
        {
            _onValueChanged = onValueChanged;
            Value = value;
        }

        public ObservableObject(TypeOfObject value) : this(value, null)
        {
        }

        public delegate void ObservableObjectValueChanged(TypeOfObject oldValue, TypeOfObject newValue);

        private ObservableObjectValueChanged _onValueChanged { get; }

        private TypeOfObject _value { get; set; }

        public TypeOfObject Value
        {
            get
            {
                return _value;
            }
            set
            {
                TypeOfObject oldValue = _value;
                _value = value;

                OnPropertyChanged(nameof(Value));
                _onValueChanged?.Invoke(oldValue, _value);
            }
        }

        public static implicit operator TypeOfObject(ObservableObject<TypeOfObject> observableObject)
        {
            return observableObject.Value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}