using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MeetHooks.Portal.Commands
{
    public class UpsertUserCommandHandler
    {
        private readonly EngineHttpClientFactory _httpClientFactory;

        public UpsertUserCommandHandler(EngineHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task HandleAsync(UpsertUserCommand command)
        {
            using (var httpClient = _httpClientFactory.Create())
            {
                var json = JsonConvert.SerializeObject(new
                {
                    command.Name,
                    command.Country,
                    command.AccessToken,
                    command.RefreshToken,
                    command.TokenExpiry
                }, new JsonSerializerSettings(){ContractResolver = new CamelCasePropertyNamesContractResolver()});

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var responseMessage = await httpClient.PutAsync($"/users/{command.Id}", content);

                responseMessage.EnsureSuccessStatusCode();
            }
        }
    }
}