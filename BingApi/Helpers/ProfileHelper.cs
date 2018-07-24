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
        private const int MaxProfileKeywords = 3;

        public static async Task<UserProfile> GetUserProfile(string userId)
        {
            const int branchLength = 3;
            
            List<SearchKeyword> searchKeywords =
                await DocumentClientHelper.GetMostRecentDocuments<SearchKeyword>(
                    DocumentCollections.SearchKeywordsCollection,
                    userId, branchLength);
            List<GifSelection> gifSelections = await DocumentClientHelper.GetMostRecentDocuments<GifSelection>(
                DocumentCollections.GifSelectionCollection, userId, branchLength);
            List<string> categoryKeywords = gifSelections.Select(x => x.Category).Where(x => !string.IsNullOrEmpty(x))
                .Distinct().ToList();
            List<GifImage> gifs = await DocumentClientHelper.GetGifs(gifSelections.Select(x => x.Url).ToList());
            List<string> gifSelectionKeywords =
                gifs.SelectMany(x => x.Keywords).Where(x => !string.IsNullOrEmpty(x)).Distinct().Take(branchLength)
                    .ToList();
            return new UserProfile
            {
                SearchKeywords = searchKeywords.Select(x => x.Keyword.ToLower()).Where(x => !string.IsNullOrEmpty(x))
                    .Distinct().ToArray(),
                GifSelectionKeywords = categoryKeywords.Concat(gifSelectionKeywords).Distinct().ToArray(),
                AccentColors = gifs.Select(x => x.AccentColor).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray()
            };
        }

        public static string[] GetKeywordsFromProfile(UserProfile profile)
        {
            // add custom logic for combining search keyword and gif selection keywords
            return profile.SearchKeywords.Concat(profile.GifSelectionKeywords).Distinct().ToArray();
        }

        public static async Task UpdateProfile(SearchKeyword payload)
        {
            var profile = await DocumentClientHelper.GetUserProfile(payload.UserId);
            if (profile == null || profile.Keywords.Length == 0)
            {
                var newProfile = new UserProfileWithKeywords
                {
                    UserId = payload.UserId,
                    Keywords = new[] {payload.Keyword}
                };
                await DocumentClientHelper.UpsertUserProfile(newProfile);
            }
            else if (profile.Keywords.Length > 0)
            {
                foreach (var keyword in profile.Keywords)
                {
                    if (await Functions.Similarity.AreSimilar(keyword, payload.Keyword))
                    {
                        return;
                    }
                }

                profile.Keywords = new[] {payload.Keyword}.Concat(profile.Keywords).Take(MaxProfileKeywords).ToArray();
                await DocumentClientHelper.UpsertUserProfile(profile);
            }
        }
    }
}