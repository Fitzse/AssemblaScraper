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
        private readonly NetworkCredential _credentials;

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
                        var elements = xDoc.Descendants("ticket");
                        return xDoc.Descendants("ticket").Select(Ticket.FromElement);
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

        class SpaceNotFoundException : Exception
        {
            public SpaceNotFoundException(string message) : base(message)
            {
            }
        }
    }
}
