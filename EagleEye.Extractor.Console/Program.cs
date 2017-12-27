using System.IO;
using System.Threading;
using EagleEye.Extractor.Console.Extensions;
using EagleEye.Models.Extractor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace EagleEye.Extractor.Console
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);
            
            var config = builder.Build();

            // setup our DI
            var services = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(config["APPSETTING_AzureWebJobs_EagleEyeDb"]), ServiceLifetime.Transient)
                .AddScrapingOptions(config)
                .BuildServiceProvider();

            var settings = services.GetService<ScrapeSettings>();
            var scrapingService = services.GetService<ScrapingService>();
            var cts=  new CancellationTokenSource();

            scrapingService.RunAsync(settings, cts).Wait(cts.Token);

            Log.CloseAndFlush();
        }
    }
}