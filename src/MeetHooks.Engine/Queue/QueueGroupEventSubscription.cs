using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MeetHooks.Engine.Data.Queue;
using MeetHooks.Engine.Data.Table;
using MeetHooks.Engine.Meetup;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;

namespace MeetHooks.Engine.Queue
{
    public static class QueueGroupEventSubscription
    {
        [FunctionName(nameof(QueueGroupEventSubscription))]
        public static async Task Run(
            [QueueTrigger(QueueNames.GroupEventSubscription, Connection = "AzureWebJobsStorage")]
            UserSubscriptionsGroupEventMessage message,
            [Table(TableNames.GroupEventSubscriptionState, Connection = "AzureWebJobsStorage")]
            CloudTable stateTable,
            TraceWriter log)
        {
            var states = stateTable.CreateQuery<GroupEventSubscriptionStateEntity>()
                    .Where(x => x.PartitionKey == message.SubscriptionId)
                    .ToList();
            
            var client = new GroupEventsClient(new MeetupHttpClientFactory(new HttpClientHandler()));

            var groupEvents = await client.GetGroupEventsAsync(message.UrlName, message.AccessToken);

            var webhooks = new GroupEventsWebhookFactory()
                .Create(states, groupEvents, message);

            var updatedStates = states.Join(webhooks, entity => entity.RowKey, webhook => webhook.Id,
                (entity, webhook) =>
                    new GroupEventSubscriptionStateEntity()
                    {
                        PartitionKey = message.SubscriptionId,
                        RowKey = entity.RowKey,
                        LastStatus = webhook.Status,
                        LastUpdated = webhook.Updated,
                        Created = entity.Created,
                        IsDeleted = entity.IsDeleted
                    });


            foreach (var entity in updatedStates)
            {
                var update = TableOperation.Replace(entity);

                var result = stateTable.Execute(update);

                if (!result.IsSuccessStatusCode())
                {
                    log.Error($"Unexpected status code of {result.HttpStatusCode} while updating state {entity.PartitionKey}, {entity.RowKey}");
                }
            }
        }
    }

    public static class TableResultExtentions
    {
        public static bool IsSuccessStatusCode(this TableResult tableResult)
        {
            var httpStatusCode = tableResult.HttpStatusCode;

            return httpStatusCode >= 200 && httpStatusCode <= 299;
        }
    }
}
