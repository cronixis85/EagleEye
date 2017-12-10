using System;
using System.IO;
using EagleEye.Extractor.Console.Extensions;
using EagleEye.Extractor.Console.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EagleEye.Extractor.Job
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var configurations = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Your classes that contain the webjob methods need to be DI-ed up too
            var services = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(configurations.GetConnectionString("EagleEyeDb")),
                    ServiceLifetime.Transient)
                .AddScrapingOptions(configurations)
                .AddTransient<WebJobMethods>()
                .BuildServiceProvider();

            Environment.SetEnvironmentVariable("AzureWebJobsDashboard", configurations.GetConnectionString("AzureWebJobsDashboard"));
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", configurations.GetConnectionString("AzureWebJobsStorage"));
            
            var host = new JobHost(new JobHostConfiguration()
            {
                JobActivator = new CustomJobActivator(services),
                //DashboardConnectionString = configuration.GetConnectionString("AzureWebJobsDashboard"),
                //StorageConnectionString = configuration.GetConnectionString("AzureWebJobsStorage")
            });

            host.RunAndBlock();
        }
    }

    public class CustomJobActivator : IJobActivator
    {
        private readonly IServiceProvider _service;

        public CustomJobActivator(IServiceProvider service)
        {
            _service = service;
        }

        public T CreateInstance<T>()
        {
            var service = _service.GetService<T>();
            return service;
        }
    }
}