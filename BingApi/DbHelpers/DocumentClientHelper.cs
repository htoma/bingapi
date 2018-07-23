using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using BingApi.APIs;
using BingApi.DbModel;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace BingApi.DbHelpers
{
    public static class DocumentClientHelper
    {
        private static readonly Uri ServiceEndpoint = new Uri(ConfigurationManager.AppSettings["DbServiceEndpoint"]);
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["DbPrimaryKey"];

        public static async Task UpsertDoc<T>(string collection, T doc) where T : IUserDocument
        {
            doc.Timestamp = DateTime.UtcNow;
            await Client.Value.UpsertDocumentAsync(GetCollectionUri(collection), doc);
        }

        public static async Task<List<T>> GetMostRecentDocuments<T>(
            string collection,
            string userId,
            int max) where T : IUserDocument
        {
            IDocumentQuery<T> query = Client.Value.CreateDocumentQuery<T>(GetCollectionUri(collection))
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Timestamp)
                .Take(max)
                .AsDocumentQuery();
            var result = await query.ExecuteNextAsync<T>();
            return result.ToList();
        }

        public static async Task StoreGif(GifImage gif)
        {
            await Client.Value.CreateDocumentAsync(GetCollectionUri(DocumentCollections.GifCollection), gif);
        }

        public static async Task<GifImage> GetGifByUrl(string url)
        {
            var id = BingImageApi.ExtractIdFromUrl(url);
            if (id == null)
            {
                return null;
            }

            IDocumentQuery<GifImage> query = Client.Value
                .CreateDocumentQuery<GifImage>(GetCollectionUri(DocumentCollections.GifCollection))
                .Where(x => x.Id == id)
                .AsDocumentQuery();
            var result = await query.ExecuteNextAsync<GifImage>();
            return result.SingleOrDefault();
        }

        public static async Task<List<GifImage>> GetGifs(List<string> urls)
        {
            var ids = urls.Select(BingImageApi.ExtractIdFromUrl).ToList();
            IDocumentQuery<GifImage> query = Client.Value
                .CreateDocumentQuery<GifImage>(GetCollectionUri(DocumentCollections.GifCollection))
                .Where(x => ids.Contains(x.Id))
                .AsDocumentQuery();
            var result = await query.ExecuteNextAsync<GifImage>();
            return result.ToList();
        }

        private static readonly Lazy<IDocumentClient> Client =
            new Lazy<IDocumentClient>(() => new DocumentClient(ServiceEndpoint, PrimaryKey));

        private static Uri GetCollectionUri(string collection)
        {
            return UriFactory.CreateDocumentCollectionUri(DocumentCollections.DbName, collection);
        }        
    }
}