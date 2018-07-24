using Newtonsoft.Json;

namespace BingApi.Similarity
{
    public class SimilarityResult
    {
        [JsonProperty(PropertyName = "result")]
        public SimilarityScore[] SimilarityScores { get; set; }
    }
}