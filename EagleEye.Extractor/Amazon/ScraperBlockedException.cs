using System;

namespace EagleEye.Extractor.Amazon
{
    public class ScraperBlockedException : Exception
    {
        public ScraperBlockedException(string message) : base(message)
        {
        }
    }
}