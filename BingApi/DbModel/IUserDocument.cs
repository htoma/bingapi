using System;

namespace BingApi.DbModel
{
    public interface IUserDocument
    {
        string UserId { get; set; }
        DateTime Timestamp { get; set; }
    }
}