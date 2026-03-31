using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class EnvironmentModel : INotifyPropertyChanged
    {
        private string _launcherPath;
        private string _programMode;

        private ObservableCollection<string> _recentLauncherPathes = new ObservableCollection<string>();
        private ObservableCollection<string> _programModes = new ObservableCollection<string>();

        public string LauncherPath
        {
            get { return _launcherPath; }
            set { _launcherPath = value; OnPropertyChanged(); }
        }

        public string ProgramMode
        {
            get { return _programMode; }
            set { _programMode = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> RecentLauncherPathes
        {
            get { return _recentLauncherPathes; }
            set { _recentLauncherPathes = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> ProgramModes
        {
            get { return _programModes; }
            set { _programModes = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddPathIfDoesNotExist(string path)
        {
            if (!RecentLauncherPathes.Contains(path, StringComparer.OrdinalIgnoreCase))
                RecentLauncherPathes.Add(path);
        }

        public void AddModeIfDoesNotExist(string mode)
        {
            if (!ProgramModes.Contains(mode, StringComparer.OrdinalIgnoreCase))
                ProgramModes.Add(mode);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
