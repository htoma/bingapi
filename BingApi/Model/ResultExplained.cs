// Copyright (c) Microsoft Corporation. All rights reserved.
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BingApi.Model
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ResultExplained
    {
        public string Prefix { get; set; }
        public PrefixKeywords PrefixKeywords { get; set; }
        public UserProfile UserProfile { get; set; }
        public string[] ProfileKeywords { get; set; }
        public string[] CombinedKeywords { get; set; }
        public KeywordGifImages[] Images { get; set; }
    }
}
