using System;
using System.Collections.Generic;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public static class DictionaryExtensions
    {
        public static bool HasParameterValue(this IDictionary<string, object> parameters, string key)
        {
            if (parameters.TryGetValue(key, out var value) && value is string valueAsString)
                return !string.IsNullOrWhiteSpace(valueAsString);
            return false;
        }

        public static bool TryGetValue<T>(this IDictionary<string, object> parameters, string key, out T value)
        {
            value = default(T);

            if (!parameters.TryGetValue(key, out var objectValue))
                return false;

            if (objectValue is T castValue)
            {
                value = castValue;
                return true;
            }

            if (typeof(T) == typeof(bool))
            {
                if (objectValue is string stringValue && bool.TryParse(stringValue, out var booleanValue))
                {
                    value = (T)(object)booleanValue;
                    return true;
                }

                return false;
            }

            throw new NotSupportedException("This type is not yet support, please add a case for this type.");
        }
    }
}
