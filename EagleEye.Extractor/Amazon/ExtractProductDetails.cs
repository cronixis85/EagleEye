using System;
using System.Collections.Generic;
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
            public Product Execute(HtmlDocument doc)
            {
                var node = doc.DocumentNode;

                var product = new Product();

                // brand
                var brandNode = node.SelectSingleNode("//a[@id='bylineInfo']")
                                ?? node.SelectSingleNode("//div[@id='brandByline_feature_div']")
                                ?? node.SelectSingleNode("//div[@id='brandBylineWrapper']");

                product.Brand = brandNode?.InnerText.Clean();

                // name
                var nameNode = node.SelectSingleNode("//span[@id='productTitle']");
                product.Name = nameNode?.InnerText.Clean();

                // product details table
                var standardTable = node.SelectSingleNode("//table[@id='productDetails_detailBullets_sections1']");

                if (standardTable != null)
                {
                    var rows = standardTable.Descendants("tr");

                    if (rows == null)
                        return null;

                    var details = rows.ToDictionary(
                        x => x.Descendants("th").Single().InnerText.Clean(),
                        x => x.Descendants("td").Single().InnerHtml);

                    SetProduct(product, details);

                    return product;
                }

                // list table
                var bulletTable = node.SelectSingleNode("//table[@id='productDetailsTable']");

                if (bulletTable != null)
                {
                    try
                    {
                        var details = bulletTable
                            .Element("tr")
                            .Element("td")
                            .Element("div")
                            .Element("ul")
                            .Elements("li")
                            .Select(x =>
                            {
                                var boldElement = x.Element("b");

                                var property = boldElement.InnerText
                                                          .Replace(":", "")
                                                          .Clean();

                                boldElement.Remove();

                                var value = x.InnerHtml;

                                return new
                                {
                                    Key = property,
                                    Value = value
                                };
                            })
                            .ToDictionary(x => x.Key, x => x.Value);

                        SetProduct(product, details);
                    }
                    catch (Exception e)
                    {
                        throw;
                    }

                    return product;
                }

                return product;
            }

            private void SetProduct(Product product, Dictionary<string, string> details)
            {
                // dimensions
                if (details.ContainsKey("Product Dimensions"))
                    product.Dimensions = details["Product Dimensions"].Clean();
                else if (details.ContainsKey("Package Dimensions"))
                    product.Dimensions = details["Package Dimensions"].Clean();

                // item weight
                if (details.ContainsKey("Item Weight"))
                    product.ItemWeight = details["Item Weight"].Clean();

                // shipping weight
                if (details.ContainsKey("Shipping Weight"))
                {
                    var shippingWeight = details["Shipping Weight"];
                    var shippingWeightRemoveIndex = shippingWeight.IndexOf("(", StringComparison.OrdinalIgnoreCase);

                    product.ShippingWeight = shippingWeightRemoveIndex > -1
                        ? shippingWeight.Remove(shippingWeightRemoveIndex).Clean()
                        : shippingWeight.Clean();
                }

                // manufacturer
                if (details.ContainsKey("Manufacturer"))
                    product.Manufacturer = details["Manufacturer"].Clean();

                // ASIN
                if (details.ContainsKey("ASIN"))
                    product.Asin = details["ASIN"].Clean();

                // Item model number
                if (details.ContainsKey("Item model number"))
                    product.ModelNumber = details["Item model number"].Clean();

                // ratings, total reviews
                if (details.ContainsKey("Customer Reviews"))
                {
                    var customerReviews = details["Customer Reviews"].ToHtmlDocument().DocumentNode;

                    var ratingText = Regex.Match(customerReviews.InnerText, @"\d?(.\d) out of 5 stars").Value;
                    var rating = ratingText.Replace("out of 5 stars", "").Clean();

                    product.Rating = float.Parse(rating);

                    var totalReviewsText = customerReviews
                        .Descendants("span")
                        .Single(x => x.Attributes["id"]?.Value == "acrCustomerReviewText" || x.Attributes["id"]?.Value == "acrCustomerWriteReviewText")
                        .InnerText;

                    if (totalReviewsText.Contains("Be the first to review this item"))
                    {
                        product.TotalReviews = 0;
                    }
                    else
                    {
                        totalReviewsText = totalReviewsText
                            .Replace("customer reviews", "")
                            .Replace("customer review", "")
                            .Replace(",", "")
                            .Clean();

                        product.TotalReviews = int.Parse(totalReviewsText);
                    }
                }

                // first available on
                if (details.ContainsKey("Date first available at Amazon.com"))
                {
                    var firstAvailableText = details["Date first available at Amazon.com"].Clean();
                    product.FirstAvailableOn = DateTime.Parse(firstAvailableText);
                }

                // best sellers rank
                if (details.ContainsKey("Best Sellers Rank"))
                {
                    var rankNode = details["Best Sellers Rank"].ToHtmlDocument().DocumentNode;
                    var ranks = rankNode.ChildNodes.Descendants("span").ToArray();

                    product.Rank = ranks
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

                            var rankClean = splitByIn[0]
                                .Replace("#", "")
                                .Replace(",", "")
                                .Clean();

                            var success = int.TryParse(rankClean, out var rank);

                            if (!success)
                                rank = -1;

                            return new
                            {
                                Category = splitByIn[1].Clean(),
                                Rank = rank
                            };
                        })
                        .ToDictionary(x => x.Category, x => x.Rank);
                }

                // Amazon Best Sellers Rank
                if (details.ContainsKey("Amazon Best Sellers Rank"))
                {
                    var rankNode = details["Amazon Best Sellers Rank"].ToHtmlDocument().DocumentNode;

                    var childNodes = rankNode.ChildNodes.ToArray();
                    product.Rank = new Dictionary<string, int>();

                    if (childNodes[0].NodeType.ToString() == "Text")
                    {
                        var firstRankSplit = childNodes[0].InnerText.Split(" in ");

                        var rank = firstRankSplit[0]
                            .Replace("#", "")
                            .Replace(",", "")
                            .Clean();

                        var category = firstRankSplit[1]
                            .Replace("(", "")
                            .Clean();

                        product.Rank.Add(category, int.Parse(rank));
                    }

                    var li = rankNode
                        .Element("ul")
                        .Elements("li")
                        .Select(x =>
                        {
                            // rank
                            var rankSpan = x.ChildNodes.SingleOrDefault(n => n.Attributes["class"]?.Value == "zg_hrsr_rank");
                            var rankClean = rankSpan?.InnerText.Replace("#", "").Replace(",", "").Clean();

                            var success = int.TryParse(rankClean, out var rank);

                            if (!success)
                                rank = -1;

                            // category
                            var categorySpan = x.ChildNodes.SingleOrDefault(n => n.Attributes["class"]?.Value == "zg_hrsr_ladder");
                            var categoryClean = categorySpan?.InnerText
                                                            .Clean()
                                                            .Replace("in ", "");

                            return new
                            {
                                Category = categoryClean,
                                Rank = rank
                            };
                        })
                        .ToArray();

                    foreach (var item in li)
                        product.Rank.Add(item.Category, item.Rank);
                }
            }
        }
    }
}