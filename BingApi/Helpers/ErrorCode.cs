using System.Net;

namespace BingApi.Helpers
{
    public enum ErrorCode
    {
        [HttpStatusCode(HttpStatusCode.InternalServerError)]
        Internal,

        [HttpStatusCode(HttpStatusCode.BadRequest)]
        InvalidInput,

        [HttpStatusCode(HttpStatusCode.ServiceUnavailable)]
        ServiceUnavailable,

        [HttpStatusCode(HttpStatusCode.Forbidden)]
        Unauthorized,

        [HttpStatusCode(HttpStatusCode.NotFound)]
        NotFound
    }
}