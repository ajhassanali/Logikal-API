using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class ParameterTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Default { get; set; }
        public DataTemplate WithRestrictedValues { get; set; }
        public DataTemplate AsBoolean { get; set; }
        public DataTemplate AsWithHelper { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var parameter = item as IParameter;
            if (parameter != null)
            {
                if (parameter.RestrictedValues != null)
                    return WithRestrictedValues;

                if (parameter.Value is bool)
                    return AsBoolean;
                if (parameter.InputHelper != null)
                    return AsWithHelper;
            }

            return Default;
        }
    }
}
