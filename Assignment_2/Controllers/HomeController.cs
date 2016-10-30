using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using Assignment_2.Entities;
using Microsoft.AspNet.SignalR;
using Assignment_2.Hubs;
using System.Threading.Tasks;
using EventSourceAPI;
using EventSourceAPI.Events;
using EventSourceAPI.Helpers;
using EventSourceAPI.Models;
using Assignment_2.Models;
//using Assignment_2.Models;

namespace Assignment_2.Controllers
{
    public class HomeController : AsyncController
    {
        //Simiulated systemId
        //This information will come from another system
        string system1 = "SYS-FYZO-1";
        string system2 = "SYS-SIPARIA-1";

        //Deletes Event Log table
        public async Task<ActionResult> Delete()
        {
            ApplicationState state = await ApplicationState.getApplicationState();
            IEvent e = new TablesDeleted();
            e.execute(state);
            var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            context.Clients.All.updateItemSales("","",0,0);
            return View();
        }

        //Creates Event Log table
        public async Task<ActionResult> Create()
        {
            ViewBag.message = true;
            try
            {


                ApplicationState state = await ApplicationState.getApplicationState();
                IEvent e = new TablesCreated();
                e.execute(state);
                return View();
            }
            catch(Exception e)
            {
                ViewBag.message = false;
                return View();
            }
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }

        //Summary over all locations
        public async Task<ActionResult> Summary()
        {
            ApplicationState state = await ApplicationState.getApplicationState();
            ViewBag.totalCp = state.getForeverCP();
            ViewBag.totalSp = state.getForeverSP();

            LocationSales allSales = state.getSalesOverAllLocations();
            List<ItemSalesInfo> data = new List<ItemSalesInfo>();
            foreach (var i in allSales.sales)
            {
                data.Add(new ItemSalesInfo()
                {
                    name = i.Key,
                    foreverCP = (double)i.Value.foreverCP,
                    foreverSP = (double)i.Value.foreverSP,
                    quantitySold = i.Value.quantitySold
                });
            };
            return View(data);
        }

        //Inventory for a single location
        public async Task<ActionResult> Inventory(string id)
        {
            ViewBag.message = true;
            if(id == null)
            {
                ViewBag.message = false;
                return View();
            }
            
            try
            {
                string location = id;
                ApplicationState state = await ApplicationState.getApplicationState();
                List<Item> localItems = state.getItemStock(location);
                List<ItemInfo> data = new List<ItemInfo>();
                foreach (var i in localItems)
                {
                    data.Add(new ItemInfo()
                    {
                        name = i.name,
                        quantity = i.quantity,
                        sku = i.sku
                    });
                };

                ViewBag.location = location;
                return View(data);
            }
            catch(Exception e)
            {
                ViewBag.message = false;
                return View();
            }
        }

        //Summary for a single location
        public async Task<ActionResult> DailySummary(string id){

           
            ViewBag.message = true;
            if (id == null)
            {
                ViewBag.message = false;
                return View();
            }

            try
            {
                string location = id;
                ApplicationState state = await ApplicationState.getApplicationState();
                LocationSales localSales = state.getDailySalesForLocation(location);
                List<ItemSalesInfo> data = new List<ItemSalesInfo>();
                foreach (var i in localSales.sales)
                {
                    data.Add(new ItemSalesInfo()
                    {
                        name = i.Key,
                        foreverCP = (double)i.Value.foreverCP,
                        foreverSP = (double)i.Value.foreverSP,
                        quantitySold = i.Value.quantitySold
                    });
                };

                ViewBag.location = location;
                return View(data);
            }
            catch(Exception e)
            {
                ViewBag.message = false;
                return View();
            }
        }
 
        //Simulates placing an order at a location
        public async Task<ActionResult> TestOrder(string id)
        {

            ViewBag.message = true;
            if(id == null)
            {
                ViewBag.message = false;
                return View();
            }

            ViewBag.location = id;

            try
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                ApplicationState state = await ApplicationState.getApplicationState();

                OrderSold order1 = new OrderSold()
                {
                    location = id
                };

                order1.items.Add("Blue-Band", 1);
                order1.items.Add("Golden-Ray", 1);
                order1.items.Add("Kiss-Bread", 2);
                order1.items.Add("Crix", 4);

                order1.execute(state);
                EventLog.log(system1, order1);

                //Notify clients view about changes to each item
                ItemSales i;
                Item iStock;
                foreach (var j in order1.items)
                {
                    i = state.getDialySailesForItem(order1.location, j.Key);
                    //notify Views
                    context.Clients.All.updateItemSales(order1.location, j.Key, i.quantitySold, i.foreverCP, i.foreverSP);

                    iStock = state.getItemStockFromLocation(order1.location, j.Key);
                    context.Clients.All.updateItemStock(order1.location, iStock.name, iStock.sku, iStock.quantity);

                }


                context.Clients.All.updateSummary(state.getForeverCP(), state.getForeverSP());
                return View();
            }
            catch(Exception e)
            {
                ViewBag.message = false;
                return View();
            }
            
        }

