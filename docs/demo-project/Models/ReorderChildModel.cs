using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Shared;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class ReorderChildModel
    {
        public ICoreInfo TargetCoreInfo { get; set; }
        public ChildReorderBehavior ChildReorderBehavior { get; set; }
    }
}
