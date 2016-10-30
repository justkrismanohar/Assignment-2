using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventSourceAPI.Events
{
    public class LocationCreated : IEvent
    {
        public override void execute(ApplicationState state)
        {
            throw new NotImplementedException();
        }

        public void notifyClients()
        {
            throw new NotImplementedException();
        }
    }
}