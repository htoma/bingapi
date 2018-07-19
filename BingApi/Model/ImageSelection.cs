// Copyright (c) Microsoft Corporation. All rights reserved.

namespace BingApi.Model
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ImageSelection
    {
        public string UserId { get; set; }
        public string UserInput { get; set; }
        public string KeywordMatched { get; set; }
        public string ImageDetails { get; set; }
    }
}
