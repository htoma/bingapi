using BingApi.DbModel;

namespace BingApi.Model
{
    public class CategoryImagesExplained
    {
        public string Prefix { get; set; }
        public string[] PrefixKeywords { get; set; }
        public string Category { get; set; }
        public GifImage[] Images { get; set; }
    }
}