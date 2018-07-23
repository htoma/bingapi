using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BingApi.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace BingApi.Functions
{
    public static class GetUserProfile
    {
        [FunctionName("GetUserProfile")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "profiles/{userId}")]
            HttpRequestMessage req, 
            string userId,
            TraceWriter log)
        {
            try
            {
                var profile = await ProfileHelper.GetUserProfile(userId);
                return req.CreateResponse(HttpStatusCode.OK, profile);
            }
            catch (Exception ex)
            {
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
