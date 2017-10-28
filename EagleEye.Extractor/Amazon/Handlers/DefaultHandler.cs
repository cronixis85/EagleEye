using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EagleEye.Extractor.Amazon.Handlers
{
    public class DefaultHandler : HttpClientHandler
    {
        public DefaultHandler()
        {
            UseCookies = true;
            AllowAutoRedirect = false;
            CookieContainer = new CookieContainer();
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8
            // Accept-Encoding: gzip, deflate, br
            // Accept-Language: en-US,en;q=0.8
            // User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36

            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.8");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");

            var response = await base.SendAsync(request, cancellationToken);

            //var cookies = CookieContainer.GetCookies(new Uri("http://www.amazon.com"));
            //foreach (Cookie co in cookies)
            //{
            //    co.Expires = co.Expires.AddMinutes();
            //}

            return response;
        }
    }
}