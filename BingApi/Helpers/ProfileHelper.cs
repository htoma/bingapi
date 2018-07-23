using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BingApi.DbHelpers;
using BingApi.DbModel;
using BingApi.Model;

namespace BingApi.Helpers
{
    public static class ProfileHelper
    {
        public static async Task<UserProfile> GetUserProfile(string userId)
        {
            const int historyLength = 3;
            List<SearchKeyword> searchKeywords =
                await DocumentClientHelper.GetMostRecentDocuments<SearchKeyword>(
                    DocumentCollections.SearchKeywordsCollection,
                    userId, historyLength);
            List<GifSelection> gifSelections = await DocumentClientHelper.GetMostRecentDocuments<GifSelection>(
                DocumentCollections.GifSelectionCollection, userId, historyLength);
            return new UserProfile
            {
                SearchKeywords = searchKeywords.Select(x => x.Keyword).Where(x => !string.IsNullOrEmpty(x)).Distinct()
                    .ToArray(),
                GifSelectionKeywords = gifSelections.SelectMany(x => x.Keywords).Where(x => !string.IsNullOrEmpty(x))
                    .Distinct().ToArray(),
                AccentColors = gifSelections.Select(x => x.AccentColor).Where(x => !string.IsNullOrEmpty(x)).Distinct()
                    .ToArray()
            };
        }
    }
}