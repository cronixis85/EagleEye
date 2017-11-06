using System;
using HtmlAgilityPack;

namespace EagleEye.Extractor.Amazon
{
    public class EncounterCaptchaException : Exception
    {
        public HtmlDocument HtmlDocument { get; private set; }

        public EncounterCaptchaException(string message, HtmlDocument doc) : base(message)
        {
            HtmlDocument = doc;
        }
    }
}