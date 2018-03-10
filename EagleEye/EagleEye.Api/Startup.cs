using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;

namespace EagleEye.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddOptions();

            services.AddSingleton(x => new AppConfiguration
            {
                AzureCloudStorageAccount = CloudStorageAccount.Parse(Configuration["AzureWebJobsStorage"]),
                AzureQueueName = Configuration["AzureQueueName"]
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseMvc();
        }
    }

    public class AppConfiguration
    {
        public CloudStorageAccount AzureCloudStorageAccount { get; set; }
        public string AzureQueueName { get; set; }
    }
}