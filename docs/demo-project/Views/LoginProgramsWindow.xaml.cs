using Ofcas.Lk.Api.Shared;
using System.Windows.Controls;

namespace Ofcas.Lk.Api.Client.Demo.Views
{
    public partial class LoginProgramsWindow
    {
        protected override ListView TargetListView => ProgramsList;

        public LoginProgramsWindow()
        {
            InitializeComponent();
        }

        protected override bool Filter(object obj, string filterText)
        {
            var reportItem = obj as ILoginRunnableProgram;
            if (reportItem == null)
                return false;

            if (string.IsNullOrEmpty(filterText))
                return true;

            return DoesContainFilterText(filterText, reportItem.Name, reportItem.Id.ToString(),
                reportItem.Category.ToString());
        }
    }
}
