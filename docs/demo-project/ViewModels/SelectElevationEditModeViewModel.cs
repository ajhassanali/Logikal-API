using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class SelectElevationEditModeViewModel : ViewModelBase
    {
        public ObservableObject<ElevationEditModeModel> ElevationEditMode { get; }
        public ObservableCollection<ElevationEditModeModel> ElevationEditModes { get; }

        public SelectElevationEditModeViewModel(IEnumerable<ElevationEditModeModel> elevationEditModes)
        {
            ElevationEditModes = new ObservableCollection<ElevationEditModeModel>(elevationEditModes);
            ElevationEditMode = new ObservableObject<ElevationEditModeModel>(ElevationEditModes.FirstOrDefault());
        }
    }
}
