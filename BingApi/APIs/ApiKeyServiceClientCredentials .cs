using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace BingApi.APIs
{    
    public class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private readonly string _key;

        public ApiKeyServiceClientCredentials(string key)
        {
            _key = key;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add(ApiHeaders.HeaderKeyName, _key);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
