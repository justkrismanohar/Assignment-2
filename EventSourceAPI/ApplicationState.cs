using EventSourceAPI.Events;
using EventSourceAPI.Models;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EventSourceAPI
{

    //A Singleton which represents the
    //net effect of all the events
    
    //Can be queired for differnt views
    //Maintains different views of the data
    //Exposes an API to preforms actions on the state
    //The implemented actions are responsible for 
    //mainting the views
    public class ApplicationState
    {
        private static string connectionStr = "DefaultEndpointsProtocol = https; AccountName=kmassignment2;AccountKey=4YRs3Z0rQPKGiccqqzH3Z5DGHwTV0XoP/uk/JonuPZTpC5mtvWWNxBS83UfSU5Fh9t5E7M6qAnKGkF8aVpmX4w==";
        private static readonly object padlock = new object();
        private static ApplicationState state = null;

        private Dictionary<string,Dictionary<string, Item>> itemStore;
        private Dictionary<string,Dictionary<string, Price>> prices;

        //Maintains the view of the over all cash floow (aggregate of all locations)
        private LocationSales allsales;

        //Maintains the view of the over all cash flow by location
        private Dictionary<string, LocationSales> sales;
        //Maintains the view of the over daily cash flow by location
        private Dictionary<string, LocationSales> dailySales;
        //Maintains the view of the over monthly cash flow by location
        private Dictionary<string, LocationSales> monthlySales;

        //Thread safe singleton
        public async static Task<ApplicationState> getApplicationState()
        {
            if(state == null)
            {
                lock (padlock)
                {
                    if (state == null)
                    {
                       state = new ApplicationState();
                    }
                }

                state = await EventLog.buildFromStart();      
            }

            return state;
        }

        public ApplicationState()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                   Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));
            Config.storageAccount = storageAccount;

            itemStore = new Dictionary<string, Dictionary<string, Item>>();

            prices = new Dictionary<string, Dictionary<string, Price>>();

            sales = new Dictionary<string,LocationSales>();
            dailySales = new Dictionary<string, LocationSales>();
            monthlySales = new Dictionary<string, LocationSales>();

            allsales = new LocationSales();
        }

        public string getSKU(string name)
        {
            name = name.Replace(' ', '-');
            return name;
        }

        public string createItem(string name, string location)
        {
            //get item list for location
            Dictionary<string, Item> items;

            if (!itemStore.TryGetValue(location, out items))
            {
                items = new Dictionary<string, Item>();
                itemStore.Add(location, items);
            }

            //generate sku for item. This is unique for each item.
            string sku = getSKU(name);
            
            //insert into item map
            items.Add(sku, new Item()
            { 
                name = name, location = location, sku = sku, quantity = 0
            });

            return sku;
        }

        //Adds n items to the store
        public int stockItem(string location, string sku,int n)
        {
            Dictionary<string, Item> items;

            if (itemStore.TryGetValue(location, out items))
            {
                Item i;
                if (items.TryGetValue(sku, out i))
                {
                    i.quantity = i.quantity + n;
                    return i.quantity;
                }
                else throw new EventExceptions.ItemNotCreatedExpection();
            }
            else throw new EventExceptions.LocationNotCreatedExpection();
        }
        
        //sets or udpates the price of an item
        public void setPrice(string location, string sku,Decimal cp, Decimal sp)
        {
            Dictionary<string, Price> localPrices;

            if (!prices.TryGetValue(location, out localPrices))
            {
                localPrices = new Dictionary<string, Price>();
                prices.Add(location, localPrices);
            }
            
            Price p;
            if (!localPrices.TryGetValue(sku, out p))
            {
                p = new Price();
                localPrices.Add(sku, p);
            }

            p.costPrice = cp;
            p.sellingPrice = sp;
        }

        public void itemSold(string location, string sku, int n)
        {
            //Update views
            itemSoldUpdateStore(sales, location, sku, n);
            itemSoldUpdateStore(dailySales, location, sku, n);
            itemSoldUpdateStore(monthlySales, location, sku, n);

            Price p = getPrice(location, sku);
            allsales.itemSold(sku, p.costPrice, p.sellingPrice, n);

            //Update quantity in the inventory
            Dictionary<string, Item> localItems;
            if (!itemStore.TryGetValue(location, out localItems)) throw new EventExceptions.LocationStoreNotCreatedException();
            Item i;
            if (!localItems.TryGetValue(sku, out i)) throw new EventExceptions.ItemNotCreatedExpection();
            i.quantity = i.quantity - n;
        }

        private Price getPrice(string location, string sku)
        {
            //Look up the price
            Dictionary<string, Price> localPrices;
            if (!prices.TryGetValue(location, out localPrices)) throw new EventExceptions.NoLocalPriceException();
            Price p;
            if (!localPrices.TryGetValue(sku, out p)) throw new EventExceptions.NoPriceCreatedException();

            return p;
        }

        private void itemSoldUpdateStore(Dictionary<string, LocationSales> salesStore, string location, string sku,int n)
        {
            //Look up the price
            Price p = getPrice(location, sku);
            //Lookup sales aggregates
            LocationSales localSales;
            if (!salesStore.TryGetValue(location, out localSales)){
                localSales = new LocationSales();
                salesStore.Add(location, localSales);
            }

            localSales.itemSold(sku, p.costPrice, p.sellingPrice,n);
        }


        //Query APIs
        public double getForeverCP()
        {
            return Decimal.ToDouble(allsales.foreverCP);
        }

        public double getForeverSP()
        {
            return Decimal.ToDouble(allsales.foreverSP);
        }
    
        public LocationSales getSalesOverAllLocations()
        {
            return allsales;
        }

        public LocationSales getDailySalesForLocation(string location)
        {

            LocationSales localSales;
            if (!dailySales.TryGetValue(location, out localSales)) throw new EventExceptions.LocalSalesNotCreatedException();
            return localSales;
      
        }

        public ItemSales getDialySailesForItem(string location, string sku)
        {
            LocationSales localSales;
            if (!dailySales.TryGetValue(location, out localSales)) throw new EventExceptions.LocalSalesNotCreatedException();

            return localSales.getItem(sku);
        }

        public List<Item> getItemStock(string location)
        {
            Dictionary<string, Item> localItems;
            if (!itemStore.TryGetValue(location, out localItems)) throw new EventExceptions.ItemStoreNotCreatedException();

            return localItems.Values.ToList<Item>();
        }

        public Item getItemStockFromLocation(string location, string sku)
        {
            Dictionary<string, Item> localItems;
            if (!itemStore.TryGetValue(location, out localItems)) throw new EventExceptions.ItemStoreNotCreatedException();
            Item i;
            if (!localItems.TryGetValue(sku, out i)) throw new EventExceptions.ItemNotCreatedExpection();
            return i;
        }

        public void resetAllViews()
        {
            itemStore.Clear();
            prices.Clear();
            sales.Clear();
            dailySales.Clear();
            monthlySales.Clear();
            allsales.clearAllViews();
        }
    }
}