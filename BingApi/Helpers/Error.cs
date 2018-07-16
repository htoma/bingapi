using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BingApi.Helpers
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public sealed class Error
    {
        public ErrorCode Code { get; set; }

        public string Message { get; set; }

        public Error(ErrorCode errorCode, string message)
        {
            Code = errorCode;
            Message = message;
        }
    }
}