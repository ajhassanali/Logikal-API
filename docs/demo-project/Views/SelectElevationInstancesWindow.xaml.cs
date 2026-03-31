using Ofcas.Lk.Api.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ofcas.Lk.Api.Client.Demo.Views
{
    /// <summary>
    /// Interaktionslogik für SelectElevationInstancesWindow.xaml
    /// </summary>
    public partial class SelectElevationInstancesWindow
    {
        public SelectElevationInstancesWindow()
        {
            InitializeComponent();
        }

        protected override ListView TargetListView => InstancesList;

        protected override bool Filter(object obj, string filterText)
        {
            var elevationInstanceInfo = obj as IElevationInstanceInfo;
            if (elevationInstanceInfo == null)
                return false;

            if (string.IsNullOrEmpty(filterText))
                return true;

            return DoesContainFilterText(filterText, elevationInstanceInfo.Guid.ToString());
        }
    }
}
