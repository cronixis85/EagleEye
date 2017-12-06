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
            Assert.Equal(25.95m, details.CurrentPrice);
            Assert.Null(details.OriginalPrice);
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
        public void ShouldReturnProductDetails2Column()
        {
            var doc = GetHtmlDocument("productDetails_2column.html");

            var details = new AmazonHttpClient.ExtractProductDetails().Execute(doc);

            // https://www.amazon.com/Zinus-Compack-Adjustable-Spring-Mattress/dp/B00IGGJQ6O/ref=lp_3733101_1_2?s=furniture&amp;ie=UTF8&amp;qid=1508310075&amp;sr=1-2
            Assert.Equal("Zinus Compack Adjustable Steel Bed Frame, for Box Spring & Mattress Set, Fits Full to King", details.Name);
            Assert.Equal("Zinus", details.Brand);
            Assert.Equal(49.32m, details.CurrentPrice);
            Assert.Equal(49.32m, details.OriginalPrice);
            Assert.Equal("53.5 x 70.5 x 7 inches", details.Dimensions);
            Assert.Equal("25.4 pounds", details.ItemWeight);
            Assert.Equal("25.4 pounds", details.ShippingWeight);
            Assert.Null(details.Manufacturer);
            Assert.Equal("B00IGGJQ6O", details.Asin);
            Assert.Equal("AZ-SBF-07U", details.ModelNumber);
            Assert.Equal(4.5f, details.Rating);
            Assert.Equal(2260, details.TotalReviews);
            Assert.Equal(422, details.Rank["Home & Kitchen"]);
            Assert.Equal(3, details.Rank["Home & Kitchen > Furniture > Bedroom Furniture > Beds, Frames & Bases > Bed Frames"]);
            Assert.Equal(new DateTime(2014, 2, 6), details.FirstAvailableOn);
        }

        [Fact]
        public void ShouldReturnProductDetails3Tables()
        {
            var doc = GetHtmlDocument("productDetails_3tables.html");

            var details = new AmazonHttpClient.ExtractProductDetails().Execute(doc);

            // https://www.amazon.com/Apple-ME088LL-27-Inch-Certified-Refurbished/dp/B00M4LWO8O/ref=lp_13896603011_1_27?s=pc&amp;ie=UTF8&amp;qid=1508312035&amp;sr=1-27
            Assert.Equal("Apple iMac ME088LL/A 27-Inch Desktop ( VERSION) (Certified Refurbished)", details.Name);
            Assert.Equal("Apple", details.Brand);
            Assert.Equal(1127.59m, details.CurrentPrice);
            Assert.Equal(1127.59m, details.OriginalPrice);
            Assert.Equal("25.6 x 20.3 x 8 inches", details.Dimensions);
            Assert.Equal("21 pounds", details.ItemWeight);
            Assert.Equal("30.1 pounds", details.ShippingWeight);
            Assert.Null(details.Manufacturer);
            Assert.Equal("B00M4LWO8O", details.Asin);
            Assert.Equal("ME088LL/A", details.ModelNumber);
            Assert.Equal(4.6f, details.Rating);
            Assert.Equal(16, details.TotalReviews);
            Assert.Equal(7415, details.Rank["Computers & Accessories"]);
            Assert.Equal(144, details.Rank["Computers & Accessories > Desktops > All-in-Ones"]);
            Assert.Equal(new DateTime(2014, 7, 25), details.FirstAvailableOn);
        }

        [Fact]
        public void ShouldReturnProductDetailsForListTableInfo()
        {
            var doc = GetHtmlDocument("productDetails2_listTable.html");

            var details = new AmazonHttpClient.ExtractProductDetails().Execute(doc);

            // https://www.amazon.com/Puller-America-unfinished-American-Plywood/dp/B075YYXYMB/ref=lp_289719_1_2_sspa?s=kitchen&ie=UTF8&qid=1508011883&sr=1-2-spons&psc=1
            Assert.Equal(@"h-BAR Oven Rack Push Puller for Baking, Made in America 17"" L x 4.75"" H x 0.5"" W, unfinished American Birch Plywood - hBAR at Home Series", details.Name);
            Assert.Equal("hBARSCI", details.Brand);
            Assert.Equal(9.99m, details.CurrentPrice);
            Assert.Equal(9.99m, details.OriginalPrice);
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
            Assert.Equal(199.00m, details.CurrentPrice);
            Assert.Equal(299.99m, details.OriginalPrice);
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

        [Fact]
        public void ShouldReturnProductDetailsDetailBulletInfo()
        {
            var doc = GetHtmlDocument("productDetails_detailBullet.html");

            var details = new AmazonHttpClient.ExtractProductDetails().Execute(doc);

            // https://www.amazon.com/Franklin-Sports-Mini-Hockey-Goal/dp/B00T7DZRUO/ref=lp_5680888011_1_11?s=team-sports&amp;ie=UTF8&amp;qid=1508067481&amp;sr=1-11
            Assert.Equal(@"Franklin Sports NHL Mini Hockey Goal Set", details.Name);
            Assert.Equal("Franklin Sports", details.Brand);
            Assert.Equal(17.99m, details.CurrentPrice);
            Assert.Equal(19.99m, details.OriginalPrice);
            Assert.Equal("28 x 7 x 3 inches ; 2 pounds", details.Dimensions);
            Assert.Null(details.ItemWeight);
            Assert.Equal("2.2 pounds", details.ShippingWeight);
            Assert.Null(details.Manufacturer);
            Assert.Equal("B00T7DZRUO", details.Asin);
            Assert.Equal("47009E2", details.ModelNumber);
            Assert.Equal(3.9f, details.Rating);
            Assert.Equal(4, details.TotalReviews);
            Assert.Equal(64494, details.Rank["Sports & Outdoors"]);
            Assert.Equal(8, details.Rank["Sports & Outdoors > Sports & Fitness > Team Sports > Ice Hockey > Rink Equipment > Goals"]);
            Assert.Equal(11, details.Rank["Sports & Outdoors > Sports & Fitness > Team Sports > Other Team Sports > Field Hockey > Goals & Nets"]);
            Assert.Equal(15, details.Rank["Sports & Outdoors > Fan Shop > Sports Equipment > Hockey Equipment > Hockey Gear"]);
            Assert.Null(details.FirstAvailableOn);
        }

        [Fact]
        public void ShouldReturnProductDetailsDetailWithoutDisclaimInfo()
        {
            var doc = GetHtmlDocument("productDetails_detailBullet_withDisclaim.html");

            var details = new AmazonHttpClient.ExtractProductDetails().Execute(doc);

            // https://www.amazon.com/UNEXTATI-Silicone-Card-Slot-Kickstand-iPhone4s/dp/B01FD6CSLY/ref=lp_11971691_1_20?s=musical-instruments&amp;ie=UTF8&amp;qid=1508308547&amp;sr=1-20
            Assert.Equal(@"UNEXTATI iPhone 4 / iPhone 4s Case, PU Leather Case with Silicone Cover, Magnet Closure, Card-Slot, Kickstand, Wallet Case for Apple iPhone4 / iPhone4s", details.Name);
            Assert.Equal("UNEXTATI", details.Brand);
            Assert.Equal(6.99m, details.CurrentPrice);
            Assert.Equal(14.99m, details.OriginalPrice);
            Assert.Null(details.Dimensions);
            Assert.Null(details.ItemWeight);
            Assert.Equal("2.1 ounces", details.ShippingWeight);
            Assert.Null(details.Manufacturer);
            Assert.Equal("B01FD6CSLY", details.Asin);
            Assert.Null(details.ModelNumber);
            Assert.Equal(5.0f, details.Rating);
            Assert.Equal(1, details.TotalReviews);
            Assert.Null(details.Rank);
            Assert.Null(details.FirstAvailableOn);
        }

        [Fact]
        public void ShouldReturnProductDetailsWithVarianceInfo()
        {
            var doc = GetHtmlDocument("productDetail_withVariance.html");

            var details = new AmazonHttpClient.ExtractProductDetails().Execute(doc);

            // https://www.amazon.com/Sockwell-Compression-Socks-Ideal-Travel-Sports-Prolonged-Sitting-Standing/dp/B00OUP6JQA/ref=lp_9590791011_1_26?s=sports-and-fitness-clothing&amp;ie=UTF8&amp;qid=1512275962&amp;sr=1-26
            Assert.Equal(@"Sockwell Men's Concentric Stripe Graduated Compression Socks", details.Name);
            Assert.Equal("Sockwell", details.Brand);
            Assert.Null(details.CurrentPrice);
            Assert.Null(details.OriginalPrice);
            Assert.Null(details.Dimensions);
            Assert.Null(details.ItemWeight);
            Assert.Equal("1.6 ounces", details.ShippingWeight);
            Assert.Null(details.Manufacturer);
            Assert.Equal("B00OUP0RTA", details.Asin);
            Assert.Null(details.ModelNumber);
            Assert.Equal(4.1f, details.Rating);
            Assert.Equal(46, details.TotalReviews);
            Assert.Equal(13885, details.Rank["Sports & Outdoors"]);
            Assert.Equal(9, details.Rank["Sports & Outdoors > Outdoor Recreation > Outdoor Clothing > Men > Compression Socks"]);
            Assert.Equal(46, details.Rank["Sports & Outdoors > Sports & Fitness > Clothing > Men > Compression"]);
            Assert.Equal(118, details.Rank["Sports & Outdoors > Sports & Fitness > Team Sports > Basketball > Clothing > Men"]);
            Assert.Null(details.FirstAvailableOn);

            Assert.Equal("Medium/Large", details.Variances[0].Name);
            Assert.Equal("B00OUP6JR4", details.Variances[0].Asin);

            Assert.Equal("Large/X-Large", details.Variances[1].Name);
            Assert.Equal("B00OUP6JQA", details.Variances[1].Asin);
        }

        [Fact]
        public void ShouldReturnProductDetailsWithSingleCategoryRanking()
        {
            var doc = GetHtmlDocument("productDetails_singleCategoryRanking.html");

            var details = new AmazonHttpClient.ExtractProductDetails().Execute(doc);

            // https://www.amazon.com/Cooling-Sleeves-Protection-outside-activities/dp/B06Y5BKGB4/ref=lp_11443905011_1_15?s=outdoor-recreation&amp;ie=UTF8&amp;qid=1512580925&amp;sr=1-15
            Assert.Equal(@"KOVISS Sports Cooling Arm Sleeves UV Protection Unisex Sun Block Warmer or Cooler Bike Hiking Golf Cycle Drive Outside Activities", details.Name);
            Assert.Equal("koviss", details.Brand);
            Assert.Equal(18.99m, details.CurrentPrice);
            Assert.Equal(18.99m, details.OriginalPrice);
            Assert.Null(details.Dimensions);
            Assert.Null(details.ItemWeight);
            Assert.Equal("6.4 ounces", details.ShippingWeight);
            Assert.Null(details.Manufacturer);
            Assert.Equal("B06Y5BKGB4", details.Asin);
            Assert.Equal("LYSB06Y5BKGB4-SPRTSEQIP", details.ModelNumber);
            Assert.Equal(4.0f, details.Rating);
            Assert.Equal(357, details.TotalReviews);
            Assert.Equal(66008, details.Rank["Sports & Outdoors"]);
            Assert.Null(details.FirstAvailableOn);
        }

        [Fact]
        public void ShouldReturnProductDetailsWithPriceAndFreeReturns()
        {
            var doc = GetHtmlDocument("productDetails_priceWithFreeReturns.html");

            var details = new AmazonHttpClient.ExtractProductDetails().Execute(doc);

            // https://www.amazon.com/Compression-20-30mmHg-Athletics-Pregnancy-Circulation/dp/B0759FB1GK/ref=lp_9590791011_1_16?s=sports-and-fitness-clothing&amp;ie=UTF8&amp;qid=1512580926&amp;sr=1-16
            Assert.Equal(@"Compression Socks for Men & Women (20-30mmHg), BEST for Athletics, Running, Nurses, Shin Splints, Flight Travel, Pregnancy. Improve Blood Circulation & Muscle Recovery", details.Name);
            Assert.Equal("aZengear", details.Brand);
            Assert.Equal(14.99m, details.CurrentPrice);
            Assert.Equal(14.99m, details.OriginalPrice);
            Assert.Null(details.Dimensions);
            Assert.Null(details.ItemWeight);
            Assert.Null(details.ShippingWeight);
            Assert.Null(details.Manufacturer);
            Assert.Equal("B0759KV3WV", details.Asin);
            Assert.Null(details.ModelNumber);
            Assert.Equal(4.9f, details.Rating);
            Assert.Equal(16, details.TotalReviews);
            Assert.Equal(67890, details.Rank["Clothing, Shoes & Jewelry"]);
            Assert.Equal(18, details.Rank["Sports & Outdoors > Outdoor Recreation > Outdoor Clothing > Men > Compression Socks"]);
            Assert.Equal(96, details.Rank["Sports & Outdoors > Sports & Fitness > Clothing > Men > Compression"]);
            Assert.Equal(222, details.Rank["Sports & Outdoors > Sports & Fitness > Team Sports > Basketball > Clothing > Men"]);
            Assert.Equal(new DateTime(2017, 8, 31), details.FirstAvailableOn);
        }
    }
}