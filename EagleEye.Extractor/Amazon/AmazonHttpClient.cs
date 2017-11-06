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
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(6);
        private static readonly Uri BaseUri = new Uri("https://www.amazon.com");
        private static readonly Uri SiteDirectoryUri = new Uri("/gp/site-directory/", UriKind.Relative);
        private static readonly Uri ValidateCaptcha = new Uri("/errors/validateCaptcha", UriKind.Relative);

        public TesseractService TesseractService { get; set; }

        public AmazonHttpClient() : this(new HttpClientHandler())
        {
        }

        public AmazonHttpClient(HttpMessageHandler handler) : base(handler)
        {
            BaseAddress = BaseUri;
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

            details.Url = result.RedirectUri != null
                ? result.RedirectUri.OriginalString
                : productUri.OriginalString;

            Log.Information("{Uri}: Completed", productUri);

            return details;
        }


        private async Task<AmazonResponseResult> GetAsyncAsHtmlDocWithEnsureAllowed(Uri uri, CancellationToken cancellationToken)
        {
            try
            {
                await Semaphore.WaitAsync(TimeSpan.FromHours(1), cancellationToken);

                var success = true;
                AmazonResponseResult result = null;

                do
                {
                    try
                    {
                        result = await GetAsyncAsHtmlDoc(uri, cancellationToken);
                        success = true;
                    }
                    catch (EncounterCaptchaException e)
                    {
                        Log.Information("{Uri}: Encounter Captcha", uri);

                        success = false;
                        await new SolveCaptcha(this, uri).ExecuteAsync(e.HtmlDocument, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        success = false;
                        Log.Error(e.StackTrace);
                        throw;
                    }

                } while (!success);

                return result;
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
                        throw new EncounterCaptchaException("Amazon has blocked scraper.", result.HtmlDocument);

                    return result;
                }

                throw new NotSupportedException();
            }
        }

        private static readonly object CaptchLock = new object();

        

        private class AmazonResponseResult
        {
            public Uri RedirectUri { get; set; }

            public HtmlDocument HtmlDocument { get; set; }
        }
    }
}