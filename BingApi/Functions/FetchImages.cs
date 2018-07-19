using BingApi.Model;

namespace BingApi.Functions
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using APIs;
    using Helpers;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;

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

            Keywords keywords = await TextAnalyticsApi.GetKeywords(payload);

            if (!int.TryParse(req.GetQueryParameter("maxImagesPerKeyword"), out int maxImagesPerKeyword))
            {
                maxImagesPerKeyword = 3;
            }

            GifImage[] images = await BingImageApi.GetImages(keywords.AllKeywords, maxImagesPerKeyword);

            return req.CreateResponse(HttpStatusCode.OK, new ResultExplained
                {
                    Keywords = keywords,
                    Images = images
                });
        }
    }
}
