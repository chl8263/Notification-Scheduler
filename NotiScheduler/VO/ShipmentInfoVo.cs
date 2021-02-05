using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotiScheduler.VO {
    class ShipmentInfoVO {
        public string DocEntry { get; set; }
        public string OrderNumber { get; set; }
        public string DeliveryNumber { get; set; }
        public string CardCode { get; set; }
        public string Language { get; set; }
        public string ShipToName { get; set; }
        public string ShipToAddress1 { get; set; }
        public string ShipToAddress2 { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToState { get; set; }
        public string ShipToZip { get; set; }
        public string ShipToCountry { get; set; }
        public string ShippingMethod { get; set; }
        public string ShipToEmail { get; set; }
        public string TrackingNumber { get; set; }
    }
}
