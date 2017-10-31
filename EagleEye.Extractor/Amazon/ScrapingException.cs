using System;

namespace EagleEye.Extractor.Amazon
{
    public class ScrapingException : Exception
    {
        public ScrapingException(string message) : base(message)
        {
        }
    }
}