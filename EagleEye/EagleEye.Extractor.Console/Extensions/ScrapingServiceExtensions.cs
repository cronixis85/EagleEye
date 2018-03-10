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
            
            var loggingConfiguration = 
                new LoggerConfiguration()
                    .WriteTo.LiterateConsole();

            if (Convert.ToBoolean(config["APPSETTING_Logging_EnableRollingFile"]))
                loggingConfiguration.WriteTo.RollingFile(@"logs\EagleEye.Job-{Date}.txt");

            Log.Logger = loggingConfiguration.CreateLogger();

            services
                .AddLogging()
                .AddSingleton(_ => new RunPythonTesseract(
                    config["APPSETTING_Tesseract_Python_Path"],
                    config["APPSETTING_Tesseract_Python_CaptchaSolvePath"]))
                .AddSingleton(_ => new RunDotNetTesseract(config["APPSETTING_Tesseract_Path"], config["APPSETTING_Tesseract_TempDir"]))
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