using Ofcas.Lk.Api.Client.Demo.Mvvm;
using System;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class MessageViewModel : ParentViewModelBase
    {
        private readonly Action<MessageViewModel> _detailsAction;
        private readonly Action _sendStatusReportAction;

        public string Message { get; }

        public ICommand ShowDetailsCommand { get; }
        public ICommand SendStatusReportCommand { get; }

        public string MessageDialogTitle { get; }

        public MessageViewModel(IViewProvider viewProvider, string message, string dialogTitle, Action<MessageViewModel> detailsAction)
            : base(viewProvider)
        {
            _detailsAction = detailsAction;
            MessageDialogTitle = dialogTitle;
            Message = message;
            if (detailsAction != null)
                ShowDetailsCommand = new Command(ShowDetails, CanShowDetails);
        }

        public MessageViewModel(IViewProvider viewProvider, string message, string dialogTitle,
            Action<MessageViewModel> detailsAction, Action sendStatusReportAction)
            : this(viewProvider, message, dialogTitle, detailsAction)
        {
            _sendStatusReportAction = sendStatusReportAction;
            if (_sendStatusReportAction != null)
                SendStatusReportCommand = new Command(SendStatusReport, CanSendStatusReport);
        }

        private bool CanSendStatusReport()
        {
            return _sendStatusReportAction != null;
        }

        private void SendStatusReport()
        {
            _sendStatusReportAction.Invoke();
            Accept();
        }

        public bool CanShowDetails()
        {
            return _detailsAction != null;
        }

        public void ShowDetails()
        {
            _detailsAction.Invoke(this);
        }
    }
}
