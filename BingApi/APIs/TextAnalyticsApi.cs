using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;

namespace BingApi.APIs
{
    using BingApi.Model;

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

        public static async Task<Keywords> GetKeywords(string payload)
        {
            KeyPhraseBatchResult batchResult = await GetClient().KeyPhrasesAsync(
                          new MultiLanguageBatchInput(
                              new[]
                                  {
                                      new MultiLanguageInput("en", "1", payload)
                                  }));
            
            var keywords = batchResult.Documents.SelectMany(x => x.KeyPhrases).Distinct().ToList();

            var result = new Keywords
                {
                    TextAnalyticsKeywords = keywords.ToArray()
                };

            const int MaxWordsBeforeFilteringOnText = 4;
            if (keywords.Count == 0) 
            {
                // if no keywords found
                if (CountWords(payload) <= MaxWordsBeforeFilteringOnText)
                {
                    // less than 4 words in the text, we'll search on the whole text
                    keywords.Add(payload);
                    result.EntirePhrase = true;
                }
                else
                {
                    // search on the last word
                    keywords.Add(GetLastWord(payload));
                    result.LastWord = true;
                }
            }
            else
            {
                // if last word is not part of the last keyword, add it as a keyword
                var lastKeywordLastWord = GetLastWord(keywords.Last());
                var lastTextWord = GetLastWord(payload);
                if (lastTextWord != lastKeywordLastWord)
                {
                    keywords.Add(lastTextWord);
                    result.LastWord = true;
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