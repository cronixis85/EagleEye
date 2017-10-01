using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using EagleEye.Extractor.Extensions;
using HtmlAgilityPack;

namespace EagleEye.Extractor.Amazon
{
    public class AmazonHttpClient : HttpClient
    {
        private readonly Uri _baseUri = new Uri("https://www.amazon.com");
        private readonly Uri _siteDirectoryUri = new Uri("/gp/site-directory/", UriKind.Relative);

        public AmazonHttpClient()
        {
            BaseAddress = _baseUri;
        }
        
        public async Task<List<SiteSection>> GetSiteSectionsAsync()
        {
            using (var response = await GetAsync(_siteDirectoryUri))
            {
                var doc = await response.Content.ReadAsHtmlDocumentAsync();

                var siteSections = new List<SiteSection>();

                var deptBoxes = doc.DocumentNode
                                   .SelectNodes("//div[@class='fsdDeptBox']");

                if (deptBoxes == null)
                    return siteSections;

                siteSections = deptBoxes
                    .SelectMany(x =>
                    {
                        var dept = x.Descendants("h2")
                                    .Single(n => n.Attributes["class"].Value == "fsdDeptTitle")
                                    .InnerText;

                        var sections = x.Descendants("a")
                                        .Where(n => n.Attributes["class"].Value == "a-link-normal fsdLink fsdDeptLink")
                                        .Select(a => new SiteSection
                                        {
                                            Department = dept,
                                            Name = a.InnerText,
                                            Uri = new Uri(_baseUri, a.Attributes["href"].Value)
                                        })
                                        .ToArray();

                        return sections;
                    })
                    .ToList();

                return siteSections;
            }
        }

        public async Task<List<SiteSubCategory>> GetCategoriesAsync(SiteSection siteSection)
        {
            using (var response = await GetAsync(siteSection.Uri))
            {
                var doc = await response.Content.ReadAsHtmlDocumentAsync();

                var siteCategories = new List<SiteSubCategory>();

                var categoryLinks = doc.DocumentNode.SelectNodes("//a[@class='list-item__category-link']");

                if (categoryLinks == null)
                    return siteCategories;
                
                var subCategoryWrappers = doc.DocumentNode.SelectNodes("//div[@class='sub-categories__list']");

                if (subCategoryWrappers == null)
                    return siteCategories;

                foreach (var c in categoryLinks)
                {
                    var elementId = c.Attributes["id"].Value;
                    var categoryName = c.InnerText;

                    var subCategoryNode = subCategoryWrappers.SingleOrDefault(x => x.Attributes["id"].Value == "sub" + elementId);

                    if (subCategoryNode == null)
                        continue;

                    siteCategories = subCategoryNode.Descendants("a")
                                   .Select(x => new SiteSubCategory
                                   {
                                       Department = siteSection.Department,
                                       Section = siteSection.Name,
                                       Category = categoryName,
                                       Name = x.InnerText.Replace("\n", "").Trim(),
                                       Uri = new Uri(_baseUri, x.Attributes["href"].Value)
                                   })
                                   .ToList();
                    
                    siteCategories.AddRange(siteCategories);
                }

                return siteCategories;
            }
        }

        public async Task<List<SiteProduct>> GetProductsAsync(Uri uri)
        {
            using (var response = await GetAsync(uri))
            {
                var doc = await response.Content.ReadAsHtmlDocumentAsync();

                var container = doc.DocumentNode.SelectSingleNode("//div[@id='mainResults']");

                var productLinks = container
                    .Descendants("a")
                    .Where(x => x.Attributes["class"].Value.Contains("s-access-detail-page"))
                    .Select(x => new SiteProduct()
                    {
                        Name = x.Attributes["title"].Value,
                        Uri = new Uri(x.Attributes["href"].Value)
                    })
                    .ToList();

                return productLinks;
            }
        }
    }
}