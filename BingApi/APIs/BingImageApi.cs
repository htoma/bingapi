using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using BingApi.Model;
using Newtonsoft.Json;

namespace BingApi.APIs
{    
    public class BingImageApi
    {
        private const string UriBase = "https://api.cognitive.microsoft.com/bing/v7.0/images/search";

        private static readonly Lazy<HttpClient> Client = new Lazy<HttpClient>(() => new HttpClient());

        private static HttpClient GetClient()
        {
            if (!Client.Value.DefaultRequestHeaders.Contains(ApiHeaders.HeaderKeyName))
            {
                Client.Value.DefaultRequestHeaders.Add(ApiHeaders.HeaderKeyName,
                    ConfigurationManager.AppSettings["BingImageKey"]);
            }

            return Client.Value;
        }

        public static async Task<GifImage[]> GetImages(string[] keywords, int maxImagesPerKeyword)
        {
            // work backwards, last word is the most important
            var result = keywords.Reverse().Select(x => BingImageSearch(x, maxImagesPerKeyword));
            GifImage[][] images = await Task.WhenAll(result);

            // todo: distinct on url
            return images.SelectMany(x => x).ToArray();
        }

        private static async Task<GifImage[]> BingImageSearch(string searchQuery, int max)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                return Array.Empty<GifImage>();
            }

            try
            {
                var uriQuery = UriBase + "?q=" + Uri.EscapeDataString(searchQuery.Replace(" ", "+"));
                var response = await GetClient().GetAsync(new Uri(uriQuery));
                var content = await response.Content.ReadAsStringAsync();

                var value = JsonConvert.DeserializeObject<ApiImage>(content);
                return value.Value.Take(max).ToArray();
            }
            catch (Exception ex)
            {
                return Array.Empty<GifImage>();
            }
        }
    }
}