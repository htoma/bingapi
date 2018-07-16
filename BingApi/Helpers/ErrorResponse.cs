using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BingApi.Helpers
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public sealed class ErrorResponse
    {
        public Error Error { get; set; }
    }
}