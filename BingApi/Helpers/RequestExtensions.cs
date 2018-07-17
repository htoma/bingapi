using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;

namespace BingApi.Helpers
{
    public static class RequestExtensions
    {
        public const string AzureKeyIdPropertyName = "MS_AzureFunctionsKeyId";

        private static readonly bool IsDev = string.Equals(
            ConfigurationManager.AppSettings["env"],
            "dev",
            StringComparison.OrdinalIgnoreCase);

        public static bool IsKeyId(this HttpRequestMessage req, params string[] keyIds)
        {
            string keyIdValue = req.GetKeyId();
            return keyIds.Any(expected => IsMatchingKey(expected, keyIdValue)) || IsDev;
        }

        public static string GetKeyId(this HttpRequestMessage req)
        {
            return (string) req.Properties[AzureKeyIdPropertyName];
        }

        public static string GetQueryParameter(this HttpRequestMessage req, string name)
        {
            return req.GetQueryNameValuePairs()
                .FirstOrDefault(
                    q => string.Equals(q.Key, name, StringComparison.OrdinalIgnoreCase))
                .Value;
        }

        public static bool TryGetQueryParameter(this HttpRequestMessage req, string name, out string value)
        {
            value = req.GetQueryParameter(name);
            return value != null;
        }

        private static bool IsMatchingKey(string keyId, string keyIdValue)
        {
            return string.Equals(keyId, keyIdValue, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}