using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace EagleEye.Extractor.Extensions
{
    public static class HtmlConversionExtensions
    {
        public static async Task<HtmlDocument> ReadAsHtmlDocumentAsync(this HttpContent source)
        {
            var html = await source.ReadAsStringAsync();
            return html.ToHtmlDocument();
        }

        public static HtmlDocument ToHtmlDocument(this string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc;
        }
    }
}