using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EagleEye.Extractor.Console;
using Microsoft.Azure.WebJobs;

namespace EagleEye.Extractor.Job
{
    public class WebJobMethods
    {
        private readonly ScrapingService _scrapingService;
        private readonly ScrapeSettings _settings;

        public WebJobMethods(ScrapeSettings settings, ScrapingService scrapingService)
        {
            _settings = settings;
            _scrapingService = scrapingService;
        }

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public async Task ProcessQueueMessage([QueueTrigger("amz-jobs")] string message, TextWriter log)
        {
            var cts = new CancellationTokenSource();

            if (_settings.RebuildDatabase)
                await _scrapingService.RebuildDatabaseAsync(cts.Token);

            if (_settings.UpdateDepartments)
                await _scrapingService.UpdateDepartmentalSectionsAsync(cts.Token);

            if (_settings.UpdateCategories)
                await _scrapingService.UpdateCategoriesAsync(cts.Token);

            if (_settings.UpdateProducts)
                await _scrapingService.UpdateProductsAsync(cts.Token);

            if (_settings.UpdateProductDetails)
                await _scrapingService.UpdateProductsDetailsAsync(cts.Token);

            if (_settings.UpdateProductVariances)
                await _scrapingService.UpdateProductVariancesAsync(cts.Token);
        }
    }
}