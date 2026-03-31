using Ofcas.Lk.Api.Shared;
using System;
using System.Collections.Generic;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class ProjectInformationContainerModel
    {
        public Guid CoreObjectId { get; set; }
        public Guid Guid { get; set; }

        public string Name { get; set; }

        public IProjectInformationType Type { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public bool CalculationRequired { get; set; }
    }
}
