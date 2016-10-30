using Owin;
using Microsoft.Owin;
[assembly: OwinStartup(typeof(Assignment_2.Hubs.Startup))]

namespace Assignment_2.Hubs
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}
