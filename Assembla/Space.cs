using System;
using System.Xml.Serialization;

namespace Assembla
{
    [Serializable]
    public class Space
    {
        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("wiki-name")]
        public string WikiName { get; set; }

        [XmlElement("wiki-format")]
        public string WikiFormat { get; set; }
    }
}