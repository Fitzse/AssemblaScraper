using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Assembla.Models;

namespace Assembla
{
    public class Api
    {
        private int _ticketNumber;
        private readonly string _key;
        private readonly string _secret;

        public Api(string key, string secret)
        {
            _key = key;
            _secret = secret;
        }

        private HttpWebRequest CreateRequest(String url)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request != null)
            {
                request.MaximumAutomaticRedirections = 1;
                request.Headers.Add("X-Api-Key", _key); 
                request.Headers.Add("X-Api-Secret", _secret);
                request.AllowAutoRedirect = true;

                request.Accept = "application/xml";
                request.ContentType = "application/xml";
            }
            return request;
        }

        public IEnumerable<Ticket> GetTicketsForSpace(string spaceName)
        {
            var url = String.Format("https://api.assembla.com/v1/spaces/{0}/tickets.xml", spaceName);
            var request = CreateRequest(url);
            if(request != null)
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using(var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        var xDoc = XDocument.Load(reader);
                        var tickets = xDoc.Descendants("ticket").Select(Ticket.FromElement).ToList();
                        return tickets;
                    }
                }
            }
            return Enumerable.Empty<Ticket>();
        }

        private Space GetSpace(string spaceName)
        {
            try
            {
                var url = String.Format("https://api.assembla.com/v1/spaces/{0}", spaceName);
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
                        "The space {0} at api.assembla.com could not be found or you do not have permission to access it.",
                        spaceName);
                throw new SpaceNotFoundException(message);
            }
            return new Space();
        }

        public IEnumerable<int> GetChildren(string spaceName, int ticketId)
        {
            try
            {
                var url = String.Format("https://api.assembla.com/v1/spaces/{0}/tickets/{1}/list_associations", spaceName, ticketId);
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
                        "The space {0} at api.assembla.com could not be found or you do not have permission to access it.",
                        spaceName);
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

        public int CreateTicket(string spaceName, Ticket ticket)
        {
            var children = ticket.Children.ToList();
            ticket.Number = _ticketNumber++;
            foreach (var t in children)
            {
                t.Id = CreateTicket(spaceName, t);
            }
            var url = String.Format("https://api.assembla.com/v1/spaces/{0}/tickets.xml", spaceName);
            var request = CreateRequest(url);
            request.Method = "POST";
            using (var requestStream = request.GetRequestStream())
            {
                var doc = ticket.ToXDocument();
                var xmlString = Encoding.UTF8.GetBytes(doc.ToString());
                requestStream.Write(xmlString, 0, xmlString.Length);
                using(var response = request.GetResponse())
                using(var responseStream = response.GetResponseStream())
                {
                    var responseDoc = XDocument.Load(responseStream);
                    var ticketRoot = responseDoc.Element("ticket");
                    if(ticketRoot != null)
                    {
                        ticket.Id = Convert.ToInt32(ticketRoot.Element("id").Value);
                    }
                };
            }
            foreach (var child in children)
            {
                CreateAssociation(spaceName, ticket.Number, child.Id);
            }
            return ticket.Id;
        }

        public void DeleteTicket(string spaceName, int ticketNumber)
        {
            var url = String.Format("https://api.assembla.com/v1/spaces/{0}/tickets/{1}", spaceName, ticketNumber);
            var request = CreateRequest(url);
            request.Method = "DELETE";
            request.GetResponse();
        }

        public void CreateAssociation(string spaceName, int parentNumber, int childNumber)
        {
            var url = String.Format("https://api.assembla.com/v1/spaces/{0}/tickets/{1}/ticket_associations.xml", spaceName, parentNumber);
            var request = CreateRequest(url);
            request.Method = "POST";
            using(var requestStream = request.GetRequestStream())
            {
                var doc = new XDocument(new XElement("ticket_association",
                                        new XElement("ticket2_id", childNumber),
                                        new XElement("relationship", "1")));
                var xmlString = Encoding.UTF8.GetBytes(doc.ToString());
                requestStream.Write(xmlString, 0, xmlString.Length);
                request.GetResponse();
            }
        }

        class SpaceNotFoundException : Exception
        {
            public SpaceNotFoundException(string message) : base(message)
            {
            }
        }
    }

}
