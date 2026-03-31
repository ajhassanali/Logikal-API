using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Shared;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class SelectReportsViewModel : ViewModelBase
    {
        public SelectionAdapter<IReportItem> Reports { get; } = new SelectionAdapter<IReportItem>();

        public SelectReportsViewModel(IEnumerable<IReportItem> reportItems)
        {
            Reports.ItemsSource = new ObservableCollection<IReportItem>(reportItems);
        }
    }
}
