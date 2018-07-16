using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BingApi.Functions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace BingApi
{
    public static class FetchImages
    {
        private const string Mobile = "mobile";

        [FunctionName("FetchImages")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "images")]
            HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"C# HTTP trigger function processed a request from {req.GetKeyId()}");
            if (!req.IsKeyId(Mobile))
            {
                return req.CreateErrorResponse(HttpStatusCode.Forbidden, "Unauthorized");
            }

            return req.CreateResponse(HttpStatusCode.OK, new[]
            {
                new Image
                {
                    Url = "https://a"
                },
                new Image
                {
                    Url = "https://b"
                }
            });
        }
    }
}
