using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.WindowsAzure.Storage.Table;
using Assignment_2.Entities;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;

namespace Assignment_2.Hubs
{
    public class ChatHub : Hub
    {

        public void Send(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.addNewMessageToPage(name, message);
        }

        public void Load(string name)
        {
            getAllWherePartition(name);
            //getAllRecords(name);

            Clients.Caller.addNewMessageToPage("Server",name+" received past events");
        }


        public void getAllWherePartition(string partKey)
        {

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("people");

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<CustomerEntity> query = new TableQuery<CustomerEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,partKey));

            // Print the fields for each customer.
            foreach (CustomerEntity entity in table.ExecuteQuery(query))
            {
                Clients.Caller.addNewMessageToPage("Server", entity.PartitionKey + " " + entity.RowKey);
            }
        }

        public void getAllRecords(string name)
        { 
            //Make this thing async later

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("people");
           
            TableQuery<CustomerEntity> query = new TableQuery<CustomerEntity>();
            
            foreach (CustomerEntity entity in table.ExecuteQuery(query))
            {
                Clients.Caller.addNewMessageToPage("Server", entity.PartitionKey + " " +entity.RowKey);
            }
        
        }
    }
}