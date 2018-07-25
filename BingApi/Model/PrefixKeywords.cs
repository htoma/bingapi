// Copyright (c) Microsoft Corporation. All rights reserved.

namespace BingApi.Model
{    
    public class PrefixKeywords
    {
        public string[] AllKeywords { get; set; }
        public string[] TextAnalyticsKeywords { get; set; }
        public bool LastWordAddedToKeywords { get; set; }
    }
}
