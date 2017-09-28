using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace EagleEye.Extractor.Amazon
{
    public class AmazonHttpClient : HttpClient
    {
        private readonly Uri _siteDirectoryUri = new Uri("https://www.amazon.com/gp/site-directory/");

        public async Task<List<SiteSection>> GetSiteSectionsAsync()
        {
            using (var response = await GetAsync(_siteDirectoryUri))
            {
                var html = await response.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

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
                                            Url = a.Attributes["href"].Value
                                        })
                                        .ToArray();

                        return sections;
                    })
                    .ToList();

                return siteSections;
            }
        }
    }
}