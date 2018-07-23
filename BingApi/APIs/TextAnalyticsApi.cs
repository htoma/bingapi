using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using BingApi.Model;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;

namespace BingApi.APIs
{
    public class TextAnalyticsApi
    {
        private static readonly Lazy<ITextAnalyticsAPI> Client = new Lazy<ITextAnalyticsAPI>(
            () => new TextAnalyticsAPI(
                new ApiKeyServiceClientCredentials(ConfigurationManager.AppSettings["TextAnalyticsKey"]))
                {
                    AzureRegion = AzureRegions.Eastus
                });

        private static ITextAnalyticsAPI GetClient()
        {
            return Client.Value;
        }

        public static async Task<PrefixKeywords> GetKeywords(string payload)
        {
            KeyPhraseBatchResult batchResult = await GetClient().KeyPhrasesAsync(
                          new MultiLanguageBatchInput(
                              new[]
                                  {
                                      new MultiLanguageInput("en", "1", payload)
                                  }));
            
            var keywords = batchResult.Documents.SelectMany(x => x.KeyPhrases).Distinct().ToList();

            var result = new PrefixKeywords
                {
                    TextAnalyticsKeywords = keywords.ToArray()
                };

            if (keywords.Count == 0)
            {
                keywords.Add(GetLastWord(payload));
            }
            else
            {
                // if last word is not part of the last keyword, add it as a keyword
                var lastKeywordLastWord = GetLastWord(keywords.Last());
                var lastTextWord = GetLastWord(payload);
                if (lastTextWord != lastKeywordLastWord)
                {
                    keywords.Add(lastTextWord);
                    result.LastWordAddedToKeywords = true;
                }
            }

            result.AllKeywords = keywords.ToArray();

            return result;
        }

        private static async Task<double?> GetSentimentScore(string payload)
        {
            SentimentBatchResult result = await GetClient().SentimentAsync(
                                              new MultiLanguageBatchInput(
                                                  new List<MultiLanguageInput>
                                                      {
                                                          new MultiLanguageInput("en", "0", payload)
                                                      }));
            return result.Documents[0].Score;
        }

        private static int CountWords(string text)
        {
            return GetWords(text).Length;
        }

        private static string GetLastWord(string text)
        {
            var words = GetWords(text);
            return words.Length > 0 ? words.Last() : null;
        }

        private static string[] GetWords(string text)
        {
            return text.Split(" ,.!&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }
    }
}