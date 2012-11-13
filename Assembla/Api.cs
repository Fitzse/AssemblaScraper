using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Assembla.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        private HttpWebRequest CreateXmlRequest(String url)
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

        private HttpWebRequest CreateJsonRequest(String url)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request != null)
            {
                request.MaximumAutomaticRedirections = 1;
                request.Headers.Add("X-Api-Key", _key); 
                request.Headers.Add("X-Api-Secret", _secret);
                request.AllowAutoRedirect = true;

                request.Accept = "application/json";
                request.ContentType = "application/json";
            }
            return request;
        }

        public IEnumerable<Ticket> GetTicketsForSpace(string spaceName)
        {
            var url = String.Format("https://api.assembla.com/v1/spaces/{0}/tickets.json", spaceName);
            var json = GetJArrayResponse(url);
            return json.ToObject<IEnumerable<Ticket>>();
        }

        private JArray GetJArrayResponse(string url)
        {
            var request = CreateXmlRequest(url);
            if(request != null)
            {
                using (var response = request.GetResponse())
                using(var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    var jsonString = reader.ReadToEnd();
                    return JArray.Parse(jsonString);
                }
            }
            return null;
        }

        private JObject GetJOjectResponse(string url)
        {
            var request = CreateXmlRequest(url);
            if(request != null)
            {
                using (var response = request.GetResponse())
                using(var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    var jsonString = reader.ReadToEnd();
                    return JObject.Parse(jsonString);
                }
            }
            return null;
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
            var request = CreateXmlRequest(url);
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
            var request = CreateXmlRequest(url);
            request.Method = "DELETE";
            request.GetResponse();
        }

        public void CreateAssociation(string spaceName, int parentNumber, int childNumber)
        {
            var url = String.Format("https://api.assembla.com/v1/spaces/{0}/tickets/{1}/ticket_associations.xml", spaceName, parentNumber);
            var request = CreateXmlRequest(url);
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
    }

}
