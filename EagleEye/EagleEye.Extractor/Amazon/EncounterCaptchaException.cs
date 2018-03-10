using System;
using HtmlAgilityPack;

namespace EagleEye.Extractor.Amazon
{
    public class EncounterCaptchaException : Exception
    {
        public Uri Uri { get; private set; }

        public HtmlDocument HtmlDocument { get; private set; }

        public EncounterCaptchaException(string message, Uri uri, HtmlDocument doc) : base(message)
        {
            Uri = uri;
            HtmlDocument = doc;
        }
    }
}