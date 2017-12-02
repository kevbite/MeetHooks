using System;
using Newtonsoft.Json;

namespace MeetHooks.Engine.Meetup
{
    public class GroupEvent
    {
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("updated")]
        public DateTime Updated { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}