using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using BingApi.Similarity;

namespace BingApi.Functions
{
    public static class Similarity
    {
        private static HttpClient Client = new HttpClient();

        [FunctionName("Similarity")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequestMessage req, TraceWriter log)
        {
            var w1 = req.GetQueryNameValuePairs().First(q => q.Key == "w1").Value;
            var w2 = req.GetQueryNameValuePairs().First(q => q.Key == "w2").Value;

            var orthogonal = await AreOrthogonal(w1, w2);
            var similar = await AreSimilar(w1, w2);
            var highSimilarityScore = await HighSimilarityScore(w1, w2);

            return req.CreateResponse(HttpStatusCode.OK,
                $"Similar: {similar}, Orthogonal: {orthogonal}, High score: {highSimilarityScore}");
        }

        public static async Task<double> HighSimilarityScore(string word1, string word2)
        {
            var scores = new[] { "wup", "lin", "jcn" }.Select(x => GetScore(Client, x, word1, word2))
                .ToArray();
            var result = (await Task.WhenAll(scores)).ToDictionary(x => x.Coef, x => x.Value);
            if (result.Any(x => x.Value == -1))
            {
                return 0;
            }

            return (result["wup"] + result["lin"]) / 2 * (result["jcn"] < 0.25 ? result["jcn"] : 1);
        }

        public static async Task<bool> AreSimilar(string word1, string word2)
        {
            var scores = new[] {"wup", "jcn", "lch", "lin", "res"}.Select(x => GetScore(Client, x, word1, word2))
                .ToArray();
            var result = (await Task.WhenAll(scores)).ToDictionary(x => x.Coef, x => x.Value);
            var sum = 0;
            if (result["wup"] >= 0.75)
            {
                sum++;
            }
            if (result["jcn"] >= 2.5)
            {
                sum++;
            }
            if (result["lch"] > 2)
            {
                sum++;
            }
            if (result["lin"] > 0.7)
            {
                sum++;
            }
            if (result["res"] > 5)
            {
                sum++;
            }

            return sum >= 4;
        }

        public static async Task<bool> AreOrthogonal(string word1, string word2)
        {
            var scores = new[] {"lesk", "hso"}.Select(x => GetScore(Client, x, word1, word2)).ToArray();
            var result = (await Task.WhenAll(scores)).ToDictionary(x => x.Coef, x => x.Value);
            return result["hso"] <= 2;
        }

        private static async Task<LocalResult> GetScore(HttpClient client, string coef, string word1, string word2)
        {
            try
            {
                var response =
                    await client.GetAsync($"http://ws4jdemo.appspot.com/ws4j?measure={coef}&args={word1}%3A%3A{word2}");
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<SimilarityResult>(content);
                return new LocalResult
                {
                    Coef = coef,
                    Value = result.SimilarityScores[0].Score
                };
            }
            catch (Exception ex)
            {
                return new LocalResult
                {
                    Coef = coef,
                    Value = 0
                };
            }
        }

        public class LocalResult
        {
            public string Coef { get; set; }
            public double Value { get; set; }
        }
    }
}
