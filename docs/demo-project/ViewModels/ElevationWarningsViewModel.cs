using System.Collections.Generic;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Ui;
using Ofcas.Lk.Api.Shared;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ElevationWarningsViewModel : ViewModelBase
    {
        public IList<IElevationWarning> ElevationWarnings { get; }

        public ElevationWarningsViewModel(IElevationUi elevationUi)
        {
            ElevationWarnings = elevationUi.Info.ElevationWarnings;
        }
    }
}
