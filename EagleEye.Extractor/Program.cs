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
            dbContext.Database.ExecuteSqlCommand("DELETE FROM SubCategories");

            var isCompleted = RunWebScraper(httpClient, dbContext).Result;
        }

        private static async Task<bool> RunWebScraper(AmazonHttpClient httpClient, ApplicationDbContext dbContext)
        {
            var siteSections = httpClient.GetSectionsAsync().Result;

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

            var includedDepartments = new[]
            {
                "Home, Garden & Tools"
            };

            var getSiteSubcategories = siteSections
                //.Where(x => !excludedDepartments.Contains(x.Department))
                .Where(x => includedDepartments.Contains(x.Department))
                .Select(httpClient.GetSubCategoriesAsync)
                .ToArray();

            await Task.WhenAll(getSiteSubcategories);
            
            var subCategories = getSiteSubcategories.SelectMany(x => x.Result).ToArray();

            dbContext.SubCategories.AddRange(subCategories);
            dbContext.SaveChanges();

            return true;
        }
    }
}