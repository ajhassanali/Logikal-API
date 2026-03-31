using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Events;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Shared;
using System;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class SynchronizationContainerViewModel : CoreObjectViewModel, IExtendedDisposable
    {
        private readonly ICoreObjectWithSynchronizations _coreObjectWithSynchronizations;
        private readonly Timer _timer;
        private volatile bool _isDiposed;

        public SelectionAdapter<ISynchronizedEvent> SynchronizedEvents { get; }
        public SelectionAdapter<ISynchronizedMessage> SynchronizedMessages { get; }

        public ObservableObject<string> Title { get; } = new ObservableObject<string>(string.Empty);

        public ICommand ClearSynchronizationEventsCommand { get; }
        public ICommand ClearSynchronizationMessagesCommand { get; }

        public ICommand ShowDetailsCommand { get; }
        public ICommand ToggleSynchronizationCommand { get; }

        public event EventHandler<SynchronizedEventReceivedEventArgs> SynchronizedEventReceived;

        public event EventHandler<SynchronizedMessageReceivedEventArgs> SynchronizedMessageReceived;

        public bool AcceptsSynchronizations => _coreObjectWithSynchronizations.SynchronizationContainer.EventSynchronizationEnabled;

        public SynchronizationContainerViewModel(IViewProvider viewProvider, ICoreObjectWithSynchronizations coreObjectWithSynchronizations)
            : base(viewProvider, coreObjectWithSynchronizations)
        {
            if (coreObjectWithSynchronizations == null)
                throw new ArgumentNullException(nameof(coreObjectWithSynchronizations));

            _coreObjectWithSynchronizations = coreObjectWithSynchronizations;
            Title.Value = $"Synchronization [{_coreObjectWithSynchronizations.Id}]";

            _timer = new Timer(500) { AutoReset = false };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();

            SynchronizedEvents = new SelectionAdapter<ISynchronizedEvent>()
            {
                ItemsSource = new ObservableCollection<ISynchronizedEvent>()
            };

            SynchronizedMessages = new SelectionAdapter<ISynchronizedMessage>()
            {
                ItemsSource = new ObservableCollection<ISynchronizedMessage>()
            };

            ClearSynchronizationEventsCommand = new Command(ClearSynchronizationEvents, CanClearSynchronizationEvents);
            ClearSynchronizationMessagesCommand = new Command(ClearSynchronizationMessages, CanClearSynchronizationMessages);
            ShowDetailsCommand = new Command<ISynchronizedEvent>(ShowDetails);
            ToggleSynchronizationCommand = new Command(ToggleSynchronization);
        }

        private bool CanClearSynchronizationEvents()
        {
            return SynchronizedEvents.ItemsSource.Count > 0;
        }

        private void ClearSynchronizationEvents()
        {
            SynchronizedEvents.ItemsSource.Clear();
        }

        private bool CanClearSynchronizationMessages()
        {
            return SynchronizedMessages.ItemsSource.Count > 0;
        }

        private void ClearSynchronizationMessages()
        {
            SynchronizedMessages.ItemsSource.Clear();
        }

        private void ShowDetails(ISynchronizedEvent synchronizedEvent)
        {
            if (synchronizedEvent.Exception == null)
                return;

            var exceptionsDetailsViewModel = ViewProvider.ViewModelFactory.GetExceptionDetailsViewModel(synchronizedEvent.Exception);
            ViewProvider.Show(exceptionsDetailsViewModel);
        }

        public void Dispose()
        {
            _isDiposed = true;
            _timer.Dispose();
        }

        public bool CanDispose(out string falseReason)
        {
            falseReason = "";
            return true;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                var synchronizedEvents = _coreObjectWithSynchronizations.SynchronizationContainer.GetEvents().SynchronizedEvents;
                var synchronizedMessages = _coreObjectWithSynchronizations.SynchronizationContainer.TakeMessages().SynchronizedMessages;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var synchronizedEvent in synchronizedEvents)
                    {
                        SynchronizedEvents.ItemsSource.Add(synchronizedEvent);
                        SynchronizedEventReceived?.Invoke(this, new SynchronizedEventReceivedEventArgs(synchronizedEvent));
                    }

                    foreach (var synchronizedMessage in synchronizedMessages)
                    {
                        SynchronizedMessages.ItemsSource.Add(synchronizedMessage);
                        SynchronizedMessageReceived?.Invoke(this, new SynchronizedMessageReceivedEventArgs(synchronizedMessage));
                    }
                });

                if (!_isDiposed)
                    _timer.Start();
            }
            catch (Exception exception)
            {
                Application.Current.Dispatcher.Invoke(() => { MessageBox.Show(exception.ToString()); });
            }
        }

        private void ToggleSynchronization()
        {
            _coreObjectWithSynchronizations.SynchronizationContainer.EventSynchronizationEnabled = !AcceptsSynchronizations;
        }
    }
}
