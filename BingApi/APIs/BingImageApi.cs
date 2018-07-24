using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using BingApi.DbHelpers;
using BingApi.DbModel;
using BingApi.Model;
using Newtonsoft.Json;

namespace BingApi.APIs
{    
    public class BingImageApi
    {
        private const string UriBase = "https://api.cognitive.microsoft.com/bing/v7.0/images/search";

        private const string Pattern = @"<meta name=""keywords"" content=""((.+?)+)"">";

        private const string GiphyDomain = "media.giphy.com";

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
                List<GifImage> images = value.Value.Where(x => x.ContentUrl.Contains(GiphyDomain)).Take(max).ToList();
                foreach (var image in images)
                {
                    AddIdToImage(image);
                    await AddKeywordsToImage(image);
                    await DocumentClientHelper.StoreGif(image);
                }

                return images.ToArray();
            }
            catch (Exception ex)
            {
                return Array.Empty<GifImage>();
            }
        }

        public static string ExtractIdFromUrl(string url)
        {
            if (!url.Contains(GiphyDomain))
            {
                return null;
            }
            var id = url.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            //https://media.giphy.com/media/zHRHMP5jzXdxm/giphy.gif
            return id[3];
        }

        private static void AddIdToImage(GifImage image)
        {
            image.Id = ExtractIdFromUrl(image.ContentUrl);
        }

        private static async Task AddKeywordsToImage(GifImage image)
        {
            image.Keywords = await GetKeywords(image);
        }

        private static async Task<string> GetKeywords(GifImage gif)
        {
            string input = await GetResponse(gif.ContentUrl);
            MatchCollection matches = Regex.Matches(input, Pattern);
            return matches[0].Groups[1].Value;
        }

        private static async Task<string> GetResponse(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url)))
            {
                request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                request.Headers.TryAddWithoutValidation("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                request.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

                using (var response = await Client.Value.SendAsync(request).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                    using (var streamReader = new StreamReader(decompressedStream))
                    {
                        return await streamReader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        public static async Task<bool> IsImageOk(string url)
        {
            var id = ExtractIdFromUrl(url);
            if (id == null)
            {
                return false;
            }

            try
            {
                var image = await DocumentClientHelper.GetGifByUrl(url);
                if (image == null)
                {
                    var newImage = new GifImage
                    {
                        Id = id,
                        ContentUrl = url
                    };
                    var keywords = await GetKeywords(newImage);
                    newImage.Keywords = keywords;
                    await DocumentClientHelper.StoreGif(newImage);                    
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}