using Ofcas.Lk.Api.Client.Demo.Mvvm;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class CreatePhaseViewModel : ViewModelBase
    {
        public ObservableObject<string> Name { get; } = new ObservableObject<string>(string.Empty);
        public ObservableObject<string> Description { get; } = new ObservableObject<string>(string.Empty);
    }
}