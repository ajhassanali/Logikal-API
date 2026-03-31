using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class ApplicationHelper : INotifyPropertyChanged
    {
        public static ApplicationHelper Instance { get; } = new ApplicationHelper();

        private string _versionInformation;

        public string VersionInformation
        {
            get { return _versionInformation; }
            set { _versionInformation = value; OnPropertyChanged(nameof(VersionInformation)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
