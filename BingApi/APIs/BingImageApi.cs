using System;
using System.Threading.Tasks;
using BingApi.Functions;
using System.Configuration;

namespace BingApi.APIs
{
    using System.Net.Http;
    using BingApi.Model;
    using Newtonsoft.Json;

    public class BingImageApi
    {
        private const string HeaderKeyName = "Ocp-Apim-Subscription-Key";

        private static readonly Lazy<HttpClient> Client = new Lazy<HttpClient>(() => new HttpClient());

        private static HttpClient GetClient()
        {
            if (!Client.Value.DefaultRequestHeaders.Contains(HeaderKeyName))
            {
                Client.Value.DefaultRequestHeaders.Add(HeaderKeyName, ConfigurationManager.AppSettings["BingImageKey"]);
            }

            return Client.Value;
        }

        private const string UriBase = "https://api.cognitive.microsoft.com/bing/v7.0/images/search";

        public static async Task<GifImage[]> GetImages(string[] keywords, EBingImageSearchType searchType)
        {
            return new[]
                {
                    new GifImage
                        {
                            Url = "https://media.giphy.com/media/26Ff7HyH9n400tmta/giphy.gif",
                            Width = 480,
                            Height = 480,
                            AccentColor = "733E1D"
                        },
                    new GifImage
                        {
                            Url = "https://media.giphy.com/media/JIHNyyJSPQJKo/giphy.gif",
                            Width = 500,
                            Height = 276,
                            AccentColor = "676564"
                        }
                };
        }

        private static async Task<GifImage[]> BingImageSearch(string searchQuery)
        {
            var uriQuery = UriBase + "?q=" + Uri.EscapeDataString(searchQuery);            
            var response = await GetClient().GetAsync(new Uri(uriQuery));
            var content = await response.Content.ReadAsStringAsync();

            var value = JsonConvert.DeserializeObject<ApiImage>(content);
            return value.Value;;
        }
    }
}