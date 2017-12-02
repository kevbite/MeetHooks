using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace MeetHooks.Engine.Data.Table
{
    public class GroupEventSubscriptionEntity : TableEntity
    {
        public string Name { get; set; }

        public string UrlName { get; set; }

        public bool OnCreated { get; set; }

        public bool OnUpdated { get; set; }

        public bool OnDeleted { get; set; }

        public Uri EndpointUrl { get; set; }
    }
}