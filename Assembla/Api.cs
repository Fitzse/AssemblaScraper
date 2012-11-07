using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Assembla.Models;
using FreeMind.Models;

namespace Assembla
{
    public class Api
    {
        private const string LogSource = "Assembla Scraper";
        private const string Log = "Application";
        private readonly NetworkCredential _credentials;
        private int _ticketNumber = 0;

        public Api(string username, string password)
        {
            _credentials = new NetworkCredential(username, password);
        }

        private HttpWebRequest CreateRequest(String url)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request != null)
            {
                request.MaximumAutomaticRedirections = 1;
                request.AllowAutoRedirect = true;

                request.Accept = "application/xml";
                request.ContentType = "application/xml";
                request.Credentials = _credentials;
            }
            return request;
        }

        public IEnumerable<Ticket> GetTicketsForSpace(string subdomain, string spaceName)
        {
            var space = GetSpace(subdomain, spaceName);
            var url = String.Format("https://{0}.assembla.com/spaces/{1}/tickets/", subdomain, space.Id);
            var request = CreateRequest(url);
            if(request != null)
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using(var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        var xDoc = XDocument.Load(reader);
                        var tickets = xDoc.Descendants("ticket").Select(Ticket.FromElement).ToList();
                        var associations = new Dictionary<int, int>();
                        foreach (var t in tickets)
                        {
                            if(!associations.ContainsKey(t.Id))
                            {
                                var children = GetChildren(subdomain, spaceName, t.Number);
                                foreach (var child in children)
                                {
                                    associations.Add(child,t.Id);
                                }
                            }
                        }
                        foreach (var t in tickets.Where(t => associations.ContainsKey(t.Id)))
                        {
                            t.ParentNumber = associations[t.Id];
                        }
                        return tickets;
                    }
                }
            }
            return Enumerable.Empty<Ticket>();
        }

        private Space GetSpace(string subdomain, string spaceName)
        {
            try
            {
                var url = String.Format("https://{0}.assembla.com/spaces/{1}", subdomain, spaceName);
                var request = CreateRequest(url);
                if(request != null)
                {
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        using(var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        {
                            var xDoc = XDocument.Load(reader);
                            var spaceElement = xDoc.Element("space");
                            return Space.FromElement(spaceElement);
                        }
                    }
                }
            }
            catch (Exception)
            {
                var message =
                    String.Format(
                        "The space {0} at {1}.assembla.com could not be found or you do not have permission to access it.",
                        spaceName, subdomain);
                throw new SpaceNotFoundException(message);
            }
            return new Space();
        }

        public IEnumerable<int> GetChildren(string subdomain, string spaceName, int ticketId)
        {
            try
            {
                var space = GetSpace(subdomain, spaceName);
                var url = String.Format("https://{0}.assembla.com/spaces/{1}/tickets/{2}/list_associations", subdomain, space.Id, ticketId);
                var request = CreateRequest(url);
                if(request != null)
                {
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        using(var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        {
                            var xDoc = XDocument.Load(reader);
                            var elements = xDoc.Descendants("ticket-association");
                            return elements.Where(IsParentElement).Select(GetChildId);
                        }
                    }
                }
            }
            catch (Exception)
            {
                var message =
                    String.Format(
                        "The space {0} at {1}.assembla.com could not be found or you do not have permission to access it.",
                        spaceName, subdomain);
                throw new SpaceNotFoundException(message);
            }
            return Enumerable.Empty<int>();
        }
    
        private static bool IsParentElement(XElement element)
        {
            var relationshipElement = element.Element("relationship");
            if(relationshipElement == null)
            {
                return false;
            }

            return Convert.ToInt32(relationshipElement.Value) == 0;
        }
        private static int GetChildId(XElement element)
        {
            var idElement = element.Element("ticket1-id");
            return Convert.ToInt32(idElement.Value);
        }

        public void CreateTicket(string subdomain, string spaceName, Ticket ticket)
        {
            var children = ticket.Children.ToList();
            ticket.Number = _ticketNumber++;
            foreach (var t in children)
            {
                CreateTicket(subdomain, spaceName, t);
            }
            var space = GetSpace(subdomain, spaceName);
            var url = String.Format("https://{0}.assembla.com/spaces/{1}/tickets/", subdomain, space.Id);
            var request = CreateRequest(url);
            request.Method = "POST";
            using (var requestStream = request.GetRequestStream())
            {
                var doc = ticket.ToXDocument();
                var xmlString = Encoding.UTF8.GetBytes(doc.ToString());
                requestStream.Write(xmlString, 0, xmlString.Length);
                try
                {
                    request.GetResponse();
                }
                catch (Exception)
                {
                    var stuff = 10;
                }
            }
            foreach (var child in children)
            {
                try
                {
                    CreateAssociation(subdomain, spaceName, ticket.Number, child.Number);
                }
                catch(Exception)
                {
                    var stuff = 10;
                }
            }
        }

        public void DeleteTicket(string subdomain, string spaceName, int ticketNumber)
        {
            var space = GetSpace(subdomain, spaceName);
            var url = String.Format("https://{0}.assembla.com/spaces/{1}/tickets/{2}", subdomain, space.Id, ticketNumber);
            var request = CreateRequest(url);
            request.Method = "DELETE";
            request.GetResponse();
        }

        public void CreateAssociation(string subdomain, string spaceName, int parentNumber, int childNumber)
        {
            var space = GetSpace(subdomain, spaceName);
            var url = String.Format("https://{0}.assembla.com/spaces/{1}/tickets/{2}/api_add_association", subdomain, space.Id, parentNumber);
            var request = CreateRequest(url);
            request.Method = "Post";
            using(var requestStream = request.GetRequestStream())
            {
                var doc = new XDocument(new XElement("association",
                                        new XElement("with", childNumber, new XAttribute("type", "integer")),
                                        new XElement("relationship", "1")));
                var xmlString = Encoding.UTF8.GetBytes(doc.ToString());
                requestStream.Write(xmlString, 0, xmlString.Length);
                request.GetResponse();
            }
        }

        public IEnumerable<Ticket> GetTicketsFromActor(Actor actor)
        {
            return actor.Stories.Select(x => CreateTicket(actor, x));
        }

        private Ticket CreateTicket(Actor actor, Story story)
        {
            var ticket = new Ticket()
                             {
                                 Description = story.GetNarrative(actor),
                                 Summary = story.Title,
                                 Children = story.Children.Select(x => CreateTicket(actor, x))
                             };
            return ticket;
        }


        class SpaceNotFoundException : Exception
        {
            public SpaceNotFoundException(string message) : base(message)
            {
            }
        }
    }

}
