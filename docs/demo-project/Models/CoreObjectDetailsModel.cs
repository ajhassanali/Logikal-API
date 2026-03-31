using System;
using System.Collections.Generic;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class CoreObjectDetailsModel
    {
        public IList<CoreObjectDetailsModel> ChildrenDetails { get; set; }

        public IDictionary<String, String> Properties { get; set; }

        public CoreObjectDetailsModel()
        {
            ChildrenDetails = new List<CoreObjectDetailsModel>();
            Properties = new Dictionary<String, String>();
        }
    }
}
