using System;
using System.Collections.Generic;
using System.Linq;
using MeetHooks.Engine.Data.Queue;
using MeetHooks.Engine.Data.Table;
using MeetHooks.Engine.Meetup;

namespace MeetHooks.Engine.Queue
{
    public class GroupEventsWebhookFactory
    {
        public List<WebhookMessage> Create(IReadOnlyList<GroupEventSubscriptionStateEntity> state,
            IReadOnlyList<GroupEvent> groupEvent, UserSubscriptionsGroupEventMessage message)
        {
            var messages = new List<WebhookMessage>();

            if (message.OnCreated)
            {
                var createdEvents = GetNewlyCreatedEvents(state, groupEvent);
                messages.AddRange(CreateMessages(createdEvents, "Created", message.EndpointUrl));
            }

            if (message.OnUpdated)
            {
                var updatedEvents = GetUpdatedEvents(state, groupEvent);
                messages.AddRange(CreateMessages(updatedEvents, "Updated", message.EndpointUrl));
            }

            if (message.OnDeleted)
            {
                var deletedEvents = GetDeletedStates(state, groupEvent);
                messages.AddRange(deletedEvents.Select(x => new WebhookMessage()
                {
                    Id = x.RowKey,
                    Updated = SystemDateTime.UtcNow,
                    Created = x.Created,
                    Status = "Deleted",
                    Type = "GroupEvent",
                    EndpointUrl = message.EndpointUrl
                }));
            }

            return messages;
        }

        private IEnumerable<GroupEventSubscriptionStateEntity> GetDeletedStates(IReadOnlyList<GroupEventSubscriptionStateEntity> state, IReadOnlyList<GroupEvent> groupEvent)
        {
            return state.Where(p => groupEvent.All(p2 => p2.Id != p.RowKey))
                .Where(x => !x.IsDeleted);

        }

        private IEnumerable<GroupEvent> GetUpdatedEvents(IReadOnlyList<GroupEventSubscriptionStateEntity> state, IReadOnlyList<GroupEvent> groupEvent)
        {
            return state.Join(groupEvent, x => x.RowKey, x => x.Id, (s, e) => new {Event = e, State = s})
                .Where(x => x.State.LastUpdated < x.Event.Updated)
                .Select(x => x.Event);
        }

        private IEnumerable<WebhookMessage> CreateMessages(IEnumerable<GroupEvent> createdEvents, string status, Uri endpointUrl)
        {
            return createdEvents.Select(x => new WebhookMessage()
            {
                Id = x.Id,
                Created = x.Created,
                Updated = x.Updated,
                Status = status,
                Type = "GroupEvent",
                EndpointUrl = endpointUrl
            });
        }

        private static IEnumerable<GroupEvent> GetNewlyCreatedEvents(IReadOnlyList<GroupEventSubscriptionStateEntity> state, IReadOnlyList<GroupEvent> groupEvent)
        {
            return groupEvent.Where(p => state.All(p2 => p2.RowKey != p.Id));
        }
    }
}