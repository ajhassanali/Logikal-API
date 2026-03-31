using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Shared;
using System.Collections.ObjectModel;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class SelectCadDrawingViewModel : ParentViewModelBase
    {
        public SelectionAdapter<IFileData> CadDrawings { get; } = new SelectionAdapter<IFileData>();

        public SelectCadDrawingViewModel(IViewProvider viewProvider, ILoginScope loginScope)
            : base(viewProvider)
        {
            var recentCadDrawings = loginScope.GetRecentCadDrawings().FileDataList;
            CadDrawings.ItemsSource = new ObservableCollection<IFileData>(recentCadDrawings);
        }
    }
}
