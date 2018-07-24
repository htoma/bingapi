using Newtonsoft.Json;

namespace BingApi.Similarity
{
    public class SimilarityScore
    {
        [JsonProperty(PropertyName = "score")]
        public double Score { get; set; }
    }
}