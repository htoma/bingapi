using Microsoft.Azure.Documents;

namespace BingApi.DbModel
{
    public class UserProfileWithKeywords
    {
        public string id { get; set; }
        public string UserId { get; set; }
        public string[] Keywords { get; set; }
    }
}