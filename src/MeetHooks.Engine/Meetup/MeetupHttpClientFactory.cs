using System.Net.Http;

namespace MeetHooks.Engine.Meetup
{
    public class MeetupHttpClientFactory
    {
        private readonly HttpClientHandler _httpMessageHandler;

        public MeetupHttpClientFactory(HttpClientHandler httpMessageHandler)
        {
            _httpMessageHandler = httpMessageHandler;
        }

        public HttpClient Create(string accessToken)
        {
            var handler = new MeetupAuthenticationHandler(accessToken);
            handler.InnerHandler = _httpMessageHandler;
            
            return new HttpClient(handler);
        }
    }
}