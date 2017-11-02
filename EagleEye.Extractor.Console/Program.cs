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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace EagleEye.Extractor.Console
{
    internal class Program
    {
        private static readonly object _locker = new object();

        private static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
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
                    options.UseSqlServer(Configuration.GetConnectionString("EagleEyeDb")))
                .AddSingleton(_ =>
                {
                    var pipeline = new DefaultHandler()
                        .DecorateWith(new LoggingHandler(Log.Logger));

                    return new AmazonHttpClient(pipeline);
                })
                .BuildServiceProvider();

            using (var dbContext = services.GetService<ApplicationDbContext>())
            using (var httpClient = services.GetService<AmazonHttpClient>())
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                var cts = new CancellationTokenSource();

                UpdateDepartmentalSectionsAsync(dbContext, httpClient, cts.Token).Wait(cts.Token);
                UpdateCategoriesAsync(dbContext, httpClient, cts.Token).Wait(cts.Token);
                UpdateProductsAsync(dbContext, httpClient, cts.Token).Wait(cts.Token);
                UpdateProductsDetailsAsync(dbContext, httpClient, cts.Token).Wait(cts.Token);
            }

            Log.CloseAndFlush();
        }

        private static async Task UpdateDepartmentalSectionsAsync(ApplicationDbContext dbContext, AmazonHttpClient httpClient, CancellationToken cancellationToken)
        {
            Log.Information("Getting Departments and Sections");

            var amzDepartments = await httpClient.GetDepartmentalSectionsAsync(cancellationToken);

            var depts = amzDepartments.ToDbDepartments().ToList();

            // save department, sections
            dbContext.Departments.AddRange(depts);
            dbContext.SaveChanges();
        }

        private static async Task UpdateCategoriesAsync(ApplicationDbContext dbContext, AmazonHttpClient httpClient, CancellationToken cancellationToken)
        {
            Log.Information("Updating Categories");

            var sections = dbContext.Sections
                                    .Where(x => x.Enabled)
                                    .Where(x => x.Name == "Kitchen & Dining")
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

        private static async Task UpdateProductsAsync(ApplicationDbContext dbContext, AmazonHttpClient httpClient, CancellationToken cancellationToken)
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

                    lock (_locker)
                    {
                        dbContext.SaveChanges();
                    }

                    return x;
                })
                .ToArray();

            await Task.WhenAll(getProductsTasks);
        }

        private static async Task UpdateProductsDetailsAsync(ApplicationDbContext dbContext, AmazonHttpClient httpClient, CancellationToken cancellationToken)
        {
            var pendingStatus = ProductStatus.Pending.ToString();

            var products = dbContext.Products
                .Where(x => x.Status == pendingStatus)
                .ToArray();
            
            var getProductDetailTasks = products
                .Select(async p =>
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

                    lock (_locker)
                    {
                        dbContext.SaveChanges();
                    }

                    return p;
                })
                .ToArray();

            await Task.WhenAll(getProductDetailTasks);
        }
    }
}