using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourceAPI
{
    public class SerializedEvent : TableEntity
    {
        public string json { get; set; }
        public string eventType { get; set; }

        public SerializedEvent()
        {
            PartitionKey = DateTime.UtcNow.ToString(Config.partFormat);
            //RowKey = DateTime.UtcNow.ToString(Config.rowFormat);
        }
    }
}
