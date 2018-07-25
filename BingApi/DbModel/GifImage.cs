namespace BingApi.DbModel
{
    public class GifImage
    {
        public string ContentUrl { get; set; }
        public string[] Keywords { get; set; }
        public double Score { get; set; }
        public string ThumbnailUrl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string AccentColor { get; set; }
        public string ContentSize { get; set; }
        public string HostPageUrl { get; set; }
        public string HostPageDisplayUrl { get; set; }
    }
}