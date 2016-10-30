using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventSourceAPI.Models
{

    public class Item
    {
        public string name { get; set; }
        public string location { get; set; }
        public string sku { get; set; }
        public int quantity { get; set; }
    }
}