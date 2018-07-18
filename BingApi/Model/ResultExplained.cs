// Copyright (c) Microsoft Corporation. All rights reserved.

namespace BingApi.Model
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ResultExplained
    {
        public Keywords Keywords { get; set; }
        public GifImage[] Images { get; set; }
    }
}
