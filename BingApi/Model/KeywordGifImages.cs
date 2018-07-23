using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BingApi.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class KeywordGifImages
    {
        public string Keyword { get; set; }
        public GifImage[] Images { get; set; }
    }
}