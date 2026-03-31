using System;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class ProjectCenterModel
    {
        public Guid CoreObjectId { get; set; }
        public string DirectoryName { get; set; }
        public string TypeAsName { get; set; }
        public int TypeAsId { get; set; }
        public bool IsRecycleBin { get; set; }

        public string Title => string.IsNullOrWhiteSpace(DirectoryName) ? TypeAsName : $"{TypeAsName} - {DirectoryName}";
    }
}
