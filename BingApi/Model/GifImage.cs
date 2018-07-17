using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BingApi.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class GifImage
    {
        public string ContentUrl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string AccentColor { get; set; }
    }
}