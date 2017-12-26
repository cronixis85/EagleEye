using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EagleEye.Extractor.Amazon;
using EagleEye.Extractor.Console.Extensions;
using EagleEye.Models.Extractor;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace EagleEye.Extractor.Console
{
    public class ScrapingService
    {
        private static readonly object Locker = new object();
        private readonly IServiceProvider _serviceProvider;

        public ScrapingService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task RunAsync(ScrapeSettings settings, CancellationTokenSource cts)
        {
            Log.Information("ScrapeSettings = {Settings}", JsonConvert.SerializeObject(settings));

            if (settings.RebuildDatabase)
                await RebuildDatabaseAsync(cts.Token);

            if (settings.UpdateDepartments)
                await UpdateDepartmentalSectionsAsync(cts.Token);

            if (settings.UpdateCategories)
                await UpdateCategoriesAsync(cts.Token);

            if (settings.UpdateProducts)
                await UpdateProductsAsync(cts.Token);

            if (settings.UpdateProductDetails)
                await UpdateProductsDetailsAsync(cts.Token);

            if (settings.UpdateProductVariances)
                await UpdateProductVariancesAsync(cts.Token);
        }

        private async Task RebuildDatabaseAsync(CancellationToken cancellationToken)
        {
            using (var dbContext = _serviceProvider.GetService<ApplicationDbContext>())
            {
                Log.Information("Rebuilding Database");

                await dbContext.Database.EnsureDeletedAsync(cancellationToken);
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            }
        }

        private async Task UpdateDepartmentalSectionsAsync(CancellationToken cancellationToken)
        {
            using (var dbContext = _serviceProvider.GetService<ApplicationDbContext>())
            using (var httpClient = _serviceProvider.GetService<AmazonHttpClient>())
            {
                Log.Information("Getting Departments and Sections");
                
                var found = await httpClient.GetDepartmentalSectionsAsync(cancellationToken);
                
                var depts = found
                    .ToDbDepartments()
                    .ToList();

                // save department, sections
                dbContext.Departments.AddRange(depts);
                dbContext.SaveChanges();
            }
        }

        private async Task UpdateCategoriesAsync(CancellationToken cancellationToken)
        {
            using (var dbContext = _serviceProvider.GetService<ApplicationDbContext>())
            using (var httpClient = _serviceProvider.GetService<AmazonHttpClient>())
            {
                Log.Information("Updating Categories");

                var sections = dbContext.Sections
                                        .Where(x => x.Enabled)
                                        .ToList();

                var getSubCategoryTasks = sections
                    .Select(async x =>
                    {
                        var amzCategories = await httpClient.GetCategoriesAsync(x.Uri, cancellationToken);

                        if (amzCategories != null && amzCategories.Count > 0)
                        {
                            x.Categories = amzCategories.ToDbCategories().ToList();
                            x.Enabled = true;
                        }
                        else
                        {
                            x.Enabled = false;
                        }

                        return x;
                    })
                    .ToArray();

                await Task.WhenAll(getSubCategoryTasks);

                dbContext.SaveChanges();
            }
        }

        private async Task UpdateProductsAsync(CancellationToken cancellationToken)
        {
            using (var dbContext = _serviceProvider.GetService<ApplicationDbContext>())
            using (var httpClient = _serviceProvider.GetService<AmazonHttpClient>())
            {
                Log.Information("Updating Products");

                var subcategories = dbContext.Subcategories
                                             .Where(x => x.Enabled)
                                             .ToList();

                var getProductsTasks = subcategories
                    .Select(async x =>
                    {
                        var amzProducts = await httpClient.GetProductsAsync(x.Uri, cancellationToken);

                        if (amzProducts != null && amzProducts.Count > 0)
                        {
                            x.Products = amzProducts.Select(p =>
                            {
                                // set product details
                                var pdt = new Product
                                {
                                    Url = p.Url,
                                    Name = p.Name,
                                    Brand = p.Brand,
                                    Asin = p.Asin,
                                    CurrentPrice = p.CurrentPrice,
                                    OriginalPrice = p.OriginalPrice,
                                    Dimensions = p.Dimensions,
                                    ItemWeight = p.ItemWeight,
                                    ShippingWeight = p.ShippingWeight,
                                    Manufacturer = p.Manufacturer,
                                    ModelNumber = p.ModelNumber,
                                    Rating = p.Rating,
                                    TotalReviews = p.TotalReviews,
                                    FirstAvailableOn = p.FirstAvailableOn,
                                    Errors = p.Errors,
                                    Status = ProductStatus.Pending.ToString(),
                                    CreatedOn = DateTime.Now
                                };

                                // set ranking 
                                if (p.Rank != null)
                                    pdt.SetRank(p.Rank);

                                return pdt;
                            }).ToList();
                            x.Enabled = true;
                        }
                        else
                        {
                            x.Enabled = false;
                        }

                        lock (Locker)
                        {
                            dbContext.SaveChanges();
                        }

                        return x;
                    })
                    .ToArray();

                await Task.WhenAll(getProductsTasks);
            }
        }

        private async Task UpdateProductsDetailsAsync(CancellationToken cancellationToken)
        {
            using (var dbContext = _serviceProvider.GetService<ApplicationDbContext>())
            using (var httpClient = _serviceProvider.GetService<AmazonHttpClient>())
            {
                Log.Information("Updating Product Details");

                var pendingStatus = ProductStatus.Pending.ToString();

                var products = dbContext.Products
                                        .Where(x => x.Status == pendingStatus)
                                        .ToArray();

                var getProductDetailTasks = products
                    .Select(async p =>
                    {
                        try
                        {
                            var details = await httpClient.GetProductDetailAsync(p.Uri, cancellationToken);

                            p.Url = details.Url;
                            p.Name = details.Name;
                            p.Brand = details.Brand;
                            p.Asin = details.Asin;
                            p.CurrentPrice = details.CurrentPrice;
                            p.OriginalPrice = details.OriginalPrice;
                            p.Dimensions = details.Dimensions;
                            p.ItemWeight = details.ItemWeight;
                            p.ShippingWeight = details.ShippingWeight;
                            p.Manufacturer = details.Manufacturer;
                            p.ModelNumber = details.ModelNumber;
                            p.Rating = details.Rating;
                            p.TotalReviews = details.TotalReviews;
                            p.FirstAvailableOn = details.FirstAvailableOn;
                            p.Errors = details.Errors;
                            p.Status = ProductStatus.Completed.ToString();
                            p.UpdatedOn = DateTime.Now;

                            // set ranking
                            if (p.Rank != null)
                                p.SetRank(details.Rank);

                            if (details.Variances?.Count > 0)
                            {
                                p.HasVariances = true;
                                p.Variances = details.Variances
                                                     .Select(x => new ProductVariance
                                                     {
                                                         Name = x.Name,
                                                         Asin = x.Asin,
                                                         Url = x.Url,
                                                         Status = ProductStatus.Pending.ToString(),
                                                         UpdatedOn = DateTime.Now
                                                     })
                                                     .ToList();
                            }
                        }
                        catch (Exception e)
                        {
                            p.Errors += " " + e.Message + e.StackTrace;
                        }

                        lock (Locker)
                        {
                            dbContext.SaveChanges();
                        }

                        return p;
                    })
                    .ToArray();

                await Task.WhenAll(getProductDetailTasks);
            }
        }

        private async Task UpdateProductVariancesAsync(CancellationToken cancellationToken)
        {
            using (var dbContext = _serviceProvider.GetService<ApplicationDbContext>())
            using (var httpClient = _serviceProvider.GetService<AmazonHttpClient>())
            {
                Log.Information("Updating Product Variances");

                var pendingStatus = ProductStatus.Pending.ToString();

                var variances = dbContext.ProductVariances
                                         .Where(x => x.Status == pendingStatus)
                                         .ToArray();

                var getProductVariances = variances
                    .Select(async p =>
                    {
                        try
                        {
                            var details = await httpClient.GetProductDetailAsync(p.Uri, cancellationToken);

                            p.CurrentPrice = details.CurrentPrice;
                            p.OriginalPrice = details.OriginalPrice;
                            p.UpdatedOn = DateTime.Now;
                            p.Status = ProductStatus.Completed.ToString();
                        }
                        catch (Exception e)
                        {
                            p.Errors = e.Message + e.StackTrace;
                        }

                        lock (Locker)
                        {
                            dbContext.SaveChanges();
                        }

                        return p;
                    })
                    .ToArray();

                await Task.WhenAll(getProductVariances);
            }
        }
    }
}