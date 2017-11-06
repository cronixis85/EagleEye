using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using EagleEye.Extractor.Extensions;
using HtmlAgilityPack;

namespace EagleEye.Extractor.Amazon
{
    public partial class AmazonHttpClient
    {
        public class SolveCaptcha
        {
            private readonly AmazonHttpClient _httpClient;

            public SolveCaptcha(AmazonHttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public async Task ExecuteAsync(HtmlDocument doc, CancellationToken cancellationToken)
            {
                SolveCaptchaResult result = null;
                var captchaPage = doc;

                do
                {
                    result = await SolveCaptchaPageAsync(captchaPage, cancellationToken);

                    if (!result.Success)
                        captchaPage = result.NextCaptchaPage;

                } while (!result.Success);
            }

            private async Task<SolveCaptchaResult> SolveCaptchaPageAsync(HtmlDocument captchaPage, CancellationToken cancellationToken)
            {
                // get captcha image
                var captchaResult = new ExtractCaptcha().ExecuteCore(captchaPage);
                captchaResult.CaptchaBase64 = await GetImageBase64Async(captchaResult.CaptchaImageUri, cancellationToken);

                // solve captcha
                var answer = _httpClient.TesseractService.Run(captchaResult.CaptchaBase64);

                // submit answer
                var query = HttpUtility.ParseQueryString(string.Empty);

                foreach (var input in captchaResult.HiddenInputs)
                    query[input.Key] = input.Value;

                query["field-keywords"] = answer;

                using (var response = await _httpClient.GetAsync(ValidateCaptcha + "?" + query, cancellationToken))
                {
                    var doc = await response.Content.ReadAsHtmlDocumentAsync();

                    // ensure not blocked
                    var title = new ExtractTitle().Execute(doc);

                    if (title != null && title.Contains("Robot Check"))
                        return new SolveCaptchaResult()
                        {
                            Success = false,
                            NextCaptchaPage = doc
                        };

                    return new SolveCaptchaResult()
                    {
                        Success = true
                    };
                }
            }

            private async Task<string> GetImageBase64Async(Uri imageUri, CancellationToken cancellationToken)
            {
                using (var response = await _httpClient.GetAsync(imageUri, cancellationToken))
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    var s = Convert.ToBase64String(bytes);
                    return s;
                }
            }

            private class SolveCaptchaResult
            {
                public bool Success { get; set; }

                public HtmlDocument NextCaptchaPage { get; set; }
            }
        }
    }
}