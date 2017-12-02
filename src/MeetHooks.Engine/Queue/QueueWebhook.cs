using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using MeetHooks.Engine.Data.Queue;

namespace MeetHooks.Engine.Queue
{
    public static class QueueWebhook
    {
        [FunctionName(nameof(QueueWebhook))]
        public static void Run(
            [QueueTrigger(QueueNames.Webhook, Connection = "AzureWebJobsStorage")]
            WebhookMessage myQueueItem,
            TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
