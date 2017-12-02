using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using MeetHooks.Engine.Data.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace MeetHooks.Engine.Http
{
    public static class HttpGetUserSubscriptions
    {
        [FunctionName(nameof(HttpGetUserSubscriptions))]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{userId}/subscriptions")]
            HttpRequestMessage req,
            string userId,
            [Table(TableNames.GroupEventSubscription, "{userId}", Connection = "AzureWebJobsStorage")]
            IQueryable<GroupEventSubscriptionEntity> inTable,
            TraceWriter log)
        {
            var model = new SubscriptionsModel
            {
                Subscriptions = inTable.Where(x => x.PartitionKey == userId).AsEnumerable().Select(CreateGroupEventModel).ToArray()
            };

            var json = JsonConvert.SerializeObject(model);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

        }

        private static SubscriptionModel CreateGroupEventModel(GroupEventSubscriptionEntity groupEventSubscriptionEntity)
        {
            return new GroupEventSubscriptionModel()
            {
                Id = new Guid(groupEventSubscriptionEntity.RowKey),
                Name = groupEventSubscriptionEntity.Name,
                OnCreated = groupEventSubscriptionEntity.OnCreated,
                OnUpdated = groupEventSubscriptionEntity.OnUpdated,
                OnDeleted = groupEventSubscriptionEntity.OnDeleted,
                UrlName = groupEventSubscriptionEntity.UrlName,
                EndpointUrl = groupEventSubscriptionEntity.EndpointUrl
            };
        }

        public class SubscriptionsModel
        {
            [JsonProperty("subscriptions")]
            public SubscriptionModel[] Subscriptions { get; set; }
        }

        public abstract class SubscriptionModel
        {
            [JsonProperty("id")]
            public Guid Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public abstract string Type { get; }

            [JsonProperty("endpoint")]
            public Uri EndpointUrl { get; set; }
        }

        public class GroupEventSubscriptionModel : SubscriptionModel
        {
            [JsonProperty("urlName")]
            public string UrlName { get; set; }


            [JsonProperty("onCreated")]
            public bool OnCreated { get; set; }

            [JsonProperty("onUpdated")]
            public bool OnUpdated { get; set; }

            [JsonProperty("onDeleted")]
            public bool OnDeleted { get; set; }

            [JsonProperty("type")]
            public override string Type { get; } = "GroupEvent";
        }
    }
}
