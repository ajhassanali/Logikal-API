using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class EstimationDataSetContainerViewModel : CoreObjectViewModel
    {
        private IEstimationDataSetContainer _estimationDataSetContainer;

        public SelectionAdapter<IEstimationDataSetInfo> EstimationDataSets { get; } = new SelectionAdapter<IEstimationDataSetInfo>();

        public EstimationDataSetContainerViewModel(IViewProvider viewProvider, IEstimationDataSetContainer estimationDataSetContainer)
            : base(viewProvider, estimationDataSetContainer)
        {
            Throw.IfNull(estimationDataSetContainer, nameof(estimationDataSetContainer));
            _estimationDataSetContainer = estimationDataSetContainer;

            RefreshChildren();
        }

        private void RefreshChildren(bool hardRefresh = false)
        {
            if (hardRefresh)
                _estimationDataSetContainer.RefreshChildren();

            var childrenInfos = _estimationDataSetContainer.ChildrenInfos;

            EstimationDataSets.ItemsSource = new ObservableCollection<IEstimationDataSetInfo>(childrenInfos);
        }
    }
}
