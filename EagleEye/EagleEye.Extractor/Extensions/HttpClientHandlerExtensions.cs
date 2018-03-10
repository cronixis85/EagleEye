using System.Net.Http;

namespace EagleEye.Extractor.Extensions
{
    public static class HttpClientHandlerExtensions
    {
        public static DelegatingHandler DecorateWith(this DelegatingHandler handler, DelegatingHandler outerHandler)
        {
            outerHandler.InnerHandler = handler;
            return outerHandler;
        }

        public static DelegatingHandler DecorateWith(this HttpMessageHandler handler, DelegatingHandler outerHandler)
        {
            outerHandler.InnerHandler = handler;
            return outerHandler;
        }
    }
}