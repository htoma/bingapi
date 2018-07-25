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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BingApi.Functions
{
    public static class FetchImages
    {
        private const int MaxImages = 12;
        private const int MaxKeywordsPerImage = 10;

        [FunctionName("FetchImages")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "images")]
            HttpRequestMessage req, ILogger log)
        {
            if (!req.IsKeyId(Constants.Mobile))
            {
                return req.CreateErrorResponse(HttpStatusCode.Forbidden, "Unauthorized");
            }

            string payload = await req.Content.ReadAsStringAsync();

            try
            {
                var userPrefix = JsonConvert.DeserializeObject<UserPrefix>(payload);

                // get  keywords for prefix using Text Analytics
                string[] prefixKeywords = await TextAnalyticsApi.GetKeywords(userPrefix.Prefix);
                
                // get keywords from user profile
                var profile = await ProfileHelper.GetUserProfile(userPrefix.UserId);
                var profileKeywords = profile.Keywords;

                var combinedKeywords = await CombineKeywords(prefixKeywords, profileKeywords);

                if (!int.TryParse(req.GetQueryParameter("totalImages"), out int totalImages))
                {
                    totalImages = MaxImages;
                }

                bool useSimilarity = true;
                if (!int.TryParse(req.GetQueryParameter("imageKeywords"), out int imageKeywords))
                {
                    imageKeywords = MaxKeywordsPerImage;
                    useSimilarity = false;
                }

                GifImage[] images =
                    await BingImageApi.GetImages(combinedKeywords, totalImages, imageKeywords);
                GifImage[] orderedImages = await OrderImages(images, profileKeywords, useSimilarity);

                return req.CreateResponse(HttpStatusCode.OK, new FetchImagesExplained
                {
                    Prefix = userPrefix.Prefix,
                    PrefixKeywords = prefixKeywords,
                    ProfileKeywords = profileKeywords,
                    CombinedKeywords = combinedKeywords,
                    Images = orderedImages
                });
            }
            catch (Exception ex)
            {
                var msg = $"Input: {payload}, error: {ex.Message}";
                log.LogError(msg);
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private static async Task<GifImage[]> OrderImages(GifImage[] images, string[] profileKeywords,
            bool useHighSimilarity)
        {
            if (profileKeywords.Length == 0)
            {
                return images;
            }

            foreach (var image in images)
            {
                double score = await GetImageScore(image, profileKeywords, useHighSimilarity);
                image.Score = score;
            }

            return images.OrderByDescending(x => x.Score).ToArray();
        }

        private static bool ImageNameMatchesKeywords(GifImage image, string[] keywords)
        {
            if (string.IsNullOrEmpty(image.Name))
            {
                return false;
            }
            return keywords.Any(x => image.Name.Contains(x));
        }

        private static async Task<double> GetImageScore(GifImage image, string[] profileKeywords,
            bool useHighSimilarity)
        {
            if (image.Keywords.Any(profileKeywords.Contains) || ImageNameMatchesKeywords(image, profileKeywords))
            {
                return 1;
            }

            if (!useHighSimilarity)
            {
                return 0;
            }
            
            double max = 0;
            foreach (var word in image.Keywords)
            {
                foreach (var keyword in profileKeywords)
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
