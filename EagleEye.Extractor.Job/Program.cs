using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EagleEye.Extractor.Job
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
                config.UseDevelopmentSettings();

            var host = new JobHost(config);
            host.RunAndBlock();
        }
        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Setup your container here, just like a asp.net core app

            // Optional: Setup your configuration:
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            //serviceCollection.Configure<MySettings>(configuration);

            //// A silly example of wiring up some class used by the web job:
            //serviceCollection.AddScoped<ISomeInterface, SomeUsefulClass>();
            //// Your classes that contain the webjob methods need to be DI-ed up too
            //serviceCollection.AddScoped<WebJobsMethods, WebJobsMethods>();

            // One more thing - tell azure where your azure connection strings are
            Environment.SetEnvironmentVariable("AzureWebJobsDashboard", configuration.GetConnectionString("WebJobsDashboard"));
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", configuration.GetConnectionString("WebJobsStorage"));
        }
    }
}