using System.Threading.Tasks;
using EagleEye.Models.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace EagleEye.Api.Controllers
{
    [Route("[controller]")]
    public class WebJobsController : Controller
    {
        private readonly AppConfiguration _appConfig;

        public WebJobsController(AppConfiguration appConfig)
        {
            _appConfig = appConfig;
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProducts(bool enableBatch, int batchSize)
        {
            var client = new CloudQueueClient(_appConfig.AzureCloudStorageAccount.QueueStorageUri, _appConfig.AzureCloudStorageAccount.Credentials);

            var queue = client.GetQueueReference(_appConfig.AzureQueueName);

            var message = new CloudQueueMessage(JsonConvert.SerializeObject(new ScrapeSettings
            {
                RebuildDatabase = false,
                UpdateDepartments = false,
                UpdateCategories = false,
                UpdateProducts = false,
                UpdateProductDetails = true,
                UpdateProductVariances = true,
                EnableProductDetailBatchScraping = enableBatch,
                BatchSize = batchSize
            }));

            await queue.AddMessageAsync(message);

            return Ok();
        }
    }
}