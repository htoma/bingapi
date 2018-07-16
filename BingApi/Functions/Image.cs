using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BingApi.Functions
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class Image
    {
        public string Url { get; set; }
    }
}