using System;
using System.IO;
using System.Linq;
using EagleEye.Extractor.Console.Extensions;
using EagleEye.Extractor.Console.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace EagleEye.Extractor.Job
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // print env variables
            //var env = Environment.GetEnvironmentVariables();
            //Log.Information("Envrionment Variables:");
            //Log.Information(JsonConvert.SerializeObject(env));

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables();
            //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            var configurations = builder.Build();

            // Your classes that contain the webjob methods need to be DI-ed up too
            var services = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configurations["SQLAZURECONNSTR_EagleEyeDb"]), ServiceLifetime.Transient)
                .AddScrapingOptions(configurations)
                .AddTransient<WebJobMethods>()
                .BuildServiceProvider();

            Log.Information("App Settings & Environment Variables:");
            foreach (var child in configurations.GetChildren().OrderBy(x => x.Key))
            {
                Log.Information($"{child.Key} = {child.Value}");
            }

            var host = new JobHost(new JobHostConfiguration
            {
                DashboardConnectionString = configurations["APPSETTING_AzureWebJobsDashboard"],
                StorageConnectionString = configurations["APPSETTING_AzureWebJobsStorage"],
                JobActivator = new CustomJobActivator(services),
                Queues =
                {
                    MaxDequeueCount = 1,
                    BatchSize = 1,
                    MaxPollingInterval = TimeSpan.FromSeconds(30),
                    NewBatchThreshold = 1
                }
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