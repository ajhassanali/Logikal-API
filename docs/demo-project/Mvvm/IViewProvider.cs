using System;
using System.Windows;

namespace Ofcas.Lk.Api.Client.Demo.Mvvm
{
    public interface IViewProvider
    {
        /// <summary>
        /// Provides access to a view model factory.
        /// </summary>
        ViewModelFactory ViewModelFactory { get; }

        /// <summary>
        ///     Creates and opens the window registered for the provided viewmodel
        ///     and invokes the provided action after the window closed.
        /// </summary>
        /// <typeparam name="TViewModel">The type of the provided viewmodel.</typeparam>
        /// <param name="viewModel">The provided viewmodel.</param>
        /// <param name="onClosed">The action that should be invoked after the window closed.</param>
        void Show<TViewModel>(TViewModel viewModel, Action<TViewModel> onClosed = null)
            where TViewModel : ViewModelBase;

        MessageBoxResult Show(ViewModelBase parentViewModel, string message, string title = "",
            MessageBoxButton messageBoxButton = default(MessageBoxButton));

        /// <summary>
        ///     Creates and opens a dialog for the provided dialog
        ///     with the provided parent as owner of the dialog.
        /// </summary>
        /// <param name="viewModel">The provided viewmodel.</param>
        /// <param name="parentViewModel">The parent viewmodel.</param>
        /// <returns>Returns true if the dialog was confirmed.</returns>
        bool? ShowDialog(ViewModelBase viewModel, ViewModelBase parentViewModel);

        /// <summary>
        ///     Registers the provided types on this view provider.
        /// </summary>
        /// <typeparam name="TViewModel">The viewmode to register.</typeparam>
        /// <typeparam name="TView">The view linked to the viewmodel.</typeparam>
        void Register<TViewModel, TView>() where TViewModel : ViewModelBase where TView : Window;

        /// <summary>
        ///     Unregisters the provided viewmodel from this view provider.
        /// </summary>
        /// <typeparam name="TViewModel">The viewmodel to unregister.</typeparam>
        void Unregister<TViewModel>() where TViewModel : ViewModelBase;

        /// <summary>
        ///     Gets the handle for the window associated with the provided viewmodel.
        /// </summary>
        /// <param name="viewModel">The provided viewmodel.</param>
        /// <returns>Returns the handle of the associated window.</returns>
        IntPtr GetHandle(ViewModelBase viewModel);

        void ForceCloseAll();
    }
}
