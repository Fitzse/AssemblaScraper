using System;
using System.Configuration;
using System.Web.Mvc;

namespace AssemblaScaper.Controllers
{
    public class SpecificationsController : Controller
    {
        //
        // GET: /Specifications/
        private Assembla.Api _api;
        private readonly string _apiKey ;

        public SpecificationsController()
        {
            _apiKey = ConfigurationManager.AppSettings["ApiKey"];
        }

        public ActionResult Index(string space, string secret)
        {
            _api = new Assembla.Api(_apiKey, secret);
            var tickets = _api.GetTicketsForSpace(space);
            ViewData["space"] = space;
            return View(tickets);
        }

        public ActionResult Create(string space, string secret)
        {
            _api = new Assembla.Api(_apiKey, secret);
            var actors = FreeMind.Converter.GetActorsFromFile(@"D:\Personal\AssemblaScaper\AssemblaScaper\UserStories.mm");

            for (int i = 0; i < 60;i++ )
            {
                try
                {
                    _api.DeleteTicket(space, i);
                }
                catch(Exception){}
            }

            foreach (var actor in actors)
            {
                var tickets = _api.GetTicketsFromActor(actor);
                foreach (var ticket in tickets)
                {
                    _api.CreateTicket(space, ticket);
                }
            }
            return RedirectToAction("Index", new { space, secret});
        }
    }
}