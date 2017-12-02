using System.Linq;
using System.Threading.Tasks;
using MeetHooks.Engine.Data.Queue;
using MeetHooks.Engine.Data.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace MeetHooks.Engine.Timer
{
    public static class TimerSubscriptions
    {
        [FunctionName("TimerSubscriptions")]
        public static async Task Run(
            [TimerTrigger("0 */1 * * * *")]
            TimerInfo myTimer,
            [Table(TableNames.User, Connection = "AzureWebJobsStorage")]
            IQueryable<GroupEventSubscriptionEntity> inTable,
            [Queue(QueueNames.SubscriptionFanout, Connection = "AzureWebJobsStorage")]
            IAsyncCollector<UserSubscriptionMessage> messages,
            TraceWriter log)
        {
            foreach (var user in inTable)
            {
                log.Info($"Adding user subscription message for user id {user.PartitionKey}");

                await messages.AddAsync(new UserSubscriptionMessage()
                {
                    Id = user.PartitionKey
                });
            }

            await messages.FlushAsync();
        }
    }
}
