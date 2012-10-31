using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AssemblaScaper.Controllers
{
    public class SpecificationsController : Controller
    {
        //
        // GET: /Specifications/

        public ActionResult Index(string subdomain, string space, string username, string password)
        {
            var assemblaApi = new Assembla.Api(username, password);
            var tickets = assemblaApi.GetTicketsForSpace(subdomain ?? "www", space);
            ViewData["space"] = space;
            return View(tickets);
        }

    }
}