using System;
using EagleEye.Extractor.Amazon;
using Xunit;

namespace EagleEye.Extractor.Tests.Amazon
{
    public class ExtractProductDetailsTests : ExtractorTestBase
    {
        [Fact]
        public void ShouldReturnProductDetails()
        {
            var doc = GetHtmlDocument("productDetails.html");

            var details = new AmazonHttpClient.ExtractProductDetails().Execute(doc);

            // https://www.amazon.com/gp/product/B00DUHACEE/ref=s9_acsd_top_hd_bw_b8Rp8zL_c_x_w?pf_rd_m=ATVPDKIKX0DER&pf_rd_s=merchandised-search-11&pf_rd_r=E42QZDSFVG27M8GRZY5Q&pf_rd_t=101&pf_rd_p=b12e2fd8-a6df-5946-98d4-be1d821fa03b&pf_rd_i=7740213011
            Assert.Equal("French Press Coffee & Tea Makers 8 Cup (1 liter, 34 oz)--Best Coffee Press Pot with 304 Grade Stainless Steel & Heat-Resistant Borosilicate Glass--2 Free Bonus Stainless Steel Screen in Package", details.Name);
            Assert.Equal("SterlingPro", details.Brand);
            Assert.Equal("8 x 5 x 4.3 inches", details.Dimensions);
            Assert.Equal("14.4 ounces", details.ItemWeight);
            Assert.Equal("1.7 pounds", details.ShippingWeight);
            Assert.Equal("SP", details.Manufacturer);
            Assert.Equal("B00DUHACEE", details.Asin);
            Assert.Equal("8cupg", details.ModelNumber);
            Assert.Equal(4.3f, details.Rating);
            Assert.Equal(5581, details.TotalReviews);
            Assert.Equal(67, details.Rank["Kitchen & Dining"]);
            Assert.Equal(1, details.Rank["Home & Kitchen > Kitchen & Dining > Coffee, Tea & Espresso > Coffee Makers > French Presses"]);
            Assert.Equal(new DateTime(2013, 07, 9), details.FirstAvailableOn);
        }
    }
}