using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventSourceAPI.Events
{
    public class TablesDeleted : IEvent
    {
        public override void execute(ApplicationState state)
        {
            try
            {
                state.resetAllViews();

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                                CloudConfigurationManager.GetSetting("StorageConnectionString"));

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table;

                table = tableClient.GetTableReference("EventLog");
                table.DeleteIfExists();

            }
            catch (Exception e)
            {

            }
        }

        public void notifyClients()
        {
            throw new NotImplementedException();
        }
    }
}