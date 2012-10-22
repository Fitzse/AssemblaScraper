using System;
using System.Xml.Serialization;

namespace Assembla
{
    [Serializable]
    public class Ticket
    {
        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("number")]
        public int Number { get; set; }

        [XmlElement("summary")]
        public string Summary { get; set; }

        [XmlElement("reporter-id")]
        public int ReporterId { get; set; }

        [XmlElement("priority")]
        public int Priority { get; set; }

        [XmlElement("status")]
        public int Status { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }
    }
}
