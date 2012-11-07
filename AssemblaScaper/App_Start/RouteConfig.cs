﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
                url: "Specifications/{subdomain}/{space}",
                defaults: new { controller = "Specifications", action = "Index"}
            );

            routes.MapRoute(
                name: "CreateSubDomain",
                url: "Create/{subdomain}/{space}",
                defaults: new { controller = "Specifications", action = "Create"}
            );
        }
    }
}