using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MeetHooks.Engine.Data.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace MeetHooks.Engine.Http
{
    public static class HttpPutUser
    {
        [FunctionName(nameof(HttpPutUser))]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "users/{id}")]
            HttpRequestMessage req,
            string id,
            [Table(TableNames.User, Connection = "AzureWebJobsStorage")]
            CloudTable outTable,
            TraceWriter log)
        {
            var json = await req.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<UserModel>(json);
            var user = new UserEnity()
            {
                PartitionKey = id,
                RowKey = id,
                Name = model.Name,
                AccessToken = model.AccessToken,
                RefreshToken = model.RefreshToken,
                TokenExpiry= model.TokenExpiry
            };

            var updateOperation = TableOperation.InsertOrReplace(user);
            
            var result = outTable.Execute(updateOperation);

            return new HttpResponseMessage((HttpStatusCode)result.HttpStatusCode);
        }

        public class UserModel
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("accessToken")]
            public string AccessToken { get; set; }

            [JsonProperty("refreshToken")]
            public string RefreshToken { get; set; }

            [JsonProperty("tokenExpiry")]
            public DateTime TokenExpiry { get; set; }
        }

    }
}
