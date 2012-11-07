using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Assembla.Models
{
    [Serializable]
    [XmlRoot("ticket")]
    public class Ticket
    {
        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("number")]
        public int Number { get; set; }

        [XmlElement("summary")]
        public string Summary { get; set; }

        //[XmlElement("reporter-id")]
        //public string ReporterId { get; set; }

        //[XmlElement("priority")]
        //public int Priority { get; set; }

        //[XmlElement("status")]
        //public int Status { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlIgnore]
        public int? ParentNumber { get; set; }

        [XmlIgnore]
        public IEnumerable<Ticket> Children { get; set; }

        public Ticket()
        {
            Children = Enumerable.Empty<Ticket>();
        }

        public static Ticket FromElement(XElement element)
        {
            var ticket = new Ticket();
            SetProperties(element, ticket);
            return ticket;
        }

        public XDocument ToXDocument()
        {
            return new XDocument(new XElement("ticket",
                new XElement("summary",Summary),
                new XElement("description", Description),
                new XElement("number", Number, new XAttribute("type", "integer"))));
        }

        private static void SetProperties(XElement rootElement, Ticket ticket)
        {
            var properties = ticket.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var attribute = prop.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof (XmlElementAttribute));
                if (attribute != null)
                {
                    var elementName = attribute.ConstructorArguments.First().Value as string;
                    var element = rootElement.Element(elementName);
                    if (element != null)
                    {
                        var elementType = element.Attribute("type");
                        if(elementType != null && GetTypeFromName(elementType.Value) == prop.PropertyType)
                        {
                            if (prop.PropertyType == typeof (int))
                            {
                                prop.SetValue(ticket, Convert.ToInt32(element.Value));
                            }
                            else if (prop.PropertyType == typeof (bool))
                            {
                                prop.SetValue(ticket, Convert.ToBoolean(element.Value));
                            }
                            else if (prop.PropertyType == typeof (DateTime))
                            {
                                prop.SetValue(ticket, Convert.ToDateTime(element.Value));
                            }
                        }
                        else if(elementType == null && prop.PropertyType == typeof(string)){
                            prop.SetValue(ticket, element.Value);
                        }
                        else
                        {
                            throw new StrongTypingException(elementName);
                        }
                    }
                }
            }
        }

        private static Type GetTypeFromName(string name)
        {
            switch (name)
            {
                case "integer":
                    return typeof (int);
                case "boolean":
                    return typeof (bool);
                case "datetime":
                    return typeof (DateTime);
                default:
                    return typeof(string);
            }
        }
    }
}
