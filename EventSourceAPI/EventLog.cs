using EventSourceAPI.Events;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourceAPI
{
    public class EventLog
    {

        public static void testLoad()
        {

            IEvent e;
            List<ITableEntity> list = new List<ITableEntity>();
            for (int i = 0; i < 500; i++)
            {
                e = new ItemCreated()
                {
                    location = "Fyzo_" + i,
                    name = "Butter_" + i
                };
                EventLog.log("SYS-"+i,e);
                //Console.WriteLine("Created item {0}", i);
            }
     
        }

        public static async Task BatchInsert(CloudTable table, List<ITableEntity> entities)
        {
            int rowOffset = 0;

            while (rowOffset < entities.Count)
            {
                Stopwatch sw = Stopwatch.StartNew();

                var batch = new TableBatchOperation();

                // next batch
                var rows = entities.Skip(rowOffset).Take(100).ToList();

                foreach (var row in rows)
                    batch.Insert(row);

                // submit
                await table.ExecuteBatchAsync(batch);

                rowOffset += rows.Count;

                Trace.TraceInformation("Elapsed time to batch insert " + rows.Count + " rows: " + sw.Elapsed.ToString("g"));
            }
        }

        public static async void log(string systemId,IEvent data)
        {
            CloudTableClient tableClient = Config.storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(Config.eventLogTbl);
            SerializedEvent json = new SerializedEvent()
            {
                RowKey = systemId,
                eventType = data.ToString(),
                json = JsonConvert.SerializeObject(data)
            };

            TableOperation insertOperation = TableOperation.Insert(json);
            //await table.ExecuteAsync(insertOperation);
            table.Execute(insertOperation);//make sync to test with console
        }

        public static async Task<ApplicationState> buildFromStart()
        {
            ApplicationState state = new ApplicationState();

            CloudTableClient tableClient = Config.storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(Config.eventLogTbl);

            TableQuery<SerializedEvent> query = new TableQuery<SerializedEvent>().Take(100);
            List<SerializedEvent> results = new List<SerializedEvent>();
            TableContinuationToken token = null;
            IEvent e;

            try
            {
                do
                {
                    var part = await table.ExecuteQuerySegmentedAsync(query, token);
                    token = part.ContinuationToken;
                    foreach (SerializedEvent se in part.Results)
                    {
                        //Convert e to IEvent
                        e = deserialize(se);
                        //Playback on state
                        e.execute(state);
                        Console.WriteLine("Received {0} {1}", se.PartitionKey, se.RowKey);

                    }
                }
                while (token != null);
            }
            catch(Exception exp)
            {

            }

            return state;
        }

        public static ApplicationState buildFromStartSync()
        {
            ApplicationState state = new ApplicationState();

            CloudTableClient tableClient = Config.storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(Config.eventLogTbl);

            TableQuery<SerializedEvent> query = new TableQuery<SerializedEvent>().Take(100);
            List<SerializedEvent> results = new List<SerializedEvent>();
            TableContinuationToken token = null;
            IEvent e;

            try
            {
                do
                {
                    var part = table.ExecuteQuerySegmented(query, token);
                    token = part.ContinuationToken;
                    foreach (SerializedEvent se in part.Results)
                    {
                        //Convert e to IEvent
                        e = deserialize(se);
                        //Playback on state
                        e.execute(state);
                        Console.WriteLine("Received {0} {1}", se.PartitionKey, se.RowKey);
                    }
                }
                while (token != null);
            }
            catch (Exception exp)
            {

            }

            return state;
        }

        private static IEvent deserialize(SerializedEvent se)
        {
            IEvent e = null;
            if(se.eventType == "EventSourceAPI.Events.ItemCreated")
            {
                e = JsonConvert.DeserializeObject<ItemCreated>(se.json);
            }
            else
            if (se.eventType == "EventSourceAPI.Events.ItemStocked")
            {
                e = JsonConvert.DeserializeObject<ItemStocked>(se.json);
            }
            else
            if (se.eventType == "EventSourceAPI.Events.PriceSetted")
            {
                e = JsonConvert.DeserializeObject<PriceSetted>(se.json);
            }
            else
            if (se.eventType == "EventSourceAPI.Events.OrderSold")
            {
                e = JsonConvert.DeserializeObject<OrderSold>(se.json);
            }

          
            return e;
        }

    }
}
