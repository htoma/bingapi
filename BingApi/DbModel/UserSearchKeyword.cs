using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BingApi.DbModel
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserSearchKeyword
    {
        public string UserId { get; set; }
        public string Keyword { get; set; }
        public DateTime Timestamp { get; set; }
    }
}