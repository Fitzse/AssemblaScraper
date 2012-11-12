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
                url: "{space}/{secret}/tickets",
                defaults: new { controller = "Specifications", action = "Index"}
            );

            routes.MapRoute(
                name: "CreateSubDomain",
                url: "{space}/{secret}/create",
                defaults: new { controller = "Specifications", action = "Create"}
            );
        }
    }
}