using System;
using System.Collections.Generic;
using System.Linq;
using EagleEye.Extractor.Amazon.Models;
using EagleEye.Extractor.Extensions;
using HtmlAgilityPack;

namespace EagleEye.Extractor.Amazon
{
    public partial class AmazonHttpClient
    {
        public class ExtractProducts
        {
            public List<Product> Execute(HtmlDocument doc)
            {
                var container = doc.DocumentNode.SelectSingleNode("//div[@id='mainResults']");

                if (container == null)
                    return null;

                var productLinks = container
                    .Element("ul")
                    .Elements("li")
                    .Select(x =>
                    {
                        var asin = x.Attributes["data-asin"]?.Value;

                        var link = x.Descendants("a")
                                    .Single(a => a.Attributes["class"].Value.Contains("s-access-detail-page"));

                        var url = link.Attributes["href"].Value;

                        return new Product
                        {
                            Asin = asin,
                            Name = link.Attributes["title"].Value.Clean(),
                            Url = url.StartsWith(BaseUri.AbsoluteUri)
                                ? new Uri(url).AbsoluteUri
                                : new Uri(BaseUri, url).AbsoluteUri
                        };
                    })
                    .ToList();

                return productLinks;
            }
        }
    }
}