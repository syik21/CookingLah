using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(CookingLah.Startup))]

namespace CookingLah
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}