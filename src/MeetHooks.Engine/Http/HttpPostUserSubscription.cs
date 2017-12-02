using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MeetHooks.Engine.Data.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace MeetHooks.Engine.Http
{
    public static class HttpPostUserSubscription
    {
        [FunctionName(nameof(HttpPostUserSubscription))]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users/{userId}/subscriptions")]
            HttpRequestMessage req,
            string userId,
            [Table(TableNames.GroupEventSubscription, Connection = "AzureWebJobsStorage")]
            ICollector<GroupEventSubscriptionEntity> outTable,
            TraceWriter log)
        {
            var json = await req.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<GroupEventSubscriptionModel>(json);
            

            if (model?.Type != "GroupEvent")
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, $"Subscription type '{model?.Type}' not supported");
            }
            
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.UrlName))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Name and UrlName are all required for creating a subscription");
            }

            if (!model.EndpointUrl.IsAbsoluteUri ||
                !new[] {Uri.UriSchemeHttp, Uri.UriSchemeHttps}.Contains(model.EndpointUrl.Scheme))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "A Valid http or https endpoint url is required for creating a subscription");
            }

            var id = Guid.NewGuid().ToString();
            outTable.Add(new GroupEventSubscriptionEntity()
            {
                PartitionKey = userId,
                RowKey = id,
                Name = model.Name,
                UrlName = model.UrlName,
                OnCreated = model.OnCreated,
                OnUpdated = model.OnUpdated,
                OnDeleted = model.OnDeleted,
                EndpointUrl = model.EndpointUrl
            });

            return req.CreateResponse(HttpStatusCode.Created, new {id});
        }

        public class GroupEventSubscriptionModel
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("urlName")]
            public string UrlName { get; set; }

            [JsonProperty("onCreated")]
            public bool OnCreated { get; set; }

            [JsonProperty("onUpdated")]
            public bool OnUpdated { get; set; }

            [JsonProperty("onDeleted")]
            public bool OnDeleted { get; set; }

            [JsonProperty("endpointUrl")]
            public Uri EndpointUrl { get; set; }
        }
    }
}
