using Ofcas.Lk.Api.Client.Core.Exceptions;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Shared;
using Ofcas.Lk.Api.Shared.Contracts.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ExceptionDetailsViewModel : ViewModelBase
    {
        public Exception Exception { get; }
        public Type ExceptionType { get; }
        public ILastErrorInfo LastErrorInfo { get; }
        public List<SelectionAdapter<string>> StackTraces { get; } = new List<SelectionAdapter<string>>();
        public string CalledMethod { get; }

        public ICommand CopyEntriesCommand { get; }

        public ExceptionDetailsViewModel(ISynchronizationException synchronizationException)
        {
            LastErrorInfo = synchronizationException.LastErrorInfo;
            ParseLastErrorStackTraces(LastErrorInfo);
        }

        public ExceptionDetailsViewModel(Exception exception, string calledMethod)
        {
            Exception = exception;
            ExceptionType = Exception.GetType();
            CalledMethod = calledMethod;

            CopyEntriesCommand = new Command<IList<string>>(CopyEntries);

            var clientStackTrace = ParseStackTrace(Exception.StackTrace);
            if (clientStackTrace != null)
            {
                StackTraces.Add(new SelectionAdapter<string>
                {
                    ItemsSource = clientStackTrace,
                    Description = "Client Stack Trace"
                });
            }

            var apiException = exception as ApiException;
            if (apiException != null && apiException.LastErrorInfo != null)
            {
                LastErrorInfo = apiException.LastErrorInfo;
                ParseLastErrorStackTraces(LastErrorInfo);
            }
        }

        private void ParseLastErrorStackTraces(ILastErrorInfo lastErrorInfo)
        {
            var serviceStackTrace = ParseStackTrace(lastErrorInfo.ServiceStackTrace);
            if (serviceStackTrace != null)
            {
                StackTraces.Add(new SelectionAdapter<string>
                {
                    ItemsSource = serviceStackTrace,
                    Description = "Service Stack Trace"
                });
            }

            var internalStackTrace = ParseStackTrace(lastErrorInfo.InternalStackTrace);
            if (internalStackTrace != null)
            {
                StackTraces.Add(new SelectionAdapter<string>
                {
                    ItemsSource = internalStackTrace,
                    Description = "Internal Stack Trace"
                });
            }
        }

        private ObservableCollection<string> ParseStackTrace(string stackTrace)
        {
            if (string.IsNullOrWhiteSpace(stackTrace))
                return null;

            var entries = stackTrace.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return new ObservableCollection<string>(entries);
        }

        private void CopyEntries(IList<string> entries)
        {
            var text = string.Join(Environment.NewLine, entries);
            Clipboard.SetDataObject(text);
        }
    }
}
