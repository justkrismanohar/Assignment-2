using EventSourceAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;
using EventSourceAPI.Events;
using EventSourceAPI.Helpers;

namespace EventSourceAPITester
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncContext.Run(() => MainAsync(args));
        }



        static async void MainAsync(string[] args)
        {
            ApplicationState state = await ApplicationState.getApplicationState();
            /*
           
            string system1 = "SYS-FYZO-1";
            string system2 = "SYS-SIPARIA-1";

            ItemCreated fyzoBB = Helper.createItem(state, system1, "Fyzo", "Blue Band");
            ItemCreated fyzoGR = Helper.createItem(state, system1, "Fyzo", "Golden Ray");

            ItemStocked fyzoStockBB = Helper.stockItem(state, system1, fyzoBB.location, fyzoBB.sku, 10);
            ItemStocked fyzoStockGR = Helper.stockItem(state, system1, fyzoGR.location, fyzoGR.sku, 100);

            PriceSetted fyzoBBPrice = Helper.setPrice(state, system1, fyzoBB.location, fyzoBB.sku, 10.99, 15.99);
            PriceSetted fyzoGRPrice = Helper.setPrice(state, system1, fyzoGR.location, fyzoGR.sku, 5.99, 9.99);

            OrderSold order1 = new OrderSold()
            {
                location = fyzoBB.location
            };

            order1.items.Add(fyzoBB.sku, 3);
            order1.items.Add(fyzoGR.sku, 4);
            order1.execute(state);
            EventLog.log(system1, order1);
            */

            //Console.WriteLine("Event Source API Tester");
            ApplicationState fromLog = await EventLog.buildFromStart();

            Console.WriteLine("Total CP $ {0}\n Total SP $ {1} \n", fromLog.getForeverCP(), fromLog.getForeverSP());
            foreach(var i in fromLog.getSalesOverAllLocations().sales)
            {
                Console.WriteLine("{0}   {1}    {2}    {3}", i.Key, i.Value.quantitySold, i.Value.foreverCP, i.Value.foreverSP);
            }

            Console.ReadLine();
        }

    }
}
