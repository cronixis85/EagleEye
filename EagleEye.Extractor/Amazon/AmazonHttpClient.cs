using System;
using System.Collections.Generic;
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
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(7);

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

                var doc = await GetAsyncAsHtmlDocWithEnsureAllowed(SiteDirectoryUri, cancellationToken);
                return new ExtractDepartments().Execute(doc);
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

                var doc = await GetAsyncAsHtmlDocWithEnsureAllowed(section.Uri, cancellationToken);
                return new ExtractCategories().Execute(doc);
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
                var doc = await GetAsyncAsHtmlDocWithEnsureAllowed(uri, cancellationToken);
                return new ExtractProducts().Execute(doc);
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
                var doc = await GetAsyncAsHtmlDocWithEnsureAllowed(product.Uri, cancellationToken);
                var details = new ExtractProductDetails().Execute(doc);

                product.Name = details.Name;
                product.Brand = details.Brand;
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

        private async Task<HtmlDocument> GetAsyncAsHtmlDocWithEnsureAllowed(Uri uri, CancellationToken cancellationToken)
        {
            var success = true;
            HtmlDocument doc = null;

            do
            {
                try
                {
                    doc = await GetAsyncAsHtmlDoc(uri, cancellationToken);
                }
                catch (ScraperBlockedException e)
                {
                    success = false;

                    Log.Information("Scrape blocked on {Url}. Retrying in 1 min.", uri.OriginalString);
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
            } while (!success);

            return doc;
        }

        private async Task<HtmlDocument> GetAsyncAsHtmlDoc(Uri uri, CancellationToken cancellationToken)
        {
            using (var response = await GetAsync(uri, cancellationToken))
            {
                var doc = await response.Content.ReadAsHtmlDocumentAsync();

                // ensure not blocked
                var title = new ExtractTitle().Execute(doc);

                if (title != null && title.Contains("Robot Check"))
                    throw new ScraperBlockedException("Amazon has blocked scraper.");

                return doc;
            }
        }
    }
}