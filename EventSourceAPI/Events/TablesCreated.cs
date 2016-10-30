using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventSourceAPI.Events
{
    public class TablesCreated :IEvent
    {
        public override void execute(ApplicationState state)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                                CloudConfigurationManager.GetSetting("StorageConnectionString"));

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table;

                table = tableClient.GetTableReference(Config.eventLogTbl);
                table.CreateIfNotExists();
                
            }
            catch(Exception e)
            {

            }
        }
    }
}