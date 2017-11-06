using System;
using HtmlAgilityPack;

namespace EagleEye.Extractor.Amazon
{
    public class ScraperBlockedException : Exception
    {
        public HtmlDocument HtmlDocument { get; private set; }

        public ScraperBlockedException(string message, HtmlDocument doc) : base(message)
        {
            HtmlDocument = doc;
        }
    }
}