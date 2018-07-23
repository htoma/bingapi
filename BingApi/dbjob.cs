using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BingApi.DbHelpers;
using BingApi.DbModel;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Linq;

namespace BingApi
{
    public static class dbjob
    {
        [FunctionName("dbjob")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequestMessage req,
            TraceWriter log)
        {
            var results =
                await DocumentClientHelper.GetMostRecentDocuments<SearchKeyword>(
                    DocumentCollections.SearchKeywordsCollection,
                    req.GetQueryNameValuePairs().First(x => x.Key == "name").Value, 5);

            return req.CreateResponse(HttpStatusCode.OK, results);
        }
    }
}
