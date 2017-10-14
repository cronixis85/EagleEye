using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EagleEye.Extractor.Amazon;
using EagleEye.Extractor.Console.Extensions;
using EagleEye.Extractor.Console.Models;
using EagleEye.Extractor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EagleEye.Extractor.Console
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

            using (var dbContext = services.GetService<ApplicationDbContext>())
            using (var httpClient = services.GetService<AmazonHttpClient>())
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                // departments, sections
                var departments = httpClient.GetDepartmentalSectionsAsync().Result;

                var sections = departments.SelectMany(x => x.Sections).ToArray();

                var getSubCategoryTasks = sections
                    .Select(async x =>
                    {
                        x.Categories = await httpClient.GetCategoriesAsync(x);
                        return x.Categories;
                    })
                    .ToArray();

                Task.WhenAll(getSubCategoryTasks).Wait();

                // save department, sections
                var depts = departments.ToDbDepartments().ToList();
                dbContext.Departments.AddRange(depts);
                dbContext.SaveChanges();

                //dbContext.SubCategories.AddRange(subcategories);
                //dbContext.SaveChanges();

                //// products
                //var getProductsTasks = subcategories
                //    .Select(s => Task.Run(async () =>
                //    {
                //        var prods = await httpClient.GetProductsAsync(s);

                //        if (!prods.Any())
                //            return prods;

                //        var asins = prods.Select(p => p.Asin).ToArray();

                //        lock (_locker)
                //        {
                //            var existingProds = dbContext.Products.Where(p => asins.Contains(p.Asin)).ToArray();

                //            foreach (var p in prods)
                //            {
                //                var toUpdate = existingProds.SingleOrDefault(x => x.Asin == p.Asin);

                //                if (toUpdate == null)
                //                {
                //                    // add
                //                    dbContext.Products.Add(p);
                //                }
                //                else
                //                {
                //                    // edit
                //                    toUpdate.Name = p.Name;
                //                    toUpdate.Url = p.Url;
                //                }
                //            }

                //            dbContext.SaveChanges();
                //        }

                //        return prods;
                //    }))
                //    .ToArray();

                //Task.WhenAll(getProductsTasks).Wait();

                //// product detail
                //var products = getProductsTasks.SelectMany(x => x.Result).ToArray();

                //var getProductDetailTasks = products
                //    .Select(x => Task.Run(async () =>
                //    {
                //        var d = await httpClient.GetProductDetailAsync(x);

                //        lock (_locker)
                //        {
                //            dbContext.ProductDetails.Add(d);
                //            dbContext.SaveChanges();
                //        }

                //        return d;
                //    }))
                //    .ToArray();

                //Task.WhenAll(getProductDetailTasks).Wait();
            }
        }

        private static async Task<List<Subcategory>> GetSubCategoriesAsync(AmazonHttpClient httpClient)
        {
            var sections = await httpClient.GetDepartmentalSectionsAsync();

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

            throw new NotImplementedException();

            //var getSubcategoriesTasks = sections
            //    .Where(x => !excludedDepartments.Contains(x.Department))
            //    .Select(httpClient.GetSubCategoriesAsync)
            //    .ToArray();

            //await Task.WhenAll(getSubcategoriesTasks);

            //var subCategories = getSubcategoriesTasks.SelectMany(x => x.Result).ToList();

            //return subCategories;
        }
    }
}