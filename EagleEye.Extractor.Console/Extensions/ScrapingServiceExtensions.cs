using System;
using EagleEye.Extractor.Amazon;
using EagleEye.Extractor.Amazon.Handlers;
using EagleEye.Extractor.Extensions;
using EagleEye.Extractor.Tesseract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace EagleEye.Extractor.Console.Extensions
{
    public static class ScrapingServiceExtensions
    {
        public static IServiceCollection AddScrapingOptions(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile(@"logs\EagleEye.Console-{Date}.txt")
                .CreateLogger();

            services
                .AddLogging()
                .AddSingleton(_ => new ScrapeSettings
                {
                    RebuildDatabase = Convert.ToBoolean(config["ScrapeSettings:RebuildDatabase"]),
                    UpdateDepartments = Convert.ToBoolean(config["ScrapeSettings:UpdateDepartments"]),
                    UpdateCategories = Convert.ToBoolean(config["ScrapeSettings:UpdateCategories"]),
                    UpdateProducts = Convert.ToBoolean(config["ScrapeSettings:UpdateProducts"]),
                    UpdateProductDetails = Convert.ToBoolean(config["ScrapeSettings:UpdateProductDetails"]),
                    UpdateProductVariances = Convert.ToBoolean(config["ScrapeSettings:UpdateProductVariances"])
                })
                .AddSingleton(_ => new RunPythonTesseract(
                    config["Tesseract:Python:Path"],
                    config["Tesseract:Python:CaptchaSolvePath"]))
                .AddSingleton(_ => new RunDotNetTesseract(config["Tesseract:Path"]))
                .AddTransient(_ =>
                {
                    var pipeline = new DefaultHandler()
                        .DecorateWith(new LoggingHandler(Log.Logger));

                    return new AmazonHttpClient(pipeline)
                    {
                        TesseractService = _.GetService<RunDotNetTesseract>()
                    };
                })
                .AddTransient<ScrapingService>();

            return services;
        }
    }
}