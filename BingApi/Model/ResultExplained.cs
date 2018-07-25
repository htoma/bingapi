// Copyright (c) Microsoft Corporation. All rights reserved.

using BingApi.DbModel;

namespace BingApi.Model
{
    public class ResultExplained
    {
        public string Prefix { get; set; }
        public string[] PrefixKeywords { get; set; }
        public string[] ProfileKeywords { get; set; }
        public string[] CombinedKeywords { get; set; }
        public GifImage[] Images { get; set; }
    }
}
