using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EagleEye.Extractor.Console;
using EagleEye.Models.Messages;
using Microsoft.Azure.WebJobs;

namespace EagleEye.Extractor.Job
{
    public class WebJobMethods
    {
        private readonly ScrapingService _scrapingService;

        public WebJobMethods(ScrapingService scrapingService)
        {
            _scrapingService = scrapingService;
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public async Task ProcessQueueMessage([QueueTrigger("%APPSETTING_AzureQueueName%")] ScrapeSettings settings, TextWriter log)
        {
            var cts = new CancellationTokenSource();
            await _scrapingService.RunAsync(settings, cts);
        }
    }
}