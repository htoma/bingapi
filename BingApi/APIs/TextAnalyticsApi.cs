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
            
            var result = batchResult.Documents.SelectMany(x => x.KeyPhrases).Distinct().ToArray();
            if (result.Length > 0)
            {
                return result.Distinct().ToArray();
            }

            var lastWord = GetLastWord(payload);
            return string.IsNullOrEmpty(lastWord) ? new[] {"today"} : new[] {lastWord};
        }

        private static string GetLastWord(string text)
        {
            var words = GetWords(text);
            return words.Length > 0 ? words.Last() : null;
        }

        public static string[] GetWords(string text)
        {
            return text.Split(" ,.!&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);            
        }
    }
}