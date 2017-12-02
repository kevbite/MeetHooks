using System;

namespace MeetHooks.Engine.Data.Queue
{
    public class UserSubscriptionsGroupEventMessage
    {
        public string UserId { get; set; }

        public string AccessToken { get; set; }

        public string SubscriptionId { get; set; }

        public string UrlName { get; set; }

        public bool OnCreated { get; set; }

        public bool OnUpdated { get; set; }

        public bool OnDeleted { get; set; }

        public Uri EndpointUrl { get; set; }
    }
}
