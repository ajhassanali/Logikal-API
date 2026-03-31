using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using System.Collections.Generic;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class CoreObjectDetailsViewModel : ViewModelBase
    {
        public IList<CoreObjectDetailsModel> Nodes { get; }

        public CoreObjectDetailsViewModel(ICoreObject coreObject)
        {
            Nodes = new List<CoreObjectDetailsModel>();
            Nodes.Add(GetDetailsFromCoreObject(coreObject));
        }

        public CoreObjectDetailsModel GetDetailsFromCoreObject(ICoreObject coreObject)
        {
            var coreObjectDetails = new CoreObjectDetailsModel();
            switch (coreObject)
            {
                case IElevation elevation:
                    coreObjectDetails.Properties = CoreInfoTODetails(elevation.Info).Properties;
                    foreach (var childInfo in elevation.GetLivingChildrenInfos().CoreInfos)
                    {
                        coreObjectDetails.ChildrenDetails.Add(CoreInfoTODetails(childInfo));
                    }
                    break;

                case IElevationInstance elevationInstance:
                    coreObjectDetails.Properties = CoreInfoTODetails(elevationInstance.Info).Properties;
                    break;

                case IPhase phase:
                    coreObjectDetails.Properties = CoreInfoTODetails(phase.Info).Properties;
                    foreach (var childInfo in phase.GetLivingChildrenInfos().CoreInfos)
                    {
                        coreObjectDetails.ChildrenDetails.Add(CoreInfoTODetails(childInfo));
                    }
                    break;

                case IProject project:
                    coreObjectDetails.Properties = CoreInfoTODetails(project.Info).Properties;
                    foreach (var childInfo in project.GetLivingChildrenInfos().CoreInfos)
                    {
                        coreObjectDetails.ChildrenDetails.Add(CoreInfoTODetails(childInfo));
                    }
                    break;

                case IProjectCenter projectCenter:
                    coreObjectDetails.Properties = CoreInfoTODetails(projectCenter.Info).Properties;
                    foreach (var childInfo in projectCenter.GetLivingChildrenInfos().CoreInfos)
                    {
                        coreObjectDetails.ChildrenDetails.Add(CoreInfoTODetails(childInfo));
                    }
                    break;

                default:
                    coreObjectDetails.Properties.Add("Type", coreObject.GetType().Name);
                    break;
            }
            return coreObjectDetails;
        }

        private CoreObjectDetailsModel CoreInfoTODetails(ICoreInfo info)
        {
            var coreObjectDetails = new CoreObjectDetailsModel();
            switch (info)
            {
                case IElevationInfo elevationInfo:
                    coreObjectDetails.Properties.Add("Guid", elevationInfo.Guid.ToString());
                    coreObjectDetails.Properties.Add("Type", "Elevation");
                    coreObjectDetails.Properties.Add("Name", elevationInfo.Name);
                    break;

                case IElevationInstanceInfo elevationInstanceInfo:
                    coreObjectDetails.Properties.Add("Guid", elevationInstanceInfo.Guid.ToString());
                    coreObjectDetails.Properties.Add("Type", "ElevationInstance");
                    break;

                case IPhaseInfo phaseInfo:
                    coreObjectDetails.Properties.Add("Guid", phaseInfo.Guid.ToString());
                    coreObjectDetails.Properties.Add("Type", "Phase");
                    coreObjectDetails.Properties.Add("Name", phaseInfo.Name);
                    break;

                case IBaseProjectInfo projectInfo:
                    coreObjectDetails.Properties.Add("Guid", projectInfo.Guid.ToString());
                    coreObjectDetails.Properties.Add("Type", "Project");
                    coreObjectDetails.Properties.Add("Name", projectInfo.Name);
                    break;

                case IProjectCenterInfo projectCenterInfo:
                    coreObjectDetails.Properties.Add("ProjectType", projectCenterInfo.Type.ToString());
                    coreObjectDetails.Properties.Add("Type", "ProjectCenter");
                    break;

                default:
                    coreObjectDetails.Properties.Add("Type", info.GetType().Name);
                    break;
            }
            return coreObjectDetails;
        }
    }
}
