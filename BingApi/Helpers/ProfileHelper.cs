using System.Linq;
using System.Threading.Tasks;
using BingApi.DbHelpers;
using BingApi.DbModel;
using BingApi.Model;

namespace BingApi.Helpers
{
    public static class ProfileHelper
    {
        private const int MaxProfileKeywords = 4;
       
        public static async Task<UserProfile> GetUserProfile(string userId)
        {
            UserProfileWithKeywords profile = await DocumentClientHelper.GetUserProfile(userId);

            return new UserProfile
            {
                Keywords = profile.Keywords
            };
        }

        public static async Task UpdateProfileWithSearchKeywords(string userId, string userKeyword)
        {
            if (string.IsNullOrEmpty(userKeyword))
            {
                return;
            }

            var profile = await DocumentClientHelper.GetUserProfile(userId);
            if (profile == null || profile.Keywords.Length == 0)
            {
                var newProfile = new UserProfileWithKeywords
                {
                    UserId = userId,
                    Keywords = new[] { userKeyword }
                };
                await DocumentClientHelper.UpsertUserProfile(newProfile);
            }
            else if (profile.Keywords.Length > 0)
            {
                foreach (var keyword in profile.Keywords)
                {
                    if (await Functions.Similarity.AreSimilar(keyword, userKeyword))
                    {
                        return;
                    }
                }

                profile.Keywords = new[] {userKeyword}.Concat(profile.Keywords).Take(MaxProfileKeywords).ToArray();
                await DocumentClientHelper.UpsertUserProfile(profile);
            }
        }
    }
}