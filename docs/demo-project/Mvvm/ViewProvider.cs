using Microsoft.Win32;
using Ofcas.Lk.Api.Client.Demo.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Ofcas.Lk.Api.Client.Demo.Mvvm
{
    public class ViewProvider : IViewProvider
    {
        private readonly Dictionary<Type, Type> _mappings = new Dictionary<Type, Type>();
        private readonly Dictionary<ViewModelBase, Window> _registeredWindows = new Dictionary<ViewModelBase, Window>();

        public ViewModelFactory ViewModelFactory { get; protected set; }

        public ViewProvider()
        {
            ViewModelFactory = new ViewModelFactory(this);
        }

        public void Show<TViewModel>(TViewModel viewModel, Action<TViewModel> onClosed = null)
            where TViewModel : ViewModelBase
        {
            Throw.IfNull(viewModel, nameof(viewModel));

            var window = CreateWindow(viewModel, null);

            window.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Escape)
                    window.Close();
            };
            window.Closing += OnWindowClosing;

            if (onClosed != null)
                window.Closed += (sender, e) => onClosed.Invoke((TViewModel)((Window)sender).DataContext);

            BeforeShow(window, viewModel);
            window.Show();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            var senderWindow = (Window)sender;
            var extendedDisposable = senderWindow.DataContext as IExtendedDisposable;
            if (extendedDisposable == null) return;

            string falseReason;
            if (extendedDisposable.CanDispose(out falseReason))
            {
                extendedDisposable.Dispose();
                return;
            }

            if (!string.IsNullOrWhiteSpace(falseReason))
            {
                var message = $"{senderWindow.GetType().Name} could not be closed. Reason: {Environment.NewLine}{falseReason}";
                var coreObjectViewModel = senderWindow.DataContext as CoreObjectViewModel;
                Action<MessageViewModel> detailsAction = null;
                if (coreObjectViewModel != null)
                {
                    detailsAction = (x) =>
                    {
                        var coreObject = coreObjectViewModel;
                        var coreObjectDetailsViewModel = ViewModelFactory.GetCoreObjectDetailsViewModel(coreObject.CoreObject);
                        Show(coreObjectDetailsViewModel);
                    };
                }
                var messageViewModel = ViewModelFactory.GetMessageViewModel(message, "Dispose Failure Occured", detailsAction);
                var parent = senderWindow.DataContext as ViewModelBase;
                ShowDialog(messageViewModel, parent);
            }

            e.Cancel = true;
        }

        public MessageBoxResult Show(ViewModelBase parentViewModel, string message, string title = "",
            MessageBoxButton messageBoxButton = default(MessageBoxButton))
        {
            Throw.IfNull(parentViewModel, nameof(parentViewModel));

            var window = GetWindow(parentViewModel);
            return MessageBox.Show(window, message, title, messageBoxButton);
        }

        public bool? ShowDialog(ViewModelBase viewModel, ViewModelBase parentViewModel)
        {
            Throw.IfNull(viewModel, nameof(viewModel));
            Throw.IfNull(parentViewModel, nameof(parentViewModel));

            if (viewModel is SaveFileDialogViewModel)
                return ShowSaveFileDialog(viewModel as SaveFileDialogViewModel, parentViewModel);

            if (viewModel is OpenFileDialogViewModel)
                return ShowOpenFileDialog(viewModel as OpenFileDialogViewModel, parentViewModel);

            var window = CreateWindow(viewModel, parentViewModel);
            var parentWindow = GetWindow(parentViewModel);
            SetOwner(window, parentWindow);

            BeforeShow(window, viewModel);
            return window.ShowDialog();
        }

        public void Register<TViewModel, TView>() where TViewModel : ViewModelBase where TView : Window
        {
            var viewModelType = typeof(TViewModel);
            Type viewType;
            if (!_mappings.TryGetValue(viewModelType, out viewType))
            {
                viewType = typeof(TView);
                _mappings.Add(viewModelType, viewType);
            }
            else if (viewType != typeof(TView))
            {
                throw new InvalidOperationException(
                    $"There's already another view registered for type '{viewModelType}'.");
            }
        }

        public void Unregister<TViewModel>() where TViewModel : ViewModelBase
        {
            var viewModelType = typeof(TViewModel);
            if (_mappings.ContainsKey(viewModelType))
                _mappings.Remove(viewModelType);
        }

        public IntPtr GetHandle(ViewModelBase viewModel)
        {
            Throw.IfNull(viewModel, nameof(viewModel));
            var window = GetWindow(viewModel);
            return new WindowInteropHelper(window).Handle;
        }

        protected virtual void BeforeShow(Window window, ViewModelBase viewModelBase)
        {
            window.Loaded += (sender, e) =>
            {
                var senderWindow = (Window)sender;
                var applicationView = senderWindow.DataContext as IApplicationView;
                if (applicationView == null) return;
                applicationView.OnLoaded();
            };
        }

        private Window CreateWindow(ViewModelBase viewModel, ViewModelBase parentViewModel)
        {
            Type windowType;
            if (!_mappings.TryGetValue(viewModel.GetType(), out windowType))
                throw new InvalidOperationException($"No view registered for type '{viewModel.GetType()}'.");

            var window = (Window)Activator.CreateInstance(windowType);
            window.DataContext = viewModel;
            if (parentViewModel != null)
                window.Owner = GetWindow(parentViewModel);
            window.Closed += WindowOnClosed;
            _registeredWindows.Add(viewModel, window);
            return window;
        }

        private bool? ShowSaveFileDialog(SaveFileDialogViewModel saveFileDialogViewModel, ViewModelBase parentViewModel)
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = saveFileDialogViewModel.FileName,
                Filter = saveFileDialogViewModel.Filter,
                DefaultExt = saveFileDialogViewModel.DefaultExt
            };

            var parentWindow = GetWindow(parentViewModel);
            var result = saveFileDialog.ShowDialog(parentWindow);
            saveFileDialogViewModel.FileName = saveFileDialog.FileName;
            return result;
        }

        private bool? ShowOpenFileDialog(OpenFileDialogViewModel openFileDialogViewModel, ViewModelBase parentViewModel)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = openFileDialogViewModel.Filter
            };

            var parentWindow = GetWindow(parentViewModel);
            var result = openFileDialog.ShowDialog(parentWindow);
            openFileDialogViewModel.FileName = openFileDialog.FileName;
            return result;
        }

        private Window GetWindow(ViewModelBase viewModel)
        {
            Window window;
            if (!_registeredWindows.TryGetValue(viewModel, out window))
                throw new InvalidOperationException($"No window registered for viewModel '{viewModel.GetType()}'.");
            return window;
        }

        private void WindowOnClosed(object sender, EventArgs e)
        {
            var window = (Window)sender;
            var viewModel = (ViewModelBase)window.DataContext;
            _registeredWindows.Remove(viewModel);
        }

        private void SetOwner(Window window, Window ownerWindow)
        {
            var windowInteropHelper = new WindowInteropHelper(window);
            var ownerWindowInteropHelper = new WindowInteropHelper(ownerWindow);

            windowInteropHelper.Owner = ownerWindowInteropHelper.Handle;
        }

        public void ForceCloseAll()
        {
            var registeredWindows = new Dictionary<ViewModelBase, Window>(_registeredWindows);
            foreach(var registeredWindow in registeredWindows)
            {
                var viewModel = registeredWindow.Key;
                if (viewModel is LoginViewModel)
                    continue;

                var window = registeredWindow.Value;
                window.Closing -= OnWindowClosing;

                if (viewModel is IDisposable disposable)
                    disposable.Dispose();

                window.Close();
            }
        }
    }
}
