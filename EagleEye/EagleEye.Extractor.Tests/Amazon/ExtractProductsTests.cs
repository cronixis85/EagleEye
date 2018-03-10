using EagleEye.Extractor.Amazon;
using Xunit;

namespace EagleEye.Extractor.Tests.Amazon
{
    public class ExtractProductsTests : ExtractorTestBase
    {
        [Fact]
        public void ShouldReturnProducts()
        {
            var doc = GetHtmlDocument("subcategoryListing.html");

            var products = new AmazonHttpClient.ExtractProducts().Execute(doc);

            Assert.Equal(32, products.Count);
        }
    }
}