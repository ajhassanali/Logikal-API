using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class Parameter<T> : IParameter, INotifyPropertyChanged
    {
        private string _key;
        private T _value;
        private List<T> _restrictedValues;
        public Func<object> InputHelper { get; set; }

        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public List<T> RestrictedValues
        {
            get => _restrictedValues;
            set
            {
                _restrictedValues = value;
                if (_value == null || _value.Equals(default(T)))
                    _value = _restrictedValues.FirstOrDefault();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsRequired { get; set; }

        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        object IParameter.Value
        {
            get { return Value; }
            set
            {
                if (value is T castValue)
                    Value = castValue;
                else
                    throw new InvalidCastException($"Cannot cast '{value}' of type '{value?.GetType().Name}' to type '{typeof(T).Name}'.");
            }
        }

        IList IParameter.RestrictedValues => RestrictedValues;

        public Parameter()
        {
            if (typeof(T).IsEnum)
                RestrictedValues = Utils.Enum.GetValues<T>().ToList();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
