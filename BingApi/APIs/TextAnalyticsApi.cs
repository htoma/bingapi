using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using BingApi.Helpers;
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

        public static async Task<string[]> GetKeywords(string payload)
        {
            KeyPhraseBatchResult result = await GetClient().KeyPhrasesAsync(
                          new MultiLanguageBatchInput(
                              new[]
                                  {
                                      new MultiLanguageInput("en", "1", payload)
                                  }));

            return result.Documents.SelectMany(x => x.KeyPhrases).Distinct().ToArray();
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
    }
}