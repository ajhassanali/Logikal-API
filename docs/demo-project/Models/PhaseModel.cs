using Ofcas.Lk.Api.Shared;
using System;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class PhaseModel
    {
        public Guid CoreObjectId { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
    }
}
