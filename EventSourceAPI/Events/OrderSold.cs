using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourceAPI.Events
{
    public class OrderSold : IEvent
    {
        public string location { get; set; }
        public Dictionary<string, int> items {get; set;}

        public OrderSold()
        {
            items = new Dictionary<string, int>();
        }

        public override void execute(ApplicationState state)
        {
            //Cans update other views of stats
            //E.g. What should buy next?

            foreach(var i in items)
            {
                state.itemSold(location, i.Key, i.Value);
            }
        }
    }
}
