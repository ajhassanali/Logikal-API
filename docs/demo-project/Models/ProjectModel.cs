using Ofcas.Lk.Api.Shared;
using System;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class ProjectModel
    {
        public Guid CoreObjectGuid { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string JobNumber { get; set; }
        public string OfferNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastChangeDate { get; set; }
        public int Type { get; set; }
        public bool IsCalculated { get; set; }
        public IProjectStatus Status { get; set; }
        public string CustomerName { get; set; }
        public DateTime ProvisionDate { get; set; }
        public string FabricationLotNumber { get; set; }
        public bool IsProject { get; set; }
        public bool IsFabricationLot { get; set; }
    }
}
