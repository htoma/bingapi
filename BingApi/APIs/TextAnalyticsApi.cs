using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
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
            KeyPhraseBatchResult batchResult = await GetClient().KeyPhrasesAsync(
                          new MultiLanguageBatchInput(
                              new[]
                                  {
                                      new MultiLanguageInput("en", "1", payload)
                                  }));
            
            return batchResult.Documents.SelectMany(x => x.KeyPhrases).Distinct().ToArray();
        }
    }
}