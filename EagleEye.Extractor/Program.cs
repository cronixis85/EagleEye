using EagleEye.Extractor.Amazon;

namespace EagleEye.Extractor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var t = new AmazonHttpClient().GetSiteSectionsAsync();
            var s = t.Result;
        }
    }
}