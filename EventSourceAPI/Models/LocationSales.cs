using EventSourceAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourceAPI.Models
{
    public class LocationSales
    {
        public Decimal foreverSP { get; set; }
        public Decimal foreverCP { get; set; }
        public long quantitySold { get; set; }

        public Dictionary<string, ItemSales> sales;

        public LocationSales()
        {
            sales = new Dictionary<string, ItemSales>();
        }

        public void itemSold(string sku, Decimal cp, Decimal sp, int n)
        {
            ItemSales item;
            if (!sales.TryGetValue(sku, out item))
            {
                item = new ItemSales();
                sales.Add(sku, item);
            }

            item.foreverCP = Decimal.Add(item.foreverCP, cp * n);
            item.foreverSP = Decimal.Add(item.foreverSP, sp * n);
            foreverCP = Decimal.Add(foreverCP, cp * n);
            foreverSP = Decimal.Add(foreverSP, sp * n);
            item.quantitySold = item.quantitySold + n;
            quantitySold = quantitySold + n;
        }

        public ItemSales getItem(string sku)
        {
            ItemSales i;
            if (!sales.TryGetValue(sku, out i)) throw new EventExceptions.ItemSalesNotCreatedException();
            return i;
        }

        public void clearAllViews()
        {
            foreverCP = 0;
            foreverSP = 0;
            quantitySold = 0;
            sales.Clear();
        }
    }
}
