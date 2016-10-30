using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventSourceAPI.Events
{
    //Populated from the append only log
    public class ItemCreated : IEvent
    {
        public string name { get; set; }
        public string location { get; set; }
        public string sku { get; set; }

        public override void execute(ApplicationState state)
        {
            //updates local application state based on 
            //event detials
            sku = state.createItem(name, location);
            //pass the message along if neccessary
        }
    }
}