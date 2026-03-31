using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class SelectionAdapter<T> : INotifyPropertyChanged
    {
        public string Description { get; set; }

        private ObservableCollection<T> _itemsSource;

        public ObservableCollection<T> ItemsSource
        {
            get { return _itemsSource; }
            set { _itemsSource = value; OnPropertyChanged(); }
        }

        private T _selectedItem;

        public T SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        private ObservableCollection<T> _selectedItems = new ObservableCollection<T>();

        public ObservableCollection<T> SelectedItems
        {
            get { return _selectedItems; }
            set { _selectedItems = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Load(IEnumerable<T> enumerable)
        {
            ItemsSource = new ObservableCollection<T>(enumerable);
            SelectedItem = default(T);
            SelectedItems.Clear();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            if (Description == null)
                return base.ToString();

            return Description + $" ({ItemsSource.Count})";
        }
    }
}