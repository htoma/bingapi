using BingApi.Functions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BingApi.Model
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ApiImage
    {
        public GifImage[] Value { get; set; }
    }
}
