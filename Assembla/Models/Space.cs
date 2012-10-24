using System;
using System.Data;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Linq;

namespace Assembla.Models
{
    [Serializable]
    [XmlRoot("space")]
    public class Space
    {
        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("wiki-name")]
        public string WikiName { get; set; }

        [XmlElement("wiki-format")]
        public string WikiFormat { get; set; }

        public static Space FromElement(XElement element)
        {
            var space = new Space();
            SetProperties(element, space);
            return space;
        }

        private static void SetProperties(XElement rootElement, Space space)
        {
            var properties = space.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var attribute = prop.CustomAttributes.First(x => x.AttributeType == typeof (XmlElementAttribute));
                if (attribute != null)
                {
                    var elementName = attribute.ConstructorArguments.First().Value as string;
                    var element = rootElement.Element(elementName);
                    if (element != null)
                    {
                        if (element.Value.GetType() == prop.PropertyType)
                        {
                            prop.SetValue(space, element.Value);
                        }
                        else
                        {
                            throw new StrongTypingException();
                        }
                    }
                }
            }
        }
    }
}