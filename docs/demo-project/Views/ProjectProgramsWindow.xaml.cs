using Ofcas.Lk.Api.Shared;
using System.Windows.Controls;

namespace Ofcas.Lk.Api.Client.Demo.Views
{
    public partial class ProjectProgramsWindow
    {
        protected override ListView TargetListView => ProgramsList;

        public ProjectProgramsWindow()
        {
            InitializeComponent();
        }

        protected override bool Filter(object obj, string filterText)
        {
            var runnableProgram = obj as IProjectRunnableProgram;
            if (runnableProgram == null)
                return false;

            if (string.IsNullOrEmpty(filterText))
                return true;

            return DoesContainFilterText(filterText, runnableProgram.Name, runnableProgram.Id.ToString(),
                runnableProgram.Category.ToString());
        }
    }
}
