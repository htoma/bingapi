using System;
using System.Collections.Generic;
using System.Linq;
using BingApi.Model;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BingApi.APIs;
using BingApi.DbModel;
using BingApi.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace BingApi.Functions
{
    public static class FetchImages
    {
        [FunctionName("FetchImages")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "images")]
            HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"Get images request from {req.GetKeyId()}");

            if (!req.IsKeyId(Constants.Mobile))
            {
                return req.CreateErrorResponse(HttpStatusCode.Forbidden, "Unauthorized");
            }

            string payload = await req.Content.ReadAsStringAsync();

            try
            {
                var userPrefix = JsonConvert.DeserializeObject<UserPrefix>(payload);

                // get  keywords for prefix using Text Analytics
                PrefixKeywords prefixKeywords = await TextAnalyticsApi.GetKeywords(userPrefix.Prefix);
                
                // get keywords from user profile
                var profile = await ProfileHelper.GetUserProfile(userPrefix.UserId);
                var profileKeywords = profile.Keywords;

                var combinedKeywords = await CombineKeywords(prefixKeywords.AllKeywords, profileKeywords);

                if (!int.TryParse(req.GetQueryParameter("totalImages"), out int totalImages))
                {
                    totalImages = 6;
                }

                GifImage[] images =
                    await BingImageApi.GetImages(combinedKeywords, totalImages / combinedKeywords.Length);
                GifImage[] orderedImages = await OrderImages(images, prefixKeywords.AllKeywords);

                return req.CreateResponse(HttpStatusCode.OK, new ResultExplained
                {
                    Prefix = userPrefix.Prefix,
                    PrefixKeywords = prefixKeywords,
                    UserProfile = profile,
                    ProfileKeywords = profileKeywords,
                    CombinedKeywords = combinedKeywords,
                    Images = orderedImages
                });
            }
            catch (Exception ex)
            {
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private static async Task<GifImage[]> OrderImages(GifImage[] images, string[] prefixKeywords)
        {
            //foreach (var image in images)
            //{
            //    double score = await GetImageScore(image, prefixKeywords);
            //    image.Score = score;
            //}

            return images.OrderByDescending(x => x.Score).ToArray();
        }

        private static async Task<double> GetImageScore(GifImage image, string[] prefixKeywords)
        {
            if (image.Keywords.Any(prefixKeywords.Contains))
            {
                return 1;
            }
            
            double max = 0;
            foreach (var word in image.Keywords)
            {
                foreach (var keyword in prefixKeywords)
                {
                    double score = await Similarity.HighSimilarityScore(word, keyword);
                    if (score == 1)
                    {
                        return 1;
                    }

                    if (max < score)
                    {
                        max = score;
                    }
                }
            }

            return max;
        }

        private static async Task<string[]> CombineKeywords(string[] prefixKeywords, string[] profileKeywords)
        {
            // find orthogonal combinations, if none then use only keywords from prefix
            if (profileKeywords.Length == 0)
            {
                return prefixKeywords;
            }
            var result = new List<string>();
            foreach (var prefixKeyboard in prefixKeywords)
            {
                foreach (var profileKeyword in profileKeywords.Where(x => !prefixKeyboard.Contains(x)).ToList())
                {
                    if (await Similarity.AreOrthogonal(prefixKeyboard, profileKeyword))
                    {
                        result.Add($"{profileKeyword} {prefixKeyboard}");
                    }
                }
            }

            return result.ToArray();
        }
    }
}
