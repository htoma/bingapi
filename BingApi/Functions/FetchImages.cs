using System;
using BingApi.Model;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BingApi.APIs;
using BingApi.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace BingApi.Functions
{
    public static class FetchImages
    {        
        [FunctionName("FetchImages")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "images")]
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
            catch (Exception ex)
            {
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
