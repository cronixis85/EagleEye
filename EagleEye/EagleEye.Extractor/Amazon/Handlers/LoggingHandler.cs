using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace EagleEye.Extractor.Amazon.Handlers
{
    public class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger _logger;

        public LoggingHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                // base.SendAsync calls the inner handler
                var response = await base.SendAsync(request, cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to get response: {Url}", request.RequestUri.OriginalString);
                throw;
            }
        }
    }
}