using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Shared;
using System.Collections.Generic;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ElevationQuotationTextsViewModel : ViewModelBase
    {
        public string ElevationName { get; }

        public SelectionAdapter<IQuotationText> QuotationTexts { get; } = new SelectionAdapter<IQuotationText>();

        public ICommand CopyQuotationTextCommand { get; }

        public ElevationQuotationTextsViewModel(string elevationName, IEnumerable<IQuotationText> quotationTexts)
        {
            ElevationName = elevationName;
            QuotationTexts.Load(quotationTexts);

            CopyQuotationTextCommand = new Command(CopyQuotationText, CanCopyQuotationText);
        }

        private bool CanCopyQuotationText()
        {
            return QuotationTexts.SelectedItem != null;
        }

        private void CopyQuotationText()
        {
            ClipboardHelper.CopyObjectToWindowsClipboard(QuotationTexts.SelectedItem.Text);
        }
    }
}
