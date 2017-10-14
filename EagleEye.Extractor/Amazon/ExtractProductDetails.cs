using System;
using System.Linq;
using System.Text.RegularExpressions;
using EagleEye.Extractor.Amazon.Models;
using EagleEye.Extractor.Extensions;
using HtmlAgilityPack;

namespace EagleEye.Extractor.Amazon
{
    public partial class AmazonHttpClient
    {
        public class ExtractProductDetails
        {
            public ProductDetail Execute(HtmlDocument doc)
            {
                var node = doc.DocumentNode;

                // brand
                var brandNode = node.SelectSingleNode("//a[@id='bylineInfo']");
                var brand = brandNode?.InnerText.Clean();

                // name
                var nameNode = node.SelectSingleNode("//span[@id='productTitle']");
                var name = nameNode?.InnerText.Clean();

                // product details table
                var table = node.SelectSingleNode("//table[@id='productDetails_detailBullets_sections1']");

                if (table == null)
                    return null;

                var rows = table.Descendants("tr");

                if (rows == null)
                    return null;

                var details = rows.ToDictionary(
                    x => x.Descendants("th").Single().InnerText.Clean(),
                    x => x.Descendants("td").Single().InnerHtml);

                var shippingWeight = details["Shipping Weight"];
                var shippingWeightRemoveIndex = shippingWeight.IndexOf("(", StringComparison.OrdinalIgnoreCase);
                shippingWeight = shippingWeight.Remove(shippingWeightRemoveIndex);

                var customerReviews = details["Customer Reviews"].ToHtmlDocument().DocumentNode;
                var ratingText = Regex.Match(customerReviews.InnerText, @"\d?(.\d) out of 5 stars").Value;
                var rating = ratingText.Replace("out of 5 stars", "").Clean();

                var totalReviews = customerReviews
                    .Descendants("span")
                    .Single(x => x.Attributes["id"]?.Value == "acrCustomerReviewText")
                    .InnerText
                    .Replace("customer reviews", "")
                    .Replace("customer review", "")
                    .Replace(",", "")
                    .Trim();

                var firstAvailableText = details["Date first available at Amazon.com"].Clean();
                var firstAvailableOn = DateTime.Parse(firstAvailableText);

                var rankNode = details["Best Sellers Rank"].ToHtmlDocument().DocumentNode;
                var ranks = rankNode.ChildNodes.Descendants("span").ToArray();

                var rankDictionary = ranks
                    .Select(x =>
                    {
                        var description = x.InnerText;

                        // if: #67 in Kitchen & Dining (See Top 100 in Kitchen & Dining)
                        if (description.IndexOf("(", StringComparison.Ordinal) != -1 && description.IndexOf(")", StringComparison.Ordinal) != -1)
                        {
                            var descriptionRemoveIndex = description.IndexOf("(", StringComparison.Ordinal);
                            description = description.Remove(descriptionRemoveIndex);
                        }

                        // else: // #1 in Home & Kitchen > Kitchen & Dining > Coffee, Tea & Espresso > Coffee Makers > French Presses
                        var splitByIn = description.Split(new[] {" in "}, StringSplitOptions.None);

                        return new
                        {
                            Category = splitByIn[1].Clean(),
                            Rank = splitByIn[0].Replace("#", "").Clean()
                        };
                    })
                    .ToDictionary(x => x.Category, x => int.Parse(x.Rank));

                return new ProductDetail
                {
                    Name = name,
                    Brand = brand,
                    Dimensions = details["Product Dimensions"].Clean(),
                    ItemWeight = details["Item Weight"].Clean(),
                    ShippingWeight = shippingWeight.Clean(),
                    Manufacturer = details["Manufacturer"].Clean(),
                    Asin = details["ASIN"].Clean(),
                    ModelNumber = details["Item model number"].Clean(),
                    Rating = float.Parse(rating),
                    TotalReviews = int.Parse(totalReviews),
                    FirstAvailableOn = firstAvailableOn,
                    Rank = rankDictionary
                };
            }
        }
    }
}