using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Shared;
using System.Collections.Generic;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ElevationCommentsViewModel : ViewModelBase
    {
        public string ElevationName { get; }

        public SelectionAdapter<IElevationComment> Comments { get; } = new SelectionAdapter<IElevationComment>();

        public ElevationCommentsViewModel(string elevationName, IEnumerable<IElevationComment> elevationComments)
        {
            ElevationName = elevationName;
            Comments.Load(elevationComments);
        }
    }
}
