using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace MeetHooks.Portal.Commands
{
    public class EngineHttpClientFactory
    {
        private readonly HttpClientHandler _httpMessageHandler;
        private readonly string _functionKey;
        private readonly Uri _baseAddress;

        public EngineHttpClientFactory(HttpClientHandler httpMessageHandler, IConfiguration configuration)
        {
            _httpMessageHandler = httpMessageHandler;
            _functionKey = configuration["engine:functionkey"];
            _baseAddress = new Uri(configuration["engine:baseuri"]);
        }

        public HttpClient Create()
        {
            var functionAuthenticationHandler = new FunctionAuthenticationHandler(_functionKey)
            {
                InnerHandler = _httpMessageHandler
            };
            
            return new HttpClient(functionAuthenticationHandler, false)
            {
                BaseAddress = _baseAddress
            };
        }
    }
}
