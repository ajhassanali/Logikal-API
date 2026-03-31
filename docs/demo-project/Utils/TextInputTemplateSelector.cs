using System.Windows;
using System.Windows.Controls;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class TextInputTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Default { get; set; }
        public DataTemplate WithEntries { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item == null ? Default : WithEntries;
        }
    }
}