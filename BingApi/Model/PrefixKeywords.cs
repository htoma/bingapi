// Copyright (c) Microsoft Corporation. All rights reserved.

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BingApi.Model
{    
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class PrefixKeywords
    {
        public string[] AllKeywords { get; set; }
        public string[] TextAnalyticsKeywords { get; set; }
        public bool LastWordAdded { get; set; }
    }
}
