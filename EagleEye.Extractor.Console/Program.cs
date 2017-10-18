using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EagleEye.Extractor.Amazon;
using EagleEye.Extractor.Console.Extensions;
using EagleEye.Extractor.Console.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Department = EagleEye.Extractor.Amazon.Models.Department;

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

                var departments = UpdateCategoriesAsync(httpClient).Result;

                // save department, sections
                var depts = departments.ToDbDepartments().ToList();
                dbContext.Departments.AddRange(depts);
                dbContext.SaveChanges();

                var subcats = depts
                    .Where(x => x.Sections != null)
                    .SelectMany(x => x.Sections)
                    .Where(x => x.Categories != null)
                    .SelectMany(x => x.Categories)
                    .Where(x => x.Subcategories != null)
                    .SelectMany(x => x.Subcategories)
                    .ToList();

                UpdateProducts(httpClient, dbContext, subcats).Wait();
            }
        }

        private static async Task<List<Department>> UpdateCategoriesAsync(AmazonHttpClient httpClient)
        {
            var ctr = new CancellationTokenSource();

            // departments, sections
            var departments = httpClient.GetDepartmentalSectionsAsync(ctr.Token)
                                        .Result
                                        .Where(x => x.Name == "Home, Garden & Tools")
                                        .ToList();

            var sections = departments
                .SelectMany(x => x.Sections)
                //.Where(x => x.Name == "Kitchen & Dining")
                .ToArray();

            //foreach (var s in sections)
            //{
            //    s.Categories = await httpClient.GetCategoriesAsync(s, ctr.Token);
            //}

            var getSubCategoryTasks = sections
                .Select(async x =>
                {
                    x.Categories = await httpClient.GetCategoriesAsync(x, ctr.Token);
                    return x;
                })
                .ToArray();

            await Task.WhenAll(getSubCategoryTasks).ConfigureAwait(false);

            return departments;
        }

        private static async Task UpdateProducts(AmazonHttpClient httpClient, ApplicationDbContext dbContext, List<Subcategory> subcategories)
        {
            foreach (var s in subcategories)
                dbContext.Entry(s).State = EntityState.Detached;

            var ctr = new CancellationTokenSource();

            var getProductTasks = subcategories
                .Select(async s =>
                {
                    var products = await httpClient.GetProductsAsync(s.Uri, ctr.Token);

                    if (products == null)
                        return s;

                    var getDetailTasks = products
                        .Select(async p =>
                        {
                            var pd = await httpClient.GetProductDetailAsync(p, ctr.Token);
                            return pd;
                        })
                        .ToArray();

                    Task.WhenAll(getDetailTasks).Wait(ctr.Token);

                    s.Products = getDetailTasks
                        .Select(x => x.Result)
                        .ToDbProducts().ToList();

                    return s;
                })
                .Select(x =>
                {
                    lock (_locker)
                    {
                        var subcat = x.Result;

                        if (subcat.Products != null && subcat.Products.Count > 0)
                        {
                            dbContext.Subcategories.Update(subcat);
                            dbContext.SaveChanges();
                        }
                    }

                    return x;
                })
                .ToArray();

            await Task.WhenAll(getProductTasks);
        }
    }
}