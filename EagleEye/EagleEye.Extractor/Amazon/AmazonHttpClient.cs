using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EagleEye.Extractor.Amazon.Models;
using EagleEye.Extractor.Extensions;
using EagleEye.Extractor.Tesseract;
using HtmlAgilityPack;
using Serilog;

namespace EagleEye.Extractor.Amazon
{
    public partial class AmazonHttpClient : HttpClient
    {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(5);
        private static readonly int SemaphoreMaxWaitTimeInHrs = 6;
        private static readonly Uri BaseUri = new Uri("https://www.amazon.com");
        private static readonly Uri SiteDirectoryUri = new Uri("/gp/site-directory/", UriKind.Relative);
        private static readonly Uri ValidateCaptcha = new Uri("/errors/validateCaptcha", UriKind.Relative);
        private static readonly object Locker = new object();

        private DateTime LastCaptchaSolvedOn { get; set; }

        public RunDotNetTesseract TesseractService { get; set; }

        public AmazonHttpClient() : this(new HttpClientHandler())
        {
        }

        public AmazonHttpClient(HttpMessageHandler handler) : base(handler)
        {
            BaseAddress = BaseUri;
            LastCaptchaSolvedOn = DateTime.Now;
        }

        public async Task<List<Department>> GetDepartmentalSectionsAsync(CancellationToken cancellationToken)
        {
            Log.Information("Getting Departments");

            var result = await GetAsyncAsHtmlDocWithEnsureAllowed(SiteDirectoryUri, cancellationToken);
            return new ExtractDepartments().Execute(result.HtmlDocument);
        }

        public async Task<List<Category>> GetCategoriesAsync(Uri sectionUri, CancellationToken cancellationToken)
        {
            var result = await GetAsyncAsHtmlDocWithEnsureAllowed(sectionUri, cancellationToken);
            var categories = new ExtractCategories().Execute(result.HtmlDocument);

            Log.Information("Found {Count} Categories in Section {Uri}", categories?.Count ?? 0, sectionUri);

            return categories;
        }

        public async Task<List<Product>> GetProductsAsync(Uri subcategoryUri, CancellationToken cancellationToken)
        {
            var result = await GetAsyncAsHtmlDocWithEnsureAllowed(subcategoryUri, cancellationToken);
            var products = new ExtractProducts().Execute(result.HtmlDocument);

            Log.Information("Found {Count} Products in Subcategory {Uri}", products?.Count ?? 0, subcategoryUri);

            return products;
        }

        public async Task<Product> GetProductDetailAsync(Uri productUri, CancellationToken cancellationToken)
        {
            Log.Information("{Uri}: Retrieving", productUri);

            var result = await GetAsyncAsHtmlDocWithEnsureAllowed(productUri, cancellationToken);
            var details = new ExtractProductDetails().Execute(result.HtmlDocument);

            // set the final URL if there is any redirect
            details.Url = result.RedirectUri != null
                ? result.RedirectUri.OriginalString
                : productUri.OriginalString;

            if (details.Variances?.Count > 0)
                foreach (var variance in details.Variances)
                {
                    // https://www.amazon.com/Sockwell-Compression-Socks-Ideal-Travel-Sports-Prolonged-Sitting-Standing/dp/B00OUP6JQA/ref=lp_9590791011_1_26?s=sports-and-fitness-clothing&amp;ie=UTF8&amp;qid=1512275962&amp;sr=1-26

                    var eraseStartIndex = details.Url.IndexOf("/dp/", StringComparison.Ordinal);
                    var erased = details.Url.Remove(eraseStartIndex + 4);
                    var url = $"{erased}{variance.Asin}?th=1&psc=1";

                    variance.Url = url;
                }

            Log.Information("{Uri}: Completed", productUri);

            return details;
        }

        private async Task<AmazonResponseResult> GetAsyncAsHtmlDocWithEnsureAllowed(Uri uri, CancellationToken cancellationToken)
        {
            await Semaphore.WaitAsync(TimeSpan.FromHours(SemaphoreMaxWaitTimeInHrs), cancellationToken);

            try
            {
                var success = true;
                AmazonResponseResult result = null;

                var targetUri = uri;

                do
                {
                    try
                    {
                        result = await GetAsyncAsHtmlDoc(targetUri, cancellationToken);
                        success = true;
                    }
                    catch (EncounterCaptchaException e)
                    {
                        success = false;
                        
                        lock (Locker)
                        {
                            var timeDiff = DateTime.Now - LastCaptchaSolvedOn;

                            // last captcha solved more than 10 seconds ago
                            if (timeDiff.TotalSeconds > 10)
                            {
                                Log.Information("{Uri}: Encounter Captcha", e.Uri);

                                // ensures it will be solved
                                new SolveCaptcha(this, e.Uri).ExecuteAsync(e.HtmlDocument, cancellationToken).Wait(cancellationToken);

                                targetUri = e.Uri;
                            }
                            else
                            {
                                Log.Information("{Uri}: Not required to solve Captcha", e.Uri);
                            }
                        }
                    }
                } while (!success);

                return result;
            }
            catch (Exception e)
            {
                Log.Error(e.StackTrace);
                throw;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private async Task<AmazonResponseResult> GetAsyncAsHtmlDoc(Uri uri, CancellationToken cancellationToken, bool setRedirectUri = false)
        {
            using (var response = await GetAsync(uri, cancellationToken))
            {
                if (response.StatusCode == HttpStatusCode.MovedPermanently || response.StatusCode == HttpStatusCode.Found)
                {
                    var redirectedUri = response.Headers.Location;
                    return await GetAsyncAsHtmlDoc(redirectedUri, cancellationToken, true);
                }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = new AmazonResponseResult
                    {
                        HtmlDocument = await response.Content.ReadAsHtmlDocumentAsync()
                    };

                    if (setRedirectUri)
                        result.RedirectUri = uri;

                    // ensure not blocked
                    var title = new ExtractTitle().Execute(result.HtmlDocument);

                    if (title != null && title.Contains("Robot Check"))
                        throw new EncounterCaptchaException("Amazon has blocked scraper.", uri, result.HtmlDocument);

                    return result;
                }

                throw new NotSupportedException();
            }
        }


        private class AmazonResponseResult
        {
            public Uri RedirectUri { get; set; }

            public HtmlDocument HtmlDocument { get; set; }
        }
    }
}