using System.IO;
using EagleEye.Extractor.Extensions;
using HtmlAgilityPack;

namespace EagleEye.Extractor.Tests.Amazon
{
    public abstract class ExtractorTestBase
    {
        protected static HtmlDocument GetHtmlDocument(string fileName)
        {
            var s = File.ReadAllText("Amazon/Fakes/" + fileName);
            return s.ToHtmlDocument();
        }
    }
}