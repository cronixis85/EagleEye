using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EagleEye.Extractor.Amazon;

namespace EagleEye.Extractor
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var isCompleted = RunWebScraper().Result;
        }

        private static async Task<bool> RunWebScraper()
        {
            var httpClient = new AmazonHttpClient();

            var siteSections = httpClient.GetSiteSectionsAsync().Result;

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

            var getSiteSubcategories = siteSections
                .Where(x => !excludedDepartments.Contains(x.Department))
                .Select(x => httpClient.GetCategoriesAsync(x))
                .ToArray();

            await Task.WhenAll(getSiteSubcategories);

            var subCategories = getSiteSubcategories.SelectMany(x => x.Result).ToArray();

            return true;
        }
    }
}