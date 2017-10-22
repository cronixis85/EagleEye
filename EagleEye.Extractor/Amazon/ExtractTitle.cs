using HtmlAgilityPack;

namespace EagleEye.Extractor.Amazon
{
    public partial class AmazonHttpClient
    {
        public class ExtractTitle : ExtractorBase<string>
        {
            public override string ExecuteCore(HtmlDocument doc)
            {
                return doc.DocumentNode.SelectSingleNode("//title")?.InnerText;
            }
        }
    }
}