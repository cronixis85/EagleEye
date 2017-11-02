using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EagleEye.Extractor.Amazon;
using Xunit;

namespace EagleEye.Extractor.Tests.Amazon
{
    public class ExtractCaptchaTests : ExtractorTestBase
    {
        [Fact]
        public void ShouldReturnFormHiddenInputsAndUrl()
        {
            var doc = GetHtmlDocument("validateCaptcha.html");

            var result = new AmazonHttpClient.ExtractCaptcha().Execute(doc);

            Assert.Equal("https://www.amazon.com/errors/validateCaptcha", result.SubmitFormUri.OriginalString);
            Assert.Equal("JVfGcVbKYN/rya9CmUnoPA==", result.HiddenInputs["amzn"]);
            Assert.Equal("&#047;", result.HiddenInputs["amzn-r"]);
            Assert.Equal("https://images-na.ssl-images-amazon.com/captcha/kwizfixk/Captcha_wdxsietvsx.jpg", result.CaptchaImageUri.OriginalString);
        }
    }
}
