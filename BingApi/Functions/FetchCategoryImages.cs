using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BingApi.APIs;
using BingApi.DbModel;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using BingApi.Helpers;
using BingApi.Model;
using Newtonsoft.Json;
using System;

namespace BingApi.Functions
{
    public static class FetchCategoryImages
    {
        private const int MaxImages = 12;

        [FunctionName("FetchCategoryImages")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "categoryimages")]
            HttpRequestMessage req, ILogger log)
        {
            if (!req.IsKeyId(Constants.Mobile))
            {
                return req.CreateErrorResponse(HttpStatusCode.Forbidden, "Unauthorized");
            }

            string payload = await req.Content.ReadAsStringAsync();

            try
            {
                var categoryPrefix = JsonConvert.DeserializeObject<CategoryPrefix>(payload);

                string[] prefixKeywords = await TextAnalyticsApi.GetKeywords(categoryPrefix.Prefix);
                string[] categoryKeywords = prefixKeywords.Select(x => $"{categoryPrefix.Category} {x}").ToArray();

                if (!int.TryParse(req.GetQueryParameter("totalImages"), out int totalImages))
                {
                    totalImages = MaxImages;
                }

                GifImage[] images =
                    await BingImageApi.GetImages(categoryKeywords, totalImages, 0);

                return req.CreateResponse(HttpStatusCode.OK, new CategoryImagesExplained
                {
                    Prefix = categoryPrefix.Prefix,
                    PrefixKeywords = prefixKeywords,
                    Category = categoryPrefix.Category,
                    Images = images
                });
            }
            catch (Exception ex)
            {
                var msg = $"Input: {payload}, error: {ex.Message}";
                log.LogError(msg);
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
