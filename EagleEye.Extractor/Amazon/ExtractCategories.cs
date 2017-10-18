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
        public class ExtractCategories : ExecuteBase<List<Category>>
        {
            public override List<Category> ExecuteCore(HtmlDocument doc)
            {
                var catLinks = doc.DocumentNode.SelectNodes("//a[@class='list-item__category-link']");

                if (catLinks == null)
                    return null;

                var subcatWrappers = doc.DocumentNode.SelectNodes("//div[contains(@class, 'sub-categories__list')]");

                if (subcatWrappers == null)
                    return null;

                var categories = new List<Category>();

                foreach (var c in catLinks)
                {
                    var category = new Category
                    {
                        Name = c.InnerText.Clean()
                    };

                    var elementId = c.Attributes["id"].Value;

                    var subCategoryNode = subcatWrappers.SingleOrDefault(x => x.Attributes["id"].Value == "sub" + elementId);

                    if (subCategoryNode == null)
                        continue;

                    category.Subcategories = subCategoryNode.Descendants("a")
                                                            .Select(x => new Subcategory
                                                            {
                                                                Name = x.InnerText.Clean(),
                                                                Url = new Uri(BaseUri, x.Attributes["href"].Value).AbsoluteUri
                                                            })
                                                            .ToList();

                    categories.Add(category);
                }

                return categories;
            }
        }
    }
}