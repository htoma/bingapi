// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace BingApi.Helpers
{    
    public class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private string _key;

        public ApiKeyServiceClientCredentials(string key)
        {
            _key = key;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", _key);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
