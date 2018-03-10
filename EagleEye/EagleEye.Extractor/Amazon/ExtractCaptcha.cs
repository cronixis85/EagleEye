using System;
using System.Linq;
using EagleEye.Extractor.Amazon.Models;
using HtmlAgilityPack;

namespace EagleEye.Extractor.Amazon
{
    public partial class AmazonHttpClient
    {
        public class ExtractCaptcha : ExtractorBase<ValidateCaptchaResult>
        {
            public override ValidateCaptchaResult ExecuteCore(HtmlDocument doc)
            {
                var form = doc.DocumentNode.SelectSingleNode("//form");

                if (form == null)
                    return null;

                var result = new ValidateCaptchaResult
                {
                    SubmitFormUri = new Uri(BaseUri, form.Attributes["action"].Value)
                };

                result.HiddenInputs = form
                    .Elements("input")
                    .Where(x => x.Attributes["type"]?.Value == "hidden")
                    .ToDictionary(x => x.Attributes["name"].Value, x => x.Attributes["value"].Value);

                var captchaImgUrl = form.Descendants("img").Single().Attributes["src"].Value;
                result.CaptchaImageUri = new Uri(captchaImgUrl);

                return result;
            }
        }
    }
}