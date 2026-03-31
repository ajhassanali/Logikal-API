using System;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class DocumentModel
    {
        public Guid CoreObjectId { get; set; }
        public Guid Guid { get; set; }
        public string DisplayName { get; set; }
        public string Suffix { get; set; }
        public string Uri { get; set; }
    }
}
