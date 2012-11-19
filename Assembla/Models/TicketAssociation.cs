using Newtonsoft.Json;

namespace Assembla.Models
{
    [JsonObject]
    public class TicketAssociation
    {
        [JsonProperty(PropertyName = "ticket1_id")]
        public int Ticket1Id { get; set; }

        [JsonProperty(PropertyName = "ticket2_id")]
        public int Ticket2Id { get; set; }

        [JsonProperty(PropertyName = "relationship")]
        public AssociationType Type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
    }

    public enum AssociationType
    {
        Parent=0,
        Child=1,
        Related=2,
        Duplicate=3
    }
}
