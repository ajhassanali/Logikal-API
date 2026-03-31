using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.Mvvm
{
    public class AsyncCommand : ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Func<Task> _execute;

        public AsyncCommand(Func<Task> execute)
            : this(execute, null)
        {
        }

        public AsyncCommand(Func<Task> execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter = null)
        {
            return _canExecute == null || _canExecute();
        }

        public async void Execute(object parameter = null)
        {
            await _execute().ConfigureAwait(false);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class AsyncCommand<T> : ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Func<T, Task> _execute;

        public AsyncCommand(Func<T, Task> execute)
            : this(execute, null)
        {
        }

        public AsyncCommand(Func<T, Task> execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter = null)
        {
            return _canExecute == null || _canExecute();
        }

        public async void Execute(object parameter = null)
        {
            if (parameter is T)
                await _execute((T)parameter).ConfigureAwait(false);
            else
                await _execute(default(T)).ConfigureAwait(false);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
