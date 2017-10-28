using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EagleEye.Extractor.Amazon.Models;
using EagleEye.Extractor.Extensions;
using HtmlAgilityPack;
using Serilog;

namespace EagleEye.Extractor.Amazon
{
    public partial class AmazonHttpClient : HttpClient
    {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(10);

        private static readonly Uri BaseUri = new Uri("https://www.amazon.com");
        private static readonly Uri SiteDirectoryUri = new Uri("/gp/site-directory/", UriKind.Relative);

        public AmazonHttpClient() : this(new HttpClientHandler())
        {
        }

        public AmazonHttpClient(HttpMessageHandler handler) : base(handler)
        {
            BaseAddress = BaseUri;
        }

        public async Task<List<Department>> GetDepartmentalSectionsAsync(CancellationToken cancellationToken)
        {
            await Semaphore.WaitAsync(TimeSpan.FromHours(1), cancellationToken);

            try
            {
                Log.Information("Getting Departments");

                var result = await GetAsyncAsHtmlDocWithEnsureAllowed(SiteDirectoryUri, cancellationToken);
                return new ExtractDepartments().Execute(result.HtmlDocument);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task<List<Category>> GetCategoriesAsync(Section section, CancellationToken cancellationToken)
        {
            await Semaphore.WaitAsync(TimeSpan.FromHours(1), cancellationToken);

            try
            {
                Log.Information("Getting Categories for Section {Name}", section.Name);

                var result = await GetAsyncAsHtmlDocWithEnsureAllowed(section.Uri, cancellationToken);
                return new ExtractCategories().Execute(result.HtmlDocument);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task<List<Product>> GetProductsAsync(Uri uri, CancellationToken cancellationToken)
        {
            await Semaphore.WaitAsync(TimeSpan.FromHours(1), cancellationToken);

            try
            {
                var result = await GetAsyncAsHtmlDocWithEnsureAllowed(uri, cancellationToken);
                return new ExtractProducts().Execute(result.HtmlDocument);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task<Product> GetProductDetailAsync(Product product, CancellationToken cancellationToken)
        {
            await Semaphore.WaitAsync(TimeSpan.FromHours(1), cancellationToken);

            try
            {
                var result = await GetAsyncAsHtmlDocWithEnsureAllowed(product.Uri, cancellationToken);
                var details = new ExtractProductDetails().Execute(result.HtmlDocument);

                if (result.RedirectUri != null)
                    product.Url = result.RedirectUri.OriginalString;

                product.Name = details.Name;
                product.Brand = details.Brand;
                product.CurrentPrice = details.CurrentPrice;
                product.OriginalPrice = details.OriginalPrice;
                product.Dimensions = details.Dimensions;
                product.ItemWeight = details.ItemWeight;
                product.ShippingWeight = details.ShippingWeight;
                product.Manufacturer = details.Manufacturer;
                product.Asin = details.Asin;
                product.ModelNumber = details.ModelNumber;
                product.Rating = details.Rating;
                product.TotalReviews = details.TotalReviews;
                product.FirstAvailableOn = details.FirstAvailableOn;
                product.Rank = details.Rank;
                product.Errors = details.Errors;

                return product;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private async Task<AmazonResponseResult> GetAsyncAsHtmlDocWithEnsureAllowed(Uri uri, CancellationToken cancellationToken)
        {
            var success = true;
            AmazonResponseResult result = null;

            do
            {
                try
                {
                    result = await GetAsyncAsHtmlDoc(uri, cancellationToken);
                }
                catch (ScraperBlockedException e)
                {
                    success = false;

                    Log.Information("Scrape blocked on {Url}. Retrying in 1 min.", uri.OriginalString);
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
            } while (!success);

            return result;
        }

        private async Task<AmazonResponseResult> GetAsyncAsHtmlDoc(Uri uri, CancellationToken cancellationToken)
        {
            using (var response = await GetAsync(uri, cancellationToken))
            {
                if (response.StatusCode == HttpStatusCode.MovedPermanently || response.StatusCode == HttpStatusCode.Found)
                {
                    var redirectedUri = response.Headers.Location;
                    return await GetAsyncAsHtmlDoc(redirectedUri, cancellationToken);
                }
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = new AmazonResponseResult
                    {
                        HtmlDocument = await response.Content.ReadAsHtmlDocumentAsync(),
                        RedirectUri = response.Headers.Location
                    };

                    // ensure not blocked
                    var title = new ExtractTitle().Execute(result.HtmlDocument);

                    if (title != null && title.Contains("Robot Check"))
                        throw new ScraperBlockedException("Amazon has blocked scraper.");

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