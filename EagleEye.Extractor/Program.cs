using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EagleEye.Extractor.Amazon;
using EagleEye.Extractor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EagleEye.Extractor
{
    internal class Program
    {
        private static object _locker = new object();

        private static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            //setup our DI
            var services = new ServiceCollection()
                .AddLogging()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("EagleEyeDb")))
                .AddSingleton<AmazonHttpClient>()
                .BuildServiceProvider();

            var dbContext = services.GetService<ApplicationDbContext>();
            var httpClient = services.GetService<AmazonHttpClient>();

            dbContext.Database.EnsureCreated();
            dbContext.Database.ExecuteSqlCommand("DELETE FROM Products");
            dbContext.Database.ExecuteSqlCommand("DELETE FROM SubCategories");

            var subCategories = GetSubCategoriesAsync(httpClient).Result;

            dbContext.SubCategories.AddRange(subCategories);
            dbContext.SaveChanges();

            var getProductsTasks = subCategories
                .Select(x => Task.Run(async () =>
                {
                    var products = await GetProductsAsync(httpClient, x);

                    if (products.Any())
                    {
                        lock (_locker)
                        {
                            dbContext.Products.AddRange(products);
                            dbContext.SaveChanges(); 
                        }
                    }
                }))
                .ToArray();

            Task.WhenAll(getProductsTasks).Wait();

            //foreach (var s in subCategories)
            //{
            //    var products = GetProductsAsync(httpClient, s).Result;

            //    if (products.Any())
            //    {
            //        dbContext.Products.AddRange(products);
            //        dbContext.SaveChanges();
            //    }
            //}
        }

        private static async Task<List<SubCategory>> GetSubCategoriesAsync(AmazonHttpClient httpClient)
        {
            var sections = await httpClient.GetSectionsAsync();

            var excludedDepartments = new[]
            {
                "Amazon Video",
                "Amazon Music",
                "Appstore for Android",
                "Prime Photos and Prints",
                "Kindle E-readers & Books",
                "Fire Tablets",
                "Subscribe with Amazon",
                "Fire TV",
                "Echo & Alexa",
                "Books & Audible",
                "Movies, Music & Games",
                "Treasure Truck",
                "Food & Grocery",
                "Home Services",
                "Credit & Payment Products"
            };

            var getSubcategoriesTasks = sections
                .Where(x => !excludedDepartments.Contains(x.Department))
                .Select(httpClient.GetSubCategoriesAsync)
                .ToArray();

            await Task.WhenAll(getSubcategoriesTasks);
            
            var subCategories = getSubcategoriesTasks.SelectMany(x => x.Result).ToList();

            return subCategories;
        }

        private static async Task<List<Product>> GetProductsAsync(AmazonHttpClient httpClient, SubCategory subCategory)
        {
            var products = await httpClient.GetProductsAsync(subCategory.Uri);

            foreach (var p in products)
            {
                p.SubCategory = subCategory;
            }

            return products;
        }
    }
}