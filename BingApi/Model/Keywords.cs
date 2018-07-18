// Copyright (c) Microsoft Corporation. All rights reserved.

namespace BingApi.Model
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Keywords
    {
        public string[] AllKeywords { get; set; }
        public string[] TextAnalyticsKeywords { get; set; }
        public bool LastWord { get; set; }
        public bool EntirePhrase { get; set; }
    }
}
