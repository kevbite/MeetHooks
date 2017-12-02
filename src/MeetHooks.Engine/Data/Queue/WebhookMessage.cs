using System;

namespace MeetHooks.Engine.Data.Queue
{
    public class WebhookMessage
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public string Status { get; set; }
            
        public Uri EndpointUrl { get; set; }
    }
}