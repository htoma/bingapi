using System;

namespace BingApi.DbModel
{
    public class GifSelection : IUserDocument
    {
        public string UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }
    }
}