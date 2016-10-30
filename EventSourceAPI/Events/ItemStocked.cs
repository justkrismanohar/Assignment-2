using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourceAPI.Events
{
    public class ItemStocked : IEvent
    {
        public string sku { get; set; }
        public string location { get; set; }
        public int n { get; set; }

        public override void execute(ApplicationState state)
        {
            try
            {
                int newStock = state.stockItem(location, sku, n);
            }
            catch (Exception e)
            {

            }
        }
    }
}
