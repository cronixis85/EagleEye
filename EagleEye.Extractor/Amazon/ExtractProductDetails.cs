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
        public class ExtractProductDetails : ExtractorBase<Product>
        {
            public override Product ExecuteCore(HtmlDocument doc)
            {
                var node = doc.DocumentNode;

                var product = new Product();

                try
                {
                    // brand
                    var brandNode = node.SelectSingleNode("//a[@id='bylineInfo']")
                                    ?? node.SelectSingleNode("//div[@id='brandByline_feature_div']")
                                    ?? node.SelectSingleNode("//div[@id='brandBylineWrapper']");

                    product.Brand = brandNode?.InnerText.Clean();

                    // name
                    var nameNode = node.SelectSingleNode("//span[@id='productTitle']");
                    product.Name = nameNode?.InnerText.Clean();

                    // price (only if variance == null)
                    var priceNode = node.SelectSingleNode("//div[@id='price']");

                    if (priceNode != null)
                    {
                        // ensure is not a price range
                        if (!priceNode.InnerText.Trim().Contains(" - "))
                        {
                            var pricing = priceNode
                                .Descendants("table")
                                .SelectMany(t =>
                                {
                                    var detailsTable = t
                                        .Descendants("tr")
                                        .Where(tr => tr.Attributes["id"]?.Value != "regularprice_savings" && tr.Attributes["id"]?.Value != "dealprice_savings")
                                        .Where(tr => tr.Attributes["class"] == null || !tr.Attributes["class"].Value.Contains("couponFeature"))
                                        .Select(tr =>
                                        {
                                            var td = tr.Elements("td").ToArray();

                                            var property = td[0]?.InnerText.Clean().Replace(":", "");
                                            var value = td[1].Element("span").InnerText.Replace("$", "").Replace(",", "");

                                            return new
                                            {
                                                Key = property,
                                                Value = value
                                            };
                                        })
                                        .Where(tr => !tr.Value.Equals("Lower price", StringComparison.OrdinalIgnoreCase))
                                        .Select(tr => new
                                        {
                                            tr.Key,
                                            Value = decimal.Parse(tr.Value)
                                        })
                                        .ToArray();

                                    return detailsTable;
                                })
                                .Where(x => x.Key != null)
                                .ToDictionary(x => x.Key, x => x.Value);

                            SetPrice(product, pricing);
                        }
                    }

                    // product variance
                    var variance = node.SelectSingleNode("//div[@id='twister_feature_div']");
                    var varianceOptions = variance
                        ?.Descendants("option")
                        .ToArray();

                    if (varianceOptions != null && varianceOptions.Length > 0)
                    {
                        var options = varianceOptions
                            .Where(x => x.Attributes["value"]?.Value != "-1")
                            .Select(x =>
                            {
                                var asin = x.Attributes["value"].Value;
                                var asinRemoveIndex = asin.IndexOf(",", StringComparison.Ordinal);
                                asin = asin.Remove(0, asinRemoveIndex + 1);

                                return new ProductVariance
                                {
                                    Name = x.Attributes["data-a-html-content"].Value.Trim(),
                                    Asin = asin
                                };
                            })
                            .ToList();

                        product.Variances = options;
                    }

                    // product-details_feature_div
                    var featureDiv = node.SelectSingleNode("//div[@id='product-details_feature_div']");

                    if (featureDiv != null)
                    {
                        var details = featureDiv
                            .Descendants("table")
                            .SelectMany(table =>
                            {
                                var detailsTable = table
                                    .Descendants("tr")
                                    .Select(tr =>
                                    {
                                        var td = tr.Elements("td").ToArray();

                                        var propertyTd = td.SingleOrDefault(x => x.Attributes["class"]?.Value == "label");
                                        var property = propertyTd?.InnerText.Clean();

                                        var valueTd = td.SingleOrDefault(x => x.Attributes["class"]?.Value == "value");
                                        var value = valueTd?.InnerHtml;

                                        return new
                                        {
                                            Key = property,
                                            Value = value
                                        };
                                    })
                                    .ToArray();

                                return detailsTable;
                            })
                            .Where(x => x.Key != null)
                            .ToDictionary(x => x.Key, x => x.Value);

                        SetProduct(product, details);

                        return product;
                    }

                    // product details table
                    var standardTableWrapper = node.SelectSingleNode("//div[@id='prodDetails']");

                    if (standardTableWrapper != null)
                    {
                        var details = standardTableWrapper
                            .Descendants("table")
                            .SelectMany(t =>
                            {
                                var rows = t.Descendants("tr");

                                var d = rows?.Select(r => new
                                {
                                    Key = r.Descendants("th").Single().InnerText.Clean(),
                                    Value = r.Descendants("td").Single().InnerHtml
                                });

                                return d;
                            })
                            .Where(d => d != null)
                            .ToDictionary(x => x.Key, x => x.Value);

                        SetProduct(product, details);

                        return product;
                    }

                    // list table
                    var bulletTable = node.SelectSingleNode("//table[@id='productDetailsTable']");

                    // detailBullet
                    if (bulletTable == null)
                        bulletTable = node.SelectSingleNode("//div[@id='detail-bullets']")?.Element("table");

                    if (bulletTable != null)
                    {
                        var details = bulletTable
                            .Element("tr")
                            .Element("td")
                            .Elements("div")
                            .SingleOrDefault(x => x.Attributes["class"]?.Value == "content")
                            ?.Element("ul")
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

                        return product;
                    }
                }
                catch (Exception e)
                {
                    product.Errors = e.StackTrace;
                }

                return product;
            }

            private void SetPrice(Product product, Dictionary<string, decimal> pricing)
            {
                decimal? sale = null;
                decimal? price = null;

                if (pricing.ContainsKey("List Price") || pricing.ContainsKey("With Deal"))
                {
                    if (pricing.ContainsKey("List Price"))
                        price = pricing["List Price"];

                    if (pricing.ContainsKey("With Deal"))
                        sale = pricing["With Deal"];
                }
                else if (pricing.ContainsKey("Was"))
                {
                    if (pricing.ContainsKey("Was"))
                        price = pricing["Was"];

                    if (pricing.ContainsKey("Price"))
                        sale = pricing["Price"];
                }
                else
                {
                    if (pricing.ContainsKey("Price"))
                        price = pricing["Price"];

                    if (pricing.ContainsKey("Sale"))
                        sale = pricing["Sale"];
                }

                if (sale == null && price != null)
                {
                    product.CurrentPrice = price;
                    product.OriginalPrice = price;
                }
                else if (sale != null)
                {
                    if (price == null)
                    {
                        product.CurrentPrice = sale;
                    }
                    else
                    {
                        product.CurrentPrice = sale;
                        product.OriginalPrice = price;
                    }
                }
            }

            private void SetProduct(Product product, Dictionary<string, string> details)
            {
                // brand
                if (product.Brand == null && details.ContainsKey("Brand Name"))
                    product.Brand = details["Brand Name"].Clean();

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

                // Customer Reviews / Average Customer Review
                HtmlNode customerReviewsNode = null;

                if (details.ContainsKey("Customer Reviews"))
                    customerReviewsNode = details["Customer Reviews"].ToHtmlDocument().DocumentNode;
                else if (details.ContainsKey("Average Customer Review"))
                    customerReviewsNode = details["Average Customer Review"].ToHtmlDocument().DocumentNode;

                if (customerReviewsNode != null)
                {
                    if (customerReviewsNode.InnerText.Contains("Be the first to review this item"))
                    {
                        product.Rating = 0;
                    }
                    else
                    {
                        var ratingText = Regex.Match(customerReviewsNode.InnerText, @"\d?(.\d) out of 5 stars").Value;
                        var rating = ratingText.Replace("out of 5 stars", "").Clean();

                        product.Rating = float.Parse(rating);
                    }

                    var totalReviewsText = customerReviewsNode
                        .Descendants("span")
                        .SingleOrDefault(x => x.Attributes["id"]?.Value == "acrCustomerReviewText" || x.Attributes["id"]?.Value == "acrCustomerWriteReviewText" || x.Attributes["class"]?.Value == "a-size-small")
                        ?.InnerText;

                    if (string.IsNullOrEmpty(totalReviewsText) || totalReviewsText.Contains("Be the first to review this item"))
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

                // Date First Available
                if (details.ContainsKey("Date First Available"))
                {
                    var firstAvailableText = details["Date First Available"].Clean();
                    product.FirstAvailableOn = DateTime.Parse(firstAvailableText);
                }

                // best sellers rank
                if (details.ContainsKey("Best Sellers Rank"))
                {
                    var rankNode = details["Best Sellers Rank"].ToHtmlDocument().DocumentNode;

                    var hasClassZgHrsr = rankNode.Descendants("ul").Any(x => x.Attributes["class"]?.Value == "zg_hrsr");

                    if (hasClassZgHrsr)
                    {
                        var ranking = ExtractBestSellerListBasedRanking(rankNode);
                        product.Rank = ranking;
                    }
                    else
                    {
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
                }

                // Amazon Best Sellers Rank
                if (details.ContainsKey("Amazon Best Sellers Rank"))
                {
                    var rankNode = details["Amazon Best Sellers Rank"].ToHtmlDocument().DocumentNode;
                    var ranking = ExtractBestSellerListBasedRanking(rankNode);
                    product.Rank = ranking;
                }
            }

            // use contains class zg_hrsr
            private static Dictionary<string, int> ExtractBestSellerListBasedRanking(HtmlNode rankNode)
            {
                var categoryRanking = new Dictionary<string, int>();

                var childNodes = rankNode.ChildNodes.ToArray();

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

                    categoryRanking.Add(category, int.Parse(rank));
                }

                var li = rankNode
                    .Element("ul")?
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

                if (li == null)
                    return categoryRanking;

                foreach (var item in li)
                    categoryRanking.Add(item.Category, item.Rank);

                return categoryRanking;
            }
        }
    }
}