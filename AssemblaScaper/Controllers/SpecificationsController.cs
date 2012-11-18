using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Assembla.Models;
using FreeMind.Models;

namespace AssemblaScaper.Controllers
{
    public class SpecificationsController : Controller
    {
        private Assembla.Api _api;
        private readonly string _apiKey;

        public SpecificationsController()
        {
            _apiKey = ConfigurationManager.AppSettings["ApiKey"];
        }

        public ActionResult Tickets(string space, string secret)
        {
            _api = new Assembla.Api(_apiKey, secret);
            var tickets = _api.GetTicketsForSpace(space);
            ViewData["space"] = space;
            return View(tickets.OrderBy(x => x.Summary));
        }

        public ActionResult Create(string space, string secret)
        {
            _api = new Assembla.Api(_apiKey, secret);
            var actors =
                FreeMind.Converter.GetActorsFromFile(@"D:\Personal\AssemblaScaper\AssemblaScaper\UserStories.mm");
            ResetTIcketNumbering(space);

            foreach (var actor in actors)
            {
                var tickets = GetTicketsFromActor(actor);
                foreach (var ticket in tickets)
                {
                    _api.CreateTicket(space, ticket);
                }
            }

            return RedirectToAction("Tickets", new {space, secret});
        }

        private void ResetTIcketNumbering(string space)
        {
            var existingTickets = _api.GetTicketsForSpace(space).ToList();
            var highestTicket = 0;
            if (existingTickets.Any())
                highestTicket = existingTickets.Max(x => x.Number);
            _api.ResetTicketNumber(highestTicket + 1);
        }

        private IEnumerable<Ticket> GetTicketsFromActor(Actor actor)
        {
            return actor.Stories.Select(x => CreateTicket(actor, x));
        }

        private Ticket CreateTicket(Actor actor, Story story)
        {
            var ticket = new Ticket()
            {
                Description = story.GetNarrative(actor),
                Summary = story.Title,
                Children = story.Children.Select(x => CreateTicket(actor, x)),
                Actor = actor.Name
            };
            return ticket;
        }

        public ActionResult Delete(string space, string secret)
        {
            _api = new Assembla.Api(_apiKey, secret);
            var tickets = _api.GetTicketsForSpace(space).Select(x => x.Number).ToList();

            foreach(var t in tickets)
            {
                try
                {
                    _api.DeleteTicket(space, t);
                }
                catch (Exception)
                {
                }
            }

            return RedirectToAction("Tickets", new {space, secret});
        }
}
}