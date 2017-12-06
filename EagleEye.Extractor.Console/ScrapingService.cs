using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EagleEye.Extractor.Amazon;
using EagleEye.Extractor.Amazon.Handlers;
using EagleEye.Extractor.Console.Extensions;
using EagleEye.Extractor.Console.Models;
using EagleEye.Extractor.Extensions;
using EagleEye.Extractor.Tesseract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace EagleEye.Extractor.Console
{
    public class ScrapingService
    {
        private static readonly object Locker = new object();

        private static IConfigurationRoot Configuration { get; set; }

        public void Run()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            // setup logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile(@"logs\EagleEye.Console-{Date}.txt")
                .CreateLogger();

            // setup our DI
            var services = new ServiceCollection()
                .AddLogging()
                .AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(Configuration.GetConnectionString("EagleEyeDb")),
                    ServiceLifetime.Transient)
                .AddSingleton(_ => new ScrapeSettings
                {
                    RebuildDatabase = Convert.ToBoolean(Configuration["ScrapeSettings:RebuildDatabase"]),
                    UpdateDepartments = Convert.ToBoolean(Configuration["ScrapeSettings:UpdateDepartments"]),
                    UpdateCategories = Convert.ToBoolean(Configuration["ScrapeSettings:UpdateCategories"]),
                    UpdateProducts = Convert.ToBoolean(Configuration["ScrapeSettings:UpdateProducts"]),
                    UpdateProductDetails = Convert.ToBoolean(Configuration["ScrapeSettings:UpdateProductDetails"]),
                    UpdateProductVariances = Convert.ToBoolean(Configuration["ScrapeSettings:UpdateProductVariances"])
                })
                .AddSingleton(_ => new RunPythonTesseract(
                    Configuration["Tesseract:Python:Path"],
                    Configuration["Tesseract:Python:CaptchaSolvePath"]))
                .AddSingleton(_ => new RunDotNetTesseract(Configuration["Tesseract:Path"]))
                .AddTransient(_ =>
                {
                    var pipeline = new DefaultHandler()
                        .DecorateWith(new LoggingHandler(Log.Logger));

                    return new AmazonHttpClient(pipeline)
                    {
                        TesseractService = _.GetService<RunDotNetTesseract>()
                    };
                })
                .BuildServiceProvider();

            var settings = services.GetService<ScrapeSettings>();
            var cts = new CancellationTokenSource();

            if (settings.RebuildDatabase)
                RebuildDatabase(services);

            if (settings.UpdateDepartments)
                UpdateDepartmentalSectionsAsync(services, cts.Token).Wait(cts.Token);

            if (settings.UpdateCategories)
                UpdateCategoriesAsync(services, cts.Token).Wait(cts.Token);

            if (settings.UpdateProducts)
                UpdateProductsAsync(services, cts.Token).Wait(cts.Token);

            if (settings.UpdateProductDetails)
                UpdateProductsDetailsAsync(services, cts.Token).Wait(cts.Token);

            if (settings.UpdateProductVariances)
                UpdateProductVariancesAsync(services, cts.Token).Wait(cts.Token);

            Log.CloseAndFlush();
        }

        private void RebuildDatabase(IServiceProvider services)
        {
            using (var dbContext = services.GetService<ApplicationDbContext>())
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
            }
        }

        private async Task UpdateDepartmentalSectionsAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            using (var dbContext = services.GetService<ApplicationDbContext>())
            using (var httpClient = services.GetService<AmazonHttpClient>())
            {
                Log.Information("Getting Departments and Sections");

                var amzDepartments = await httpClient.GetDepartmentalSectionsAsync(cancellationToken);

                var depts = amzDepartments.ToDbDepartments().ToList();

                // save department, sections
                dbContext.Departments.AddRange(depts);
                dbContext.SaveChanges();
            }
        }

        private async Task UpdateCategoriesAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            using (var dbContext = services.GetService<ApplicationDbContext>())
            using (var httpClient = services.GetService<AmazonHttpClient>())
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

        private async Task UpdateProductsAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            using (var dbContext = services.GetService<ApplicationDbContext>())
            using (var httpClient = services.GetService<AmazonHttpClient>())
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
                            x.Products = amzProducts.ToDbProducts().ToList();
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

        private async Task UpdateProductsDetailsAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            using (var dbContext = services.GetService<ApplicationDbContext>())
            using (var httpClient = services.GetService<AmazonHttpClient>())
            {
                var pendingStatus = ProductStatus.Pending.ToString();

                var products = dbContext.Products
                                        .Where(x => x.Status == pendingStatus)
                                        .Take(500)
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
                            p.CurrentPrice = details.CurrentPrice;
                            p.OriginalPrice = details.OriginalPrice;
                            p.Dimensions = details.Dimensions;
                            p.ItemWeight = details.ItemWeight;
                            p.ShippingWeight = details.ShippingWeight;
                            p.Manufacturer = details.Manufacturer;
                            p.Asin = details.Asin;
                            p.ModelNumber = details.ModelNumber;
                            p.Rating = details.Rating;
                            p.TotalReviews = details.TotalReviews;
                            p.FirstAvailableOn = details.FirstAvailableOn;
                            p.Rank = JsonConvert.SerializeObject(details.Rank);
                            p.Errors = details.Errors;
                            p.UpdatedOn = DateTime.Now;
                            p.Status = ProductStatus.Completed.ToString();

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
                            p.Errors = e.Message + e.StackTrace;
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

        private async Task UpdateProductVariancesAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            using (var dbContext = services.GetService<ApplicationDbContext>())
            using (var httpClient = services.GetService<AmazonHttpClient>())
            {
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