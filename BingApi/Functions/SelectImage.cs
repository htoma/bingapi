using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using BingApi.Helpers;
using BingApi.Model;
using Newtonsoft.Json;

namespace BingApi.Functions
{
    public static class SelectImage
    {
        [FunctionName("SelectImage")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "selections")]
            HttpRequestMessage req,
            TraceWriter log)
        {
            log.Info($"Select image request from {req.GetKeyId()}");

            if (!req.IsKeyId(Constants.Mobile))
            {
                return req.CreateErrorResponse(HttpStatusCode.Forbidden, "Unauthorized");
            }

            var content = await req.Content.ReadAsStringAsync();
            var imageSelection = JsonConvert.DeserializeObject<ImageSelection>(content);

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
