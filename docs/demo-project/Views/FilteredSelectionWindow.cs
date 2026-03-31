using Ofcas.Lk.Api.Client.Demo.Utils;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ofcas.Lk.Api.Client.Demo.Views
{
    public abstract class FilteredSelectionWindow : BaseWindow
    {
        private ICollectionView _collectionView;
        private string _filterText;
        private SimpleCollectionViewSorter _sorter;

        protected abstract ListView TargetListView { get; }

        protected FilteredSelectionWindow()
        {
            Loaded += OnLoaded;
        }

        protected void OnGridViewColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader gridViewColumnHeader)
                _sorter.Sort(gridViewColumnHeader);
        }

        protected void OnFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            _filterText = textBox.Text;
            _collectionView.Refresh();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _collectionView = CollectionViewSource.GetDefaultView(TargetListView.ItemsSource);
            _collectionView.Filter = Filter;
            _sorter = new SimpleCollectionViewSorter(_collectionView);
        }

        private bool Filter(object obj)
        {
            return Filter(obj, _filterText);
        }

        protected abstract bool Filter(object obj, string filterText);

        protected bool DoesContainFilterText(string filterText, params string[] values)
        {
            return values.Any(value => value.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}