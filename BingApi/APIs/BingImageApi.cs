using System;
using System.Collections.Generic;
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

        public static async Task<KeywordGifImages[]> GetImages(string[] keywords, int maxImagesPerKeyword)
        {
            var result = new List<KeywordGifImages>();
            foreach (var keyword in keywords)
            {
                GifImage[] images = await BingImageSearch(keyword, maxImagesPerKeyword);
                result.Add(new KeywordGifImages
                {
                    Keyword = keyword,
                    Images = images
                });
            }

            return result.ToArray();
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
                uriQuery += "&imageType=AnimatedGifHttps";
                uriQuery += "&mkt=en-US";
                uriQuery += "&safeSearch=Strict";

                // we might want to add the following parameters to find GIFs as SwiftKey Android does.
                // license=conversation

                var response = await GetClient().GetAsync(new Uri(uriQuery));
                var content = await response.Content.ReadAsStringAsync();

                var value = JsonConvert.DeserializeObject<ApiImage>(content);
                return value.Value.Where(x => x.ContentUrl.Contains("media.giphy.com")).Take(max).ToArray();
            }
            catch (Exception ex)
            {
                return Array.Empty<GifImage>();
            }
        }
    }
}