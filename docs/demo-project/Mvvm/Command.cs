using System;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.Mvvm
{
    public class Command : ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        public Command(Action execute)
            : this(execute, null)
        {
        }

        public Command(Action execute, Func<bool> canExecute)
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

        public void Execute(object parameter = null)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class Command<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public Command(Action<T> execute)
            : this(execute, null)
        {
        }

        public Command(Action<T> execute, Predicate<T> canExecute)
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

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter is T ? (T)parameter : default(T));
        }

        public void Execute(object parameter)
        {
            if (parameter is T)
                _execute((T)parameter);
            else
                _execute(default(T));
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
