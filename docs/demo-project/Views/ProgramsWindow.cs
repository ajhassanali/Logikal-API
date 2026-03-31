using Ofcas.Lk.Api.Shared;

namespace Ofcas.Lk.Api.Client.Demo.Views
{
    public abstract class ProgramsWindow : FilteredSelectionWindow
    {
        protected override bool Filter(object obj, string filterText)
        {
            if (obj as ILoginRunnableProgram is null ||
                obj as IProjectRunnableProgram is null)
                return false;

            if (string.IsNullOrEmpty(filterText))
                return true;

            if (obj is ILoginRunnableProgram loginRunnableProgram)
            {
                return DoesContainFilterText(filterText, loginRunnableProgram.Name, loginRunnableProgram.Id.ToString());
            }
            else if(obj is IProjectRunnableProgram projectRunnableProgram)
            {
                return DoesContainFilterText(filterText, projectRunnableProgram.Name, projectRunnableProgram.Id.ToString());
            }

            return false;
        }
    }
}
