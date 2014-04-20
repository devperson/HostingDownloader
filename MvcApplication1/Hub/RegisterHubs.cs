using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;

[assembly: PreApplicationStartMethod(typeof(MvcApplication1.RegisterHubs), "Start")]

namespace MvcApplication1
{
    public static class RegisterHubs
    {
        public static void Start()
        {
            var config = new HubConfiguration
            {
                EnableCrossDomain = true
            };

            RouteTable.Routes.MapHubs(config);
        }
    }
}
