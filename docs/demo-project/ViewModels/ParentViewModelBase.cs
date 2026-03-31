using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Core.Exceptions;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public abstract class ParentViewModelBase : ViewModelBase
    {
        protected IViewProvider ViewProvider { get; }

        protected ParentViewModelBase(IViewProvider viewProvider)
        {
            Throw.IfNull(viewProvider, nameof(viewProvider));
            ViewProvider = viewProvider;
        }

        protected bool CanDispose(IResult coreObject, out string falseReason)
        {
            falseReason = string.Empty;
            if (coreObject.CanDispose().Value) return true;

            falseReason = "Object or children are still in use.";
            return false;
        }

        protected Exception CatchException(Action action, [CallerMemberName] string method = null, bool uiDispatch = false)
        {
            try
            {
                action.Invoke();
                return null;
            }
            catch (Exception exception)
            {
                if (uiDispatch)
                    Application.Current.Dispatcher.Invoke(() => ShowException(exception, method));
                else
                    ShowException(exception, method);
                return exception;
            }
        }

        protected async Task<Exception> CatchException(Func<Task> func, [CallerMemberName] string method = null)
        {
            try
            {
                await func.Invoke().ConfigureAwait(false);
                return null;
            }
            catch (Exception exception)
            {
                await Application.Current.Dispatcher.InvokeAsync(() => ShowException(exception, method));
                return exception;
            }
        }

        protected void ShowException(Exception exception, [CallerMemberName] string method = null)
        {
            void DetailsAction(MessageViewModel x)
            {
                var exceptionDetailsViewModel = ViewProvider.ViewModelFactory.GetExceptionDetailsViewModel(exception, method);
                ViewProvider.Show(exceptionDetailsViewModel);
            }

            var messageViewModel = ViewProvider.ViewModelFactory.GetMessageViewModel(exception.Message.Trim(), "Exception Occured",
                DetailsAction);
            ViewProvider.ShowDialog(messageViewModel, this);

            if (exception is ServiceNotFoundException)
                Environment.Exit(-1);
        }

        protected void Show<TViewModel>(TViewModel viewModel, Action<TViewModel> onClosed = null)
            where TViewModel : ViewModelBase
        {
            ViewProvider.Show(viewModel, onClosed);
        }

        protected MessageBoxResult ShowMessage(string message, string title = "",
            MessageBoxButton messageBoxButton = default(MessageBoxButton))
        {
            return ViewProvider.Show(this, message, title, messageBoxButton);
        }

        protected bool ShowDialog(ViewModelBase viewModel)
        {
            return ViewProvider.ShowDialog(viewModel, this).GetValueOrDefault(false);
        }
    }
}
