using BingApi.DbModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BingApi.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ApiImage
    {
        public GifImage[] Value { get; set; }
    }
}
