using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.Mvvm
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public ObservableObject<bool?> DialogResult { get; } = new ObservableObject<bool?>(null);

        public ICommand AcceptCommand { get; }

        protected ViewModelBase()
        {
            AcceptCommand = new Command(Accept, CanAccept);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool CanAccept()
        {
            return true;
        }

        protected virtual void Accept()
        {
            DialogResult.Value = true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}