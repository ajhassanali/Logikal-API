using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using System;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class EstimationDataSetViewModel : CoreObjectViewModel, IExtendedDisposable
    {
        public IEstimationDataSetInfo EstimationDataSet { get; }

        public EstimationDataSetViewModel(IViewProvider viewProvider, IEstimationDataSetInfo estimationDataSet)
            : base(viewProvider)
        {
            Throw.IfNull(estimationDataSet, nameof(estimationDataSet));

            EstimationDataSet = estimationDataSet;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public bool CanDispose(out string falseReason)
        {
            falseReason = "";
            return true;
        }
    }
}
