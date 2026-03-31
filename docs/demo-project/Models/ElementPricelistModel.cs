using System;
using System.Windows.Media;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class ElementPricelistModel
    {
        public Guid CoreObjectId { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string System { get; set; }
        public string Path { get; set; }
        public ImageSource Thumbnail { get; set; }
    }
}