using System;
using System.Collections.Generic;
using System.Linq;
using EagleEye.Extractor.Amazon.Models;
using HtmlAgilityPack;

namespace EagleEye.Extractor.Amazon
{
    public partial class AmazonHttpClient
    {
        public class ExtractDepartments : ExecuteBase<List<Department>>
        {
            public override List<Department> ExecuteCore(HtmlDocument doc)
            {
                var deptBoxes = doc.DocumentNode
                                   .SelectNodes("//div[@class='fsdDeptBox']");

                var siteSections = deptBoxes?
                    .Select(x =>
                    {
                        var dept = new Department
                        {
                            Name = x.Descendants("h2")
                                    .Single(n => n.Attributes["class"].Value == "fsdDeptTitle")
                                    .InnerText,
                            Sections = x.Descendants("a")
                                        .Where(n => n.Attributes["class"].Value == "a-link-normal fsdLink fsdDeptLink")
                                        .Select(a => new Section
                                        {
                                            Name = a.InnerText,
                                            Url = new Uri(BaseUri, a.Attributes["href"].Value).AbsoluteUri
                                        })
                                        .ToList()
                        };

                        return dept;
                    })
                    .ToList();

                return siteSections;
            }
        }
    }
}