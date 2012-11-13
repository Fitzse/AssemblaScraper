using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Assembla.Models
{
    [JsonObject]
    public class Ticket
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "number")]
        public int Number { get; set; }
        
        [JsonProperty(PropertyName = "summary")]
        public string Summary { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        public String Actor { get; set; }

        public IEnumerable<Ticket> Children { get; set; }

        public Ticket()
        {
            Children = Enumerable.Empty<Ticket>();
        }

        public Object ToObject()
        {
            return
                new
                    {
                        ticket =
                            new
                                {
                                    summary = Summary,
                                    description = Description,
                                    number = Number,
                                    custom_fields = new {Actor}
                                }
                    };
        }
    }
}
