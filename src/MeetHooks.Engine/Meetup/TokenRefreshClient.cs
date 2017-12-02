using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MeetHooks.Engine.Meetup
{
    public class TokenRefreshClient
    {
        private readonly HttpClientHandler _httpClientHandler;
        private readonly MeetupOAuthConfiguration _meetupOAuthConfiguration;

        public TokenRefreshClient(HttpClientHandler httpClientHandler, MeetupOAuthConfiguration meetupOAuthConfiguration)
        {
            _httpClientHandler = httpClientHandler;
            _meetupOAuthConfiguration = meetupOAuthConfiguration;
        }

        public async Task<TokenRefresh> RefreshTokenAsync(string refreshToken)
        {
            var httpClient = new HttpClient(_httpClientHandler)
            {
                BaseAddress = _meetupOAuthConfiguration.BaseUri
            };

            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "client_id", _meetupOAuthConfiguration.ClientId},
                { "client_secret", _meetupOAuthConfiguration.ClientSecret},
                { "grant_type", "refresh_token"},
                { "refresh_token", refreshToken},
            });

            var responseMessage = await httpClient.PostAsync("oauth2/access", content);

            responseMessage.EnsureSuccessStatusCode();

            return await responseMessage.Content.ReadAsAsync<TokenRefresh>();
        }
    }
}
