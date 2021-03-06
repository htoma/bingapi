using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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

        public static async Task<GifImage[]> GetImages(string[] keywords, int totalImages, int keywordsPerImage)
        {
            var result = await GetImages(keywords, totalImages);

            string[] newKeywords = keywords.Select(GetLongestKeyword).ToArray();
            result = await GetImages(newKeywords, totalImages);

            if (keywordsPerImage > 0)
            {
                var tasks = result.Select(x => AddKeywordsToImage(x, keywordsPerImage));
                await Task.WhenAll(tasks);
            }

            return result.ToArray();
        }

        private static string GetLongestKeyword(string text)
        {
            var words = TextAnalyticsApi.GetWords(text);
            if (words.Length <= 1)
            {
                return text;
            }

            return words.OrderByDescending(x => x.Length).First();
        }

        private static async Task<List<GifImage>> GetImages(string[] keywords, int totalImages)
        {
            var imagesPerKeyword = totalImages / keywords.Length;
            if (totalImages % keywords.Length != 0)
            {
                imagesPerKeyword++;
            }

            var result = new List<GifImage>();
            foreach (var keyword in keywords)
            {
                GifImage[] images = await BingImageSearch(keyword, imagesPerKeyword);
                result.AddRange(images);
            }

            return result;
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
                //List<GifImage> images = value.Value.Where(x => x.ContentUrl.Contains(GiphyDomain)).Take(max).ToList();
                List<GifImage> images = value.Value.Take(max).ToList();

                return images.ToArray();
            }
            catch (Exception ex)
            {
                return Array.Empty<GifImage>();
            }
        }

        private static async Task AddKeywordsToImage(GifImage image, int keywordsPerImage)
        {
            image.Keywords = await GetKeywords(image, keywordsPerImage);
        }

        private static async Task<string[]> GetKeywords(GifImage gif, int keywordsPerImage)
        {
            // filter out words using blacklist
            try
            {
                string input = await GetResponse(gif.ContentUrl);
                MatchCollection matches = Regex.Matches(input, Pattern);
                var keywords = matches[0].Groups[1].Value.Split(new[] {",", " "}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.ToLower()).Where(x => !Blacklist.BlacklistWords.Contains(x)).ToArray();
                return keywords.Distinct().Take(keywordsPerImage).ToArray();
            }
            catch (Exception ex)
            {
                return Array.Empty<string>();
            }
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
    }
}