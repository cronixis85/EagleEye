using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace EagleEye.Extractor.Extensions
{
    public static class HttpContentExtensions
    {
        public static async Task<HtmlDocument> ReadAsHtmlDocumentAsync(this HttpContent source)
        {
            var html = await source.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc;
        }
    }
}