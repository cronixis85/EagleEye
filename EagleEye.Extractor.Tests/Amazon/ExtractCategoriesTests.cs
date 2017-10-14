using EagleEye.Extractor.Amazon;
using Xunit;

namespace EagleEye.Extractor.Tests.Amazon
{
    public class ExtractCategoriesTests : ExtractorTestBase
    {
        [Fact]
        public void ShouldReturnCategoriesWithSubcategories()
        {
            var doc = GetHtmlDocument("section.html");

            var categories = new AmazonHttpClient.ExtractCategories().Execute(doc);

            Assert.Equal(15, categories.Count);

            foreach (var cat in categories)
                Assert.NotEmpty(cat.Subcategories);
        }
    }
}