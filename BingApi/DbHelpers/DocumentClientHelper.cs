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
  
        public static async Task<List<GifImage>> GetGifs(List<string> urls)
        {
            var ids = urls.Select(BingImageApi.ExtractIdFromUrl);
            IDocumentQuery<GifImage> query = Client.Value
                .CreateDocumentQuery<GifImage>(GetCollectionUri(DocumentCollections.GifCollection))
                .Where(x => ids.Contains(x.Id))
                .AsDocumentQuery();
            var result = await query.ExecuteNextAsync<GifImage>();
            return result.ToList();
        }

        public static async Task<UserProfileWithKeywords> GetUserProfile(string userId)
        {
            IDocumentQuery<UserProfileWithKeywords> query = Client.Value
                .CreateDocumentQuery<UserProfileWithKeywords>(GetCollectionUri(DocumentCollections.ProfileCollection))
                .Where(x => x.UserId == userId)
                .AsDocumentQuery();
            var result = await query.ExecuteNextAsync<UserProfileWithKeywords>();
            return result.SingleOrDefault();
        }

        private static readonly Lazy<IDocumentClient> Client =
            new Lazy<IDocumentClient>(() => new DocumentClient(ServiceEndpoint, PrimaryKey));

        private static Uri GetCollectionUri(string collection)
        {
            return UriFactory.CreateDocumentCollectionUri(DocumentCollections.DbName, collection);
        }

        public static async Task UpsertUserProfile(UserProfileWithKeywords newProfile)
        {
            await Client.Value.UpsertDocumentAsync(GetCollectionUri(DocumentCollections.ProfileCollection), newProfile);
        }
    }
}