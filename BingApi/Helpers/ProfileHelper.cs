﻿using System.Collections.Generic;
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
            List<string> categoryKeywords = gifSelections.Select(x => x.Category).Where(x => !string.IsNullOrEmpty(x))
                .Distinct().ToList();
            List<GifImage> gifs = await DocumentClientHelper.GetGifs(gifSelections.Select(x => x.Url).ToList());
            List<string> gifSelectionKeywords =
                gifs.SelectMany(x => x.Keywords).Where(x => !string.IsNullOrEmpty(x)).ToList();
            return new UserProfile
            {
                SearchKeywords = searchKeywords.Select(x => x.Keyword).Where(x => !string.IsNullOrEmpty(x)).Distinct()
                    .ToArray(),
                GifSelectionKeywords = categoryKeywords.Concat(gifSelectionKeywords).Distinct().ToArray(),
                AccentColors = gifs.Select(x => x.AccentColor).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray()
            };
        }

        public static string[] GetKeywordsFromProfile(UserProfile profile)
        {
            // add custom logic for combining search keyword and gif selection keywords
            return profile.SearchKeywords.Concat(profile.GifSelectionKeywords).Distinct().ToArray();
        }
    }
}