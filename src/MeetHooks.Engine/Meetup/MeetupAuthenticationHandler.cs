using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MeetHooks.Engine.Meetup
{
    public class MeetupAuthenticationHandler : DelegatingHandler
    {
        private readonly string _accessToken;

        public MeetupAuthenticationHandler(string accessToken)
        {
            _accessToken = accessToken;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return base.SendAsync(request, cancellationToken);
        }
    }
}