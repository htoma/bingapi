using System.Linq;
using System.Net;

namespace BingApi.Helpers
{
    public static class ErrorCodeExtension
    {
        public static HttpStatusCode GetStatusCode(this ErrorCode code)
        {
            var statusCode = typeof(ErrorCode).GetField(code.ToString())
                .GetCustomAttributes(typeof(HttpStatusCodeAttribute), false)
                .Cast<HttpStatusCodeAttribute>().FirstOrDefault();
            return statusCode?.StatusCode ?? HttpStatusCode.InternalServerError;
        }
    }
}