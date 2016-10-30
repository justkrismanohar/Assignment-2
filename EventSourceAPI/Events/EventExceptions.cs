using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourceAPI.Events
{
    public class EventExceptions
    {
        public class LocationNotCreatedExpection : Exception { }
        public class ItemNotCreatedExpection : Exception { }
        public class NoLocalPriceException : Exception { }
        public class NoPriceCreatedException : Exception { }
        public class LocationStoreNotCreatedException : Exception { }
        public class LocalSalesNotCreatedException : Exception { }
        public class ItemSalesNotCreatedException : Exception { }
        public class ItemStoreNotCreatedException : Exception { }
        
    }
}
