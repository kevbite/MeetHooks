using System;
using System.Linq;
using FluentAssertions;
using MeetHooks.Engine.Data.Queue;
using MeetHooks.Engine.Data.Table;
using MeetHooks.Engine.Meetup;
using MeetHooks.Engine.Queue;
using Ploeh.AutoFixture;
using Xunit;

namespace MeetHooks.Engine.Tests.Queue
{
    public class GroupEventsWebhookFactoryTests : IDisposable
    {
        private readonly GroupEventsWebhookFactory _factory;
        private readonly Fixture _fixture;
        private readonly DateTime _now;

        public GroupEventsWebhookFactoryTests()
        {
            _now = DateTime.UtcNow;
            SystemDateTime.Set(_now);
            _factory = new GroupEventsWebhookFactory();
            _fixture = new Fixture();
            _fixture.Customizations.Add(new UtcRandomDateTimeSequenceGenerator());
        }

        [Fact]
        public void ShouldReturnNothingWithNoGroupEvents()
        {
            var states = new GroupEventSubscriptionStateEntity[]{};
            var readOnlyList = new GroupEvent[]{};

            var messages = _factory.Create(states, readOnlyList, true, true, true);

            messages.Should().BeEmpty();
        }

        [Fact]
        public void ShouldReturnNothingWithAllOptionsOff()
        {
            var groupEvents = _fixture.Build<GroupEvent>().CreateMany(3).ToArray();

            var states = groupEvents.Take(2).Select(e =>
                new GroupEventSubscriptionStateEntity()
                {
                    RowKey = e.Id,
                    Created = e.Created,
                    LastUpdated = e.Updated,
                    LastStatus = e.Status
                }).ToArray();

            var messages = _factory.Create(states, groupEvents, false, false, false);

            messages.Should().BeEmpty();
        }

        [Fact]
        public void ShouldReturnCreatedWebhookMessage()
        {
            var groupEvents = _fixture.Build<GroupEvent>()
                                        .CreateMany(2)
                                        .ToArray();

            var messages = _factory.Create(new GroupEventSubscriptionStateEntity[]{}, groupEvents, true, true, true);

            messages.ShouldBeEquivalentTo(groupEvents.Select(x => new WebhookMessage()
            {
                Id = x.Id,
                Type = "GroupEvent",
                Created = x.Created,
                Updated = x.Updated,
                Status = "Created"
            }));
        }

        [Fact]
        public void ShouldReturnUpdatedWebhookMessage()
        {
            var groupEvents = _fixture.Build<GroupEvent>()
                .CreateMany(2)
                .ToArray();

            var state = groupEvents.Select(x => new GroupEventSubscriptionStateEntity
            {
                RowKey = x.Id,
                Created = x.Created,
                LastUpdated = x.Updated.AddHours(-2),
                LastStatus = x.Status
            }).ToArray();

            var messages = _factory.Create(state, groupEvents, true, true, true);

            messages.ShouldBeEquivalentTo(groupEvents.Select(x => new WebhookMessage()
            {
                Id = x.Id,
                Type = "GroupEvent",
                Created = x.Created,
                Updated = x.Updated,
                Status = "Updated"
            }));
        }

        [Fact]
        public void ShouldReturnDeletedWebhookMessage()
        {
            var state = _fixture.Build<GroupEventSubscriptionStateEntity>()
                .With(x => x.IsDeleted, false)
                .CreateMany(2)
                .ToArray();

            var messages = _factory.Create(state, new GroupEvent[0], true, true, true);

            messages.ShouldBeEquivalentTo(state.Select(x => new WebhookMessage()
            {
                Id = x.RowKey,
                Type = "GroupEvent",
                Created = x.Created,
                Updated = _now,
                Status = "Deleted"
            }));
        }

        [Fact]
        public void ShouldNotReturnDeletedWebhookMessageForDeletedState()
        {
            var state = _fixture.Build<GroupEventSubscriptionStateEntity>()
                .With(x => x.IsDeleted, true)
                .CreateMany(2)
                .ToArray();

            var messages = _factory.Create(state, new GroupEvent[0], true, true, true);

            messages.Should().BeEmpty();
        }

        public void Dispose()
        {
            SystemDateTime.Reset();
        }
    }
}
