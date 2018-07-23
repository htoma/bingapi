using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BingApi.DbHelpers;
using BingApi.DbModel;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace BingApi
{
    public static class dbjob
    {
        [FunctionName("dbjob")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestMessage req,
            TraceWriter log)
        {
            var content = await req.Content.ReadAsStringAsync();
            var payload = JsonConvert.DeserializeObject<UserSearchKeyword>(content);
            payload.Timestamp = DateTime.UtcNow;

            await DocumentClientHelper.InsertDoc(DocumentCollections.SearchKeywordsCollection, payload);

            var results =
                await DocumentClientHelper.GetMostRecentDocuments<UserSearchKeyword>(
                    DocumentCollections.SearchKeywordsCollection, payload.UserId, 5);

            return req.CreateResponse(HttpStatusCode.OK, results);
        }
    }
}
