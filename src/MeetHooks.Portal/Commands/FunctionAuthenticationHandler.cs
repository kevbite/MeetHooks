using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MeetHooks.Portal.Commands
{
    public class FunctionAuthenticationHandler : DelegatingHandler
    {
        private readonly string _functionKey;

        public FunctionAuthenticationHandler(string functionKey)
        {
            _functionKey = functionKey;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("x-functions-key", _functionKey);

            return base.SendAsync(request, cancellationToken);
        }
    }
}