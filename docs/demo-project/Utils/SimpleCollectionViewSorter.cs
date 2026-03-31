using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class SimpleCollectionViewSorter
    {
        private readonly ICollectionView _collectionView;
        private ListSortDirection _lastDirection;
        private GridViewColumnHeader _lastHeaderClicked;

        public SimpleCollectionViewSorter(ICollectionView collectionView)
        {
            _collectionView = collectionView;
        }

        public void Sort(GridViewColumnHeader headerClicked)
        {
            if (headerClicked == null) return;

            if (headerClicked.Role == GridViewColumnHeaderRole.Padding) return;

            ListSortDirection direction;
            if (!Equals(headerClicked, _lastHeaderClicked))
                direction = ListSortDirection.Ascending;
            else
                switch (_lastDirection)
                {
                    case ListSortDirection.Ascending:
                        direction = ListSortDirection.Descending;
                        break;

                    case ListSortDirection.Descending:
                        direction = ListSortDirection.Ascending;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

            var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
            var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

            if (sortBy == null) return;

            _collectionView.SortDescriptions.Clear();
            var sortDescription = new SortDescription(sortBy, direction);
            _collectionView.SortDescriptions.Add(sortDescription);
            _collectionView.Refresh();

            _lastHeaderClicked = headerClicked;
            _lastDirection = direction;
        }
    }
}