        //Simuluates the population of inventory meta data
        public async Task<ActionResult> TestLoadAsync(string id)
        {

            ViewBag.location = id;
            ViewBag.message = true;
            ViewBag.loaded = false;

            if(id == null)
            {
                ViewBag.message = false;
                return View();
            }

            try
            {
                string location = id;
                var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                ApplicationState state = await ApplicationState.getApplicationState();

                ItemCreated fyzoBB = Helper.createItem(state, system1, location, "Blue Band");
                context.Clients.All.addNewMessageToPage("server", location + " create " + fyzoBB.sku);

                ItemCreated fyzoGR = Helper.createItem(state, system1, location, "Golden Ray");
                context.Clients.All.addNewMessageToPage("server", location + " create " + fyzoGR.sku);

                ItemCreated fyzoKB = Helper.createItem(state, system1, location, "Kiss Bread");
                context.Clients.All.addNewMessageToPage("server", location + " create " + fyzoKB.sku);

                ItemCreated fyzoC = Helper.createItem(state, system1, location, "Crix");
                context.Clients.All.addNewMessageToPage("server", location + " create " + fyzoC.sku);

                ItemStocked fyzoStockBB = Helper.stockItem(state, system1, fyzoBB.location, fyzoBB.sku, 10);
                context.Clients.All.addNewMessageToPage("server", location + " stocked " + fyzoStockBB.sku + " " + fyzoStockBB.n);

                ItemStocked fyzoStockGR = Helper.stockItem(state, system1, fyzoGR.location, fyzoGR.sku, 100);
                context.Clients.All.addNewMessageToPage("server", location + " stocked " + fyzoStockGR.sku + " " + fyzoStockGR.n);

                ItemStocked fyzoStockKB = Helper.stockItem(state, system1, fyzoKB.location, fyzoKB.sku, 100);
                context.Clients.All.addNewMessageToPage("server", location + " stocked " + fyzoStockKB.sku + " " + fyzoStockKB.n);

                ItemStocked fyzoStockC = Helper.stockItem(state, system1, fyzoC.location, fyzoC.sku, 100);
                context.Clients.All.addNewMessageToPage("server", location + " stocked " + fyzoStockC.sku + " " + fyzoStockC.n);


                PriceSetted fyzoBBPrice = Helper.setPrice(state, system1, fyzoBB.location, fyzoBB.sku, 10.99, 15.99);
                context.Clients.All.addNewMessageToPage("server", location + " set price " + fyzoBBPrice.sku + " " + fyzoBBPrice.cp + " " + fyzoBBPrice.sp);

                PriceSetted fyzoGRPrice = Helper.setPrice(state, system1, fyzoGR.location, fyzoGR.sku, 5.99, 9.99);
                context.Clients.All.addNewMessageToPage("server", location + " set price " + fyzoGRPrice.sku + " " + fyzoGRPrice.cp + " " + fyzoGRPrice.sp);

                PriceSetted fyzoKBPrice = Helper.setPrice(state, system1, fyzoKB.location, fyzoKB.sku, 6.99, 12.99);
                context.Clients.All.addNewMessageToPage("server", location + " set price " + fyzoKBPrice.sku + " " + fyzoKBPrice.cp + " " + fyzoKBPrice.sp);

                PriceSetted fyzoCPrice = Helper.setPrice(state, system1, fyzoC.location, fyzoC.sku, 8.99, 10.99);
                context.Clients.All.addNewMessageToPage("server", location + " set price " + fyzoCPrice.sku + " " + fyzoCPrice.cp + " " + fyzoCPrice.sp);
               
                return View();

            }
            catch(Exception e)
            {
                ViewBag.loaded = true;
                ViewBag.message = false;
                return View();
            }
            
        }

        //Simulates adding items to the inventory
        public async Task<ActionResult> testStock(string id)
        {
            
            var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            ApplicationState state = await ApplicationState.getApplicationState();
            ViewBag.location = id;
            ViewBag.message = true;

            if(id == null)
            {
                ViewBag.message = false;
                return View();
            }

            try
            {
                string location = id;
                string bbsku = "Blue-Band";
                string grsku = "Golden-Ray";
                string kbsku = "Kiss-Bread";
                string csku = "Crix";

                ItemStocked fyzoStockBB = Helper.stockItem(state, system1, location, bbsku, 10);
                context.Clients.All.addNewMessageToPage("server", "stocked " + fyzoStockBB.sku + " " + fyzoStockBB.n);
                Item i = state.getItemStockFromLocation(location, bbsku);
                context.Clients.All.updateItemStock(location, "Blue Band", bbsku, i.quantity);

                ItemStocked fyzoStockGR = Helper.stockItem(state, system1, location, grsku, 100);
                context.Clients.All.addNewMessageToPage("server", "stocked " + fyzoStockGR.sku + " " + fyzoStockGR.n);
                i = state.getItemStockFromLocation(location, grsku);
                context.Clients.All.updateItemStock(location, "Golden Ray", grsku, i.quantity);

                ItemStocked fyzoStockKB = Helper.stockItem(state, system1, location, kbsku, 100);
                context.Clients.All.addNewMessageToPage("server", "stocked " + fyzoStockKB.sku + " " + fyzoStockKB.n);
                i = state.getItemStockFromLocation(location, kbsku);
                context.Clients.All.updateItemStock(location, "Kiss Bread", kbsku, i.quantity);

                ItemStocked fyzoStockC = Helper.stockItem(state, system1, location, csku, 100);
                context.Clients.All.addNewMessageToPage("server", "stocked " + fyzoStockC.sku + " " + fyzoStockC.n);
                i = state.getItemStockFromLocation(location, csku);
                context.Clients.All.updateItemStock(location, "Crix", csku, i.quantity);

                ViewBag.location = location;
                return View();
            }
            catch(Exception e)
            {
                ViewBag.message = false;
                return View();
            }
        }
        
        //Activity log
        public ActionResult Log()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}