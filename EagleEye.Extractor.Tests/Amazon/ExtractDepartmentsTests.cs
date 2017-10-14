using EagleEye.Extractor.Amazon;
using Xunit;

namespace EagleEye.Extractor.Tests.Amazon
{
    public class ExtractDepartmentsTests : ExtractorTestBase
    {
        [Fact]
        public void ShouldReturnDepartmentsWithSections()
        {
            var doc = GetHtmlDocument("departments.html");

            var departments = new AmazonHttpClient.ExtractDepartments().Execute(doc);

            Assert.Equal(23, departments.Count);

            foreach (var dept in departments)
                Assert.NotEmpty(dept.Sections);
        }
    }
}