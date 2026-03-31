using Ofcas.Lk.Api.Shared;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class ElevationEditModeModel
    {
        public IElevationEditMode EditMode { get; }

        public string Name => $"[{EditMode.Id}] {EditMode.Name} [{EditMode.Category.Name}]";

        public ElevationEditModeModel(IElevationEditMode editMode)
        {
            EditMode = editMode;
        }
    }
}
