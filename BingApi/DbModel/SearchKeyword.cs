using System;

namespace BingApi.DbModel
{
    public class SearchKeyword : IUserDocument
    {
        public string UserId { get; set; }
        public string Keyword { get; set; }
        public DateTime Timestamp { get; set; }
    }
}