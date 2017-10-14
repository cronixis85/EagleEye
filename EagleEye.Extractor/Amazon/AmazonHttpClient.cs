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
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(10);

        private static readonly Uri BaseUri = new Uri("https://www.amazon.com");
        private static readonly Uri SiteDirectoryUri = new Uri("/gp/site-directory/", UriKind.Relative);

        public AmazonHttpClient()
        {
            BaseAddress = BaseUri;
        }

        public async Task<List<Department>> GetDepartmentalSectionsAsync()
        {
            await Semaphore.WaitAsync(TimeSpan.FromHours(1));

            try
            {
                using (var response = await GetAsync(SiteDirectoryUri))
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

        public async Task<List<Category>> GetCategoriesAsync(Section section)
        {
            await Semaphore.WaitAsync(TimeSpan.FromHours(1));

            try
            {
                using (var response = await GetAsync(section.Uri))
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

        public async Task<List<Product>> GetProductsAsync(Subcategory subcategory)
        {
            await Semaphore.WaitAsync(TimeSpan.FromHours(1));

            try
            {
                using (var response = await GetAsync(subcategory.Uri))
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

        public async Task<ProductDetail> GetProductDetailAsync(Product product)
        {
            await Semaphore.WaitAsync(TimeSpan.FromHours(1));

            try
            {
                using (var response = await GetAsync(product.Uri))
                {
                    var doc = await response.Content.ReadAsHtmlDocumentAsync();
                    return new ExtractProductDetails().Execute(doc);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}