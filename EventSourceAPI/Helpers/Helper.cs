using EventSourceAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourceAPI.Helpers
{
    public class Helper
    {

        public static ItemCreated createItem(ApplicationState state, string system, string location, string name)
        {

            ItemCreated item = new ItemCreated()
            {
                location = location,
                name = name
            };
            item.execute(state);
            EventLog.log(system, item);
            Console.WriteLine("Created {0} {1} sku {2} ", item.location, item.name, item.sku);

            return item;
        }

        public static ItemStocked stockItem(ApplicationState state, string system, string location, string sku, int n)
        {
            ItemStocked toStock = new ItemStocked()
            {
                location = location,
                sku = sku,
                n = n
            };
            toStock.execute(state);
            EventLog.log(system, toStock);
            Console.WriteLine("Stocked {0} t {1} sku {2} ", toStock.location, toStock.n, toStock.sku);

            return toStock;
        }

        public static PriceSetted setPrice(ApplicationState state, string system, string location, string sku, double cp, double sp)
        {
            PriceSetted ps = new PriceSetted()
            {
                location = location,
                sku = sku,
                cp = cp,
                sp = sp
            };
            ps.execute(state);
            EventLog.log(system, ps);
            Console.WriteLine("{0} {1} cp {2} sp {3}", ps.location, ps.sku, ps.cp, ps.sp);
            return ps;
        }

    }
}
