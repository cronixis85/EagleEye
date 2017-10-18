using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EagleEye.Extractor.Amazon.Models;
using EagleEye.Extractor.Extensions;

namespace EagleEye.Extractor.Amazon
{
    public partial class AmazonHttpClient : HttpClient
    {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(7);

        private static readonly Uri BaseUri = new Uri("https://www.amazon.com");
        private static readonly Uri SiteDirectoryUri = new Uri("/gp/site-directory/", UriKind.Relative);

        public AmazonHttpClient()
        {
            BaseAddress = BaseUri;
        }

        public async Task<List<Department>> GetDepartmentalSectionsAsync(CancellationToken cancellationToken)
        {
            await Semaphore.WaitAsync(TimeSpan.FromHours(1));

            try
            {
                using (var response = await GetAsync(SiteDirectoryUri, cancellationToken))
                {
                    var doc = await response.Content.ReadAsHtmlDocumentAsync();
                    return new ExtractDepartments().Execute(doc);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task<List<Category>> GetCategoriesAsync(Section section, CancellationToken cancellationToken)
        {
            await Semaphore.WaitAsync(TimeSpan.FromHours(1));

            try
            {
                using (var response = await GetAsync(section.Uri, cancellationToken))
                {
                    var doc = await response.Content.ReadAsHtmlDocumentAsync();
                    return new ExtractCategories().Execute(doc);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task<List<Product>> GetProductsAsync(Uri uri, CancellationToken cancellationToken)
        {
            await Semaphore.WaitAsync(TimeSpan.FromHours(1));

            try
            {
                using (var response = await GetAsync(uri, cancellationToken))
                {
                    var doc = await response.Content.ReadAsHtmlDocumentAsync();
                    return new ExtractProducts().Execute(doc);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task<Product> GetProductDetailAsync(Product product, CancellationToken cancellationToken)
        {
            await Semaphore.WaitAsync(TimeSpan.FromHours(1));

            try
            {
                using (var response = await GetAsync(product.Uri, cancellationToken))
                {
                    var doc = await response.Content.ReadAsHtmlDocumentAsync();
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
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}