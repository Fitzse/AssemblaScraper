using System.Web.Mvc;
using System.Web.Routing;

namespace AssemblaScaper
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "SpecificationSubDomain",
                url: "{space}/{secret}/{action}",
                defaults: new { controller = "Specifications", action = "Tickets"}
            );
        }
    }
}