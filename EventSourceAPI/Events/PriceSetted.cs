using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourceAPI.Events
{
    public class PriceSetted : IEvent
    {
        public string location { get; set; }
        public string sku { get; set; }
        public double cp { get; set; }
        public double sp { get; set; }

        public override void execute(ApplicationState state)
        {
            state.setPrice(location, sku, new Decimal(cp), new Decimal(sp));
        }
    }
}
