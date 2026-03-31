using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    /// <summary>
    /// A sync behaviour for a multiselector.
    /// </summary>
    public static class MultiSelectorBehaviours
    {
        public static readonly DependencyProperty SynchronizedSelectedItems = DependencyProperty.RegisterAttached(
            "SynchronizedSelectedItems", typeof(IList), typeof(MultiSelectorBehaviours),
            new PropertyMetadata(null, OnSynchronizedSelectedItemsChanged));

        public static readonly DependencyProperty OrderSelectedItemsProperty = DependencyProperty.RegisterAttached(
            "OrderSelectedItems", typeof(bool), typeof(MultiSelectorBehaviours),
            new PropertyMetadata(false));

        private static readonly DependencyProperty SynchronizationManagerProperty = DependencyProperty.RegisterAttached(
            "SynchronizationManager", typeof(SynchronizationManager), typeof(MultiSelectorBehaviours),
            new PropertyMetadata(null));

        public static IList GetSynchronizedSelectedItems(DependencyObject dependencyObject)
            => (IList)dependencyObject.GetValue(SynchronizedSelectedItems);

        public static void SetSynchronizedSelectedItems(DependencyObject dependencyObject, IList value)
            => dependencyObject.SetValue(SynchronizedSelectedItems, value);

        public static bool GetOrderSelectedItems(DependencyObject dependencyObject)
            => (bool)dependencyObject.GetValue(OrderSelectedItemsProperty);

        public static void SetOrderSelectedItems(DependencyObject dependencyObject, bool value)
            => dependencyObject.SetValue(OrderSelectedItemsProperty, value);

        private static SynchronizationManager GetSynchronizationManager(DependencyObject dependencyObject)
            => (SynchronizationManager)dependencyObject.GetValue(SynchronizationManagerProperty);

        private static void OnSynchronizedSelectedItemsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                var synchronizer = GetSynchronizationManager(dependencyObject);
                synchronizer.StopSynchronizing();

                SetSynchronizationManager(dependencyObject, null);
            }

            var list = e.NewValue as IList;
            var selector = dependencyObject as Selector;

            // ReSharper disable once InvertIf
            if (list != null && selector != null)
            {
                var synchronizer = GetSynchronizationManager(dependencyObject);
                if (synchronizer == null)
                {
                    synchronizer = new SynchronizationManager(selector);
                    SetSynchronizationManager(dependencyObject, synchronizer);
                }

                synchronizer.StartSynchronizingList();
            }
        }

        private static void SetSynchronizationManager(DependencyObject dependencyObject, SynchronizationManager value) =>
            dependencyObject.SetValue(SynchronizationManagerProperty, value);

        /// <summary>
        /// A synchronization manager.
        /// </summary>
        private class SynchronizationManager
        {
            private readonly Selector _multiSelector;
            private TwoListSynchronizer _synchronizer;

            /// <summary>
            /// Initializes a new instance of the <see cref="SynchronizationManager"/> class.
            /// </summary>
            /// <param name="selector">The selector.</param>
            internal SynchronizationManager(Selector selector)
            {
                _multiSelector = selector;
            }

            /// <summary>
            /// Starts synchronizing the list.
            /// </summary>
            public void StartSynchronizingList()
            {
                var list = GetSynchronizedSelectedItems(_multiSelector);
                if (list == null) return;

                if (GetOrderSelectedItems(_multiSelector))
                    _synchronizer = new TwoListSynchronizer(GetSelectedItemsCollection(_multiSelector), list, (IList)_multiSelector.ItemsSource);
                else
                    _synchronizer = new TwoListSynchronizer(GetSelectedItemsCollection(_multiSelector), list);

                _synchronizer.StartSynchronizing();
            }

            /// <summary>
            /// Stops synchronizing the list.
            /// </summary>
            public void StopSynchronizing()
            {
                _synchronizer.StopSynchronizing();
            }

            private static IList GetSelectedItemsCollection(Selector selector)
            {
                var multiSelector = selector as MultiSelector;
                if (multiSelector != null)
                    return multiSelector.SelectedItems;

                var listBox = selector as ListBox;
                if (listBox != null)
                    return listBox.SelectedItems;

                throw new InvalidOperationException("Target object has no SelectedItems property to bind.");
            }
        }
    }
}
