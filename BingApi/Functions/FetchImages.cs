namespace BingApi.Functions
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using BingApi.APIs;
    using BingApi.Helpers;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;

    public static class FetchImages
    {
        private const string Mobile = "mobile";

        [FunctionName("FetchImages")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "images")]
            HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"Function processed a request from {req.GetKeyId()}");
            if (!req.IsKeyId(Mobile))
            {
                return req.CreateErrorResponse(HttpStatusCode.Forbidden, "Unauthorized");
            }

            string payload = await req.Content.ReadAsStringAsync();

            string[] keywords = await TextAnalyticsApi.GetKeywords(payload);

            GifImage[] images = await BingImageApi.GetImages(keywords, EBingImageSearchType.All);

            return req.CreateResponse(HttpStatusCode.OK, images);
        }
    }
}
