using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourceAPI.Events
{
    public abstract class IEvent
    {
        //This method is used to update
        //some application state
        //This effectively replays the event
        public abstract void execute(ApplicationState state);
    
    }
}
