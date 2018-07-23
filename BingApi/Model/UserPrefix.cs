using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BingApi.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserPrefix
    {
        public string UserId { get; set; }
        public string Prefix { get; set; } 
    }
}