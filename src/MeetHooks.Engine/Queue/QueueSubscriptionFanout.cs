using System;
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
    public static class QueueSubscriptionFanout
    {
        [FunctionName(nameof(QueueSubscriptionFanout))]
        public static async Task Run(
            [QueueTrigger(QueueNames.SubscriptionFanout, Connection = "AzureWebJobsStorage")]
            UserSubscriptionMessage message,
            [Table(TableNames.User, Connection = "AzureWebJobsStorage")]
            CloudTable userTable,
            [Table(TableNames.GroupEventSubscription, Connection = "AzureWebJobsStorage")]
            IQueryable<GroupEventSubscriptionEntity> subscriptions,
            [Queue(QueueNames.GroupEventSubscription, Connection = "AzureWebJobsStorage")]
            IAsyncCollector<UserSubscriptionsGroupEventMessage> subscriptionsQueue,
            TraceWriter log)
        {
            log.Info($"Processing user subscription message for user id {message.Id}");

            var user = userTable.CreateQuery<UserEnity>()
                            .First(x => x.PartitionKey == message.Id && x.RowKey == message.Id);
            
            if (user.TokenExpiry == DateTime.MinValue || user.TokenExpiry.AddMinutes(-5) <= DateTime.UtcNow)
            {
                var tokenRefreshClient = new TokenRefreshClient(new HttpClientHandler(), new MeetupOAuthConfiguration());
                var refreshToken = await tokenRefreshClient.RefreshTokenAsync(user.RefreshToken);

                user.AccessToken = refreshToken.AccessToken;
                user.RefreshToken = refreshToken.RefreshToken;
                user.TokenExpiry = DateTime.UtcNow.AddSeconds(refreshToken.ExpiresIn);
                user.ETag = "*";
                var mergeOperation = TableOperation.Replace(user);
                await userTable.ExecuteAsync(mergeOperation);
            }

            var userSubscriptions = subscriptions.Where(x => x.PartitionKey == user.PartitionKey);

            foreach (var subscription in userSubscriptions)
            {
                await subscriptionsQueue.AddAsync(new UserSubscriptionsGroupEventMessage()
                {
                    UserId = user.PartitionKey,
                    AccessToken = user.AccessToken,
                    SubscriptionId = subscription.RowKey,
                    UrlName = subscription.UrlName,
                    OnCreated = subscription.OnCreated,
                    OnUpdated = subscription.OnUpdated,
                    OnDeleted = subscription.OnDeleted,
                    EndpointUrl = subscription.EndpointUrl
                });
            }

            await subscriptionsQueue.FlushAsync();
        }
    }
}
