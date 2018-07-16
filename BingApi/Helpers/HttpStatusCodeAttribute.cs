using System;
using System.Net;

namespace BingApi.Helpers
{
    public class HttpStatusCodeAttribute : Attribute
    {
        public HttpStatusCode StatusCode { get; }

        public HttpStatusCodeAttribute(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}