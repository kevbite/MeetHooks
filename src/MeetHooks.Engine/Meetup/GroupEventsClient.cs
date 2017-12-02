using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace MeetHooks.Engine.Meetup
{
    public class GroupEventsClient
    {
        private readonly MeetupHttpClientFactory _factory;

        public GroupEventsClient(MeetupHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task<GroupEvent[]> GetGroupEventsAsync(string urlName, string accessToken)
        {
            using (var client = _factory.Create(accessToken))
            {
                var response = await client.GetAsync($"/{urlName}/events?only=id,created,status,updated");

                response.EnsureSuccessStatusCode();

                var groupEvents = await response.Content.ReadAsAsync<GroupEvent[]>();

                return groupEvents;
            }
            
        }
    }
}
