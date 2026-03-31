using System;

namespace Ofcas.Lk.Api.Client.Demo.Models
{
    public class AddressModel
    {
        public Guid CoreObjectId { get; set; }
        public string Type { get; set; }
        public Guid Guid { get; set; }
        public string Condition { get; set; }
        public string Country { get; set; }
        public string CustomerNo { get; set; }
        public string CustomerRef { get; set; }
        public string EMail { get; set; }
        public string FAO { get; set; }
        public string Fax { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Salutation { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string FreeText1 { get; set; }
        public string FreeText2 { get; set; }
        public string FreeText3 { get; set; }
        public bool IsEmpty { get; set; }
    }
}
