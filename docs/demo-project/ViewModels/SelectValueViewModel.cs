using Ofcas.Lk.Api.Client.Demo.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class SelectValueViewModel<T> : ViewModelBase
    {
        public ObservableObject<string> Title { get; }
        public ObservableCollection<T> Values { get; }
        public ObservableObject<T> Selected { get; }

        public SelectValueViewModel(string title, T defaultValue, IEnumerable<T> enumerable)
            : this(defaultValue, enumerable)
        {
            if (title == null)
                title = "Select value";
            Title = new ObservableObject<string>(title);
        }

        public SelectValueViewModel(T defaultValue, IEnumerable<T> enumerable)
        {
            if (defaultValue == null)
                throw new ArgumentNullException(nameof(defaultValue));

            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            Selected = new ObservableObject<T>(defaultValue);
            Values = new ObservableCollection<T>(enumerable);
        }
    }
}
