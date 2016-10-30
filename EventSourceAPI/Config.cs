using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourceAPI
{
    class Config
    {
        public static CloudStorageAccount storageAccount { get; set; }
        //Push this to an .config file later
        public static string eventLogTbl = "EventLog";
        public static string partFormat = "yyyy-MM-dd-HH-mm-ss-fff";
       // public static string rowFormat = "HH:mm:ss.fff";

    }
}
