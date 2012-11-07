using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Assembla.Models;

namespace AssemblaScaper.Controllers
{
    public class SpecificationsController : Controller
    {
        //
        // GET: /Specifications/
        private Assembla.Api _api;
        private string _subdomain;
        private string _spaceName;

        public ActionResult Index(string subdomain, string space, string username, string password)
        {
            _subdomain = subdomain;
            _spaceName = space;
            _api = new Assembla.Api(username, password);
            var tickets = _api.GetTicketsForSpace(_subdomain ?? "www", space);
            ViewData["space"] = space;
            return View(tickets);
        }

        public ActionResult Create(string subdomain, string space, string username, string password)
        {
            _api = new Assembla.Api(username, password);
            var actors = FreeMind.Converter.GetActorsFromFile(@"D:\Personal\AssemblaScaper\AssemblaScaper\UserStories.mm");

            for (int i = 0; i < 60;i++ )
            {
                try
                {
                    _api.DeleteTicket(subdomain, space, i);
                }
                catch(Exception){}
            }

            foreach (var actor in actors)
            {
                var tickets = _api.GetTicketsFromActor(actor);
                foreach (var ticket in tickets)
                {
                    _api.CreateTicket(subdomain, space, ticket);
                }
            }
            return RedirectToAction("Index", new { subdomain, space, username, password });
        }
    }
}