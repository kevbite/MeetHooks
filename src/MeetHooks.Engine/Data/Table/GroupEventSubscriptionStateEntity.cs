using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace MeetHooks.Engine.Data.Table
{
    public class GroupEventSubscriptionStateEntity : TableEntity
    {
        public DateTime Created { get; set; }

        public DateTime LastUpdated { get; set; }

        public string LastStatus { get; set; }

        public bool IsDeleted { get; set; }
    }
}
