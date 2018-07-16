using System.Net.Http;

namespace BingApi.Helpers
{
    public static class HttpResponseExtension
    {
        public static HttpResponseMessage CreateErrorResponse(this HttpRequestMessage request, Error error)
        {
            return request.CreateResponse(error.Code.GetStatusCode(), new ErrorResponse { Error = error });
        }
    }
}