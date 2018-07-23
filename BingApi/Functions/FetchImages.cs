using System;
using System.Linq;
using BingApi.Model;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BingApi.APIs;
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
                var profileKeywords = ProfileHelper.GetKeywordsFromProfile(profile);

                var combinedKeywords = CombineKeywords(prefixKeywords, profileKeywords);

                if (!int.TryParse(req.GetQueryParameter("maxImagesPerKeyword"), out int maxImagesPerKeyword))
                {
                    maxImagesPerKeyword = 3;
                }

                KeywordGifImages[] images = await BingImageApi.GetImages(combinedKeywords, maxImagesPerKeyword);

                return req.CreateResponse(HttpStatusCode.OK, new ResultExplained
                {
                    Prefix = userPrefix.Prefix,
                    PrefixKeywords = prefixKeywords,
                    UserProfile = profile,
                    ProfileKeywords = profileKeywords,
                    CombinedKeywords = combinedKeywords,
                    Images = images
                });
            }
            catch (Exception ex)
            {
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private static string[] CombineKeywords(PrefixKeywords keywords, string[] profileKeywords)
        {
            // do we take everything? all keywords from user input and all keywords from users' profile?
            return keywords.AllKeywords.Concat(profileKeywords).Distinct().ToArray();
        }
    }
}
