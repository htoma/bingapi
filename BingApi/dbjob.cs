using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BingApi.DbHelpers;
using BingApi.DbModel;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.Documents;

namespace BingApi
{
    public static class dbjob
    {
        [FunctionName("dbjob")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequestMessage req,
            TraceWriter log)
        {
            var doc = new UserSearchKeyword
            {
                UserId = "101",
                Keyword = req.GetQueryNameValuePairs().First(x => x.Key == "name").Value,
                Timestamp = DateTime.UtcNow
            };

            //await DocumentClientHelper.InsertDoc(DocumentCollections.SearchKeywordsCollection, doc);

            var results = await DocumentClientHelper.GetDocuments(DocumentCollections.SearchKeywordsCollection,
                (UserSearchKeyword x) => x.Timestamp, 5);

            return req.CreateResponse(HttpStatusCode.OK, results);
        }
    }
}
