using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BingApi.DbModel
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class GifSelection : IUserDocument
    {
        public string UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }
    }
}