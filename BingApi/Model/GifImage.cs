using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BingApi.Functions
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class GifImage
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string AccentColor { get; set; }
    }
}