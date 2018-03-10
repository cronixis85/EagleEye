using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using EagleEye.Extractor.Amazon.Models;
using EagleEye.Extractor.Extensions;
using HtmlAgilityPack;
using Serilog;

namespace EagleEye.Extractor.Amazon
{
    public partial class AmazonHttpClient
    {
        private class SolveCaptcha
        {
            private readonly AmazonHttpClient _httpClient;
            private readonly Uri _uriForLogging;

            public SolveCaptcha(AmazonHttpClient httpClient, Uri uriForLogging)
            {
                _httpClient = httpClient;
                _uriForLogging = uriForLogging;
            }

            public async Task ExecuteAsync(HtmlDocument doc, CancellationToken cancellationToken)
            {
                var solveResult = new SolveCaptchaResult
                {
                    Success = false,
                    NextCaptchaPage = doc
                };

                var tries = 1;

                do
                {
                    // extract
                    var extractResult = await SolveCaptchaPageAsync(solveResult.NextCaptchaPage, cancellationToken);

                    // solve captcha with OCR
                    var answer = await _httpClient.TesseractService.ExecuteAsync(extractResult.CaptchaImageBytes);

                    if (string.IsNullOrEmpty(answer) || answer.Length < 6)
                        solveResult.NextCaptchaPage = await GetNewCaptchaPageAsync(cancellationToken);
                    else
                        solveResult = await SubmitCaptchaAnswerAsync(answer, extractResult.HiddenInputs["amzn"], tries, cancellationToken);

                    tries++;

                } while (!solveResult.Success);
            }

            private async Task<ValidateCaptchaResult> SolveCaptchaPageAsync(HtmlDocument captchaPage, CancellationToken cancellationToken)
            {
                // get captcha image
                var result = new ExtractCaptcha().ExecuteCore(captchaPage);
                result.CaptchaImageBytes = await GetImageBytes(result.CaptchaImageUri, cancellationToken);
                return result;
            }

            private async Task<HtmlDocument> GetNewCaptchaPageAsync(CancellationToken cancellationToken)
            {
                using (var response = await _httpClient.GetAsync(ValidateCaptcha, cancellationToken))
                {
                    var doc = await response.Content.ReadAsHtmlDocumentAsync();
                    return doc;
                }
            }

            private async Task<SolveCaptchaResult> SubmitCaptchaAnswerAsync(string captcha, string amzn, int tries, CancellationToken cancellationToken)
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                query["amzn"] = amzn;
                query["amzn-r"] = "%2F";
                query["field-keywords"] = captcha;

                using (var response = await _httpClient.GetAsync(ValidateCaptcha + "?" + query, cancellationToken))
                {
                    var doc = await response.Content.ReadAsHtmlDocumentAsync();

                    // ensure not blocked
                    var title = new ExtractTitle().Execute(doc);

                    if (title != null && title.Contains("Robot Check"))
                    {
                        Log.Information("{Uri}: Solve Captcha Failed ({Tries} tries)", _uriForLogging, tries);

                        return new SolveCaptchaResult
                        {
                            Success = false,
                            NextCaptchaPage = doc
                        };
                    }

                    Log.Information("{Uri}: Solve Captcha Success ({Tries} tries)", _uriForLogging, tries);

                    return new SolveCaptchaResult
                    {
                        Success = true
                    };
                }
            }

            private async Task<byte[]> GetImageBytes(Uri imageUri, CancellationToken cancellationToken)
            {
                using (var response = await _httpClient.GetAsync(imageUri, cancellationToken))
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    var data = bytes;
                    return data;
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