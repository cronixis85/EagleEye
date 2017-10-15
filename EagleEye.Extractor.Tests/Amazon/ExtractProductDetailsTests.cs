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

        [Fact]
        public void ShouldReturnProductDetailsForListTableInfo()
        {
            var doc = GetHtmlDocument("productDetails2_listTable.html");

            var details = new AmazonHttpClient.ExtractProductDetails().Execute(doc);

            // https://www.amazon.com/Puller-America-unfinished-American-Plywood/dp/B075YYXYMB/ref=lp_289719_1_2_sspa?s=kitchen&ie=UTF8&qid=1508011883&sr=1-2-spons&psc=1
            Assert.Equal(@"h-BAR Oven Rack Push Puller for Baking, Made in America 17"" L x 4.75"" H x 0.5"" W, unfinished American Birch Plywood - hBAR at Home Series", details.Name);
            Assert.Equal("hBARSCI", details.Brand);
            Assert.Equal("17 x 0.5 x 4.8 inches ; 3 ounces", details.Dimensions);
            Assert.Null(details.ItemWeight);
            Assert.Equal("15.2 ounces", details.ShippingWeight);
            Assert.Null(details.Manufacturer);
            Assert.Equal("B075YYXYMB", details.Asin);
            Assert.Null(details.ModelNumber);
            Assert.Equal(0, details.Rating);
            Assert.Equal(0, details.TotalReviews);
            Assert.Equal(89617, details.Rank["Industrial & Scientific"]);
            Assert.Equal(261831, details.Rank["Home & Kitchen > Kitchen & Dining"]);
            Assert.Null(details.FirstAvailableOn);
        }

        [Fact]
        public void ShouldReturnProductDetailsFeatureDivInfo()
        {
            var doc = GetHtmlDocument("productDetails3_product-details_feature_div.html");

            var details = new AmazonHttpClient.ExtractProductDetails().Execute(doc);

            // https://www.amazon.com/Black-Beginners-Compatible-Connectivity-Zivix/dp/B0149YRRXA/ref=lp_11971381_1_2?s=musical-instruments&amp%3Bie=UTF8&amp%3Bqid=1508047561&amp%3Bsr=1-2&th=1
            Assert.Equal(@"Jamstik+ Black Portable App Enabled MIDI Electric Guitar, for Beginners and Music Creators, iOS, Android & Mac Compatible, with Bluetooth Connectivity, Powered by Zivix", details.Name);
            Assert.Equal("Zivix", details.Brand);
            Assert.Equal("16.5 x 3.5 x 2.5 inches", details.Dimensions);
            Assert.Equal("1.9 pounds", details.ItemWeight);
            Assert.Equal("3.2 pounds", details.ShippingWeight);
            Assert.Null(details.Manufacturer);
            Assert.Equal("B0149YRRXA", details.Asin);
            Assert.Equal("Black", details.ModelNumber);
            Assert.Equal(3.2f, details.Rating);
            Assert.Equal(206, details.TotalReviews);
            Assert.Equal(544, details.Rank["Amazon Launchpad"]);
            Assert.Equal(3, details.Rank["Musical Instruments > Guitars > Electric Guitars"]);
            Assert.Equal(98, details.Rank["Amazon Launchpad > Gadgets"]);
            Assert.Equal(new DateTime(2015, 8, 26), details.FirstAvailableOn);
        }
    }
}