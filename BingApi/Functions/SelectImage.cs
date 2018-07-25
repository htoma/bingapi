using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BingApi.APIs;
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
                        await ProfileHelper.UpdateProfileWithSearchKeywords(payload.UserId, payload.Keyword);
                        await DocumentClientHelper.UpsertDoc(DocumentCollections.SearchKeywordsCollection, payload);
                        break;
                    }
                    case "gif":
                    {
                        var payload = JsonConvert.DeserializeObject<GifSelection>(content);
                        // image may not be from giphy
                        // if the image is from giphy and we don't have it, we need to store it
                        var imageKeywords = await BingImageApi.GetImageKeywords(payload.Url);
                        if (imageKeywords.Length != 0)
                        {
                            await ProfileHelper.UpdateProfileWithSearchKeywords(payload.UserId, payload.Category);
                            await DocumentClientHelper.UpsertDoc(DocumentCollections.GifSelectionCollection, payload);
                        }

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
