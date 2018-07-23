using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BingApi.Helpers;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace BingApi.DbHelpers
{
    public static class DocumentClientHelper
    {
        private static readonly Uri ServiceEndpoint = new Uri(ConfigurationManager.AppSettings["DbServiceEndpoint"]);
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["DbPrimaryKey"];

        public static async Task InsertDoc<T>(string collection, T doc)
        {
            var result = await Client.Value.CreateDocumentAsync(GetCollectionUri(collection), doc);
        }

        public static async Task<List<T>> GetDocuments<T>(
            string collection,
            Expression<Func<T, DateTime>> filter,
            int max)
        {
            IDocumentQuery<T> query = Client.Value.CreateDocumentQuery<T>(GetCollectionUri(collection))
                .OrderByDescending(filter)
                .Take(max)
                .AsDocumentQuery();
            var result = await query.ExecuteNextAsync<T>();
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