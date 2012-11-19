using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

        public void ResetTicketNumber(int number)
        {
            _ticketNumber = number;
        }

        private HttpWebRequest CreateJsonRequest(String url, string method = "GET")
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request != null)
            {
                request.Method = method;
                request.MaximumAutomaticRedirections = 1;
                request.Headers.Add("X-Api-Key", _key); 
                request.Headers.Add("X-Api-Secret", _secret);
                request.AllowAutoRedirect = true;

                request.Accept = "application/json";
                request.ContentType = "application/json";
            }
            return request;
        }

        public IEnumerable<TicketAssociation> GetAssociations(String spaceName, Ticket ticket)
        {
            var url = String.Format("https://api.assembla.com/v1/spaces/{0}/tickets/{1}/ticket_associations.json", spaceName, ticket.Number);
            var json = GetJArrayResponse(url);
            return json.ToObject<IEnumerable<TicketAssociation>>();
        }

        public IEnumerable<Ticket> GetTicketsForSpace(string spaceName)
        {
            var url = String.Format("https://api.assembla.com/v1/spaces/{0}/tickets.json", spaceName);
            var json = GetJArrayResponse(url);
            var tickets = json.ToObject<IEnumerable<Ticket>>().ToList();
            foreach (var ticket in tickets)
            {
                var associations = GetAssociations(spaceName, ticket);
                var parentAssoc = associations.FirstOrDefault(x => x.Ticket1Id == ticket.Id);
                if(parentAssoc != null)
                {
                    ticket.ParentId = parentAssoc.Ticket2Id;
                }
            }
            return tickets.Where(x => x.ParentId == 0).Select(x => GetHierarhcy(tickets, x));
        }

        private Ticket GetHierarhcy(IEnumerable<Ticket> flatList, Ticket ticket)
        {
            flatList = flatList.ToList();
            var children = flatList.Where(x => x.ParentId == ticket.Id).ToList();
            ticket.Children = children.Select(x => GetHierarhcy(flatList, x));
            return ticket;
        }

        private JArray GetJArrayResponse(string url)
        {
            var request = CreateJsonRequest(url);
            if(request != null)
            {
                using (var response = request.GetResponse())
                using(var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    var jsonString = reader.ReadToEnd();
                    if(!String.IsNullOrWhiteSpace(jsonString))
                    {
                        return JArray.Parse(jsonString);
                    }
                }
            }
            return new JArray();
        }

        private JObject GetPostResponse(string url, object toSend)
        {
            var request = CreateJsonRequest(url, "POST");
            using(var requestStream = request.GetRequestStream())
            {
                var serializedOject = JsonConvert.SerializeObject(toSend);
                var jsonRequest = Encoding.UTF8.GetBytes(serializedOject);
                requestStream.Write(jsonRequest, 0, jsonRequest.Length);
                using (var response = request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    var jsonResponse = reader.ReadToEnd();
                    return JObject.Parse(jsonResponse);
                }
            }
        }

        public int CreateTicket(string spaceName, Ticket ticket)
        {
            var children = ticket.Children.ToList();
            ticket.Number = _ticketNumber++;

            foreach (var t in children)
            {
                t.Id = CreateTicket(spaceName, t);
            }

            var url = String.Format("https://api.assembla.com/v1/spaces/{0}/tickets.json", spaceName);
            var jsonResponse = GetPostResponse(url, ticket.ToObject());
            ticket.Id = jsonResponse["id"].Value<int>();

            foreach (var child in children)
            {
                CreateAssociation(spaceName, ticket.Number, child.Id);
            }
            return ticket.Id;
        }

        public void DeleteTicket(string spaceName, int ticketNumber)
        {
            var url = String.Format("https://api.assembla.com/v1/spaces/{0}/tickets/{1}", spaceName, ticketNumber);
            var request = CreateJsonRequest(url, "DELETE");
            request.GetResponse();
        }

        public void CreateAssociation(string space, int parentNumber, int childId)
        {
            var url = String.Format("https://api.assembla.com/v1/spaces/{0}/tickets/{1}/ticket_associations.json", space, parentNumber);
            var association = new {ticket_association = new {ticket2_id = childId, relationship = "1"}};
            GetPostResponse(url, association);
        }
    }
}
