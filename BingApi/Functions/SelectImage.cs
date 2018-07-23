using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BingApi.DbHelpers;
using BingApi.DbModel;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using BingApi.Helpers;
using Newtonsoft.Json;

namespace BingApi.Functions
{
    public static class SelectImage
    {
        [FunctionName("SelectImage")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "selections/{action}")]
            HttpRequestMessage req,
            string action,
            TraceWriter log)
        {
            log.Info($"Selection request from {req.GetKeyId()}");

            if (!req.IsKeyId(Constants.Mobile))
            {
                return req.CreateErrorResponse(HttpStatusCode.Forbidden, "Unauthorized");
            }

            var content = await req.Content.ReadAsStringAsync();

            try
            {
                switch (action)
                {
                    case "keyword":
                    {
                        var payload = JsonConvert.DeserializeObject<SearchKeyword>(content);
                        await DocumentClientHelper.UpsertDoc(DocumentCollections.SearchKeywordsCollection, payload);
                        break;
                    }
                    case "gif":
                    {
                        var payload = JsonConvert.DeserializeObject<GifSelection>(content);
                        await DocumentClientHelper.UpsertDoc(DocumentCollections.GifSelectionCollection, payload);
                        break;
                    }
                    default:
                        return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Use /keyword or /gif");
                }

                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
