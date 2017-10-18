using System;
using HtmlAgilityPack;
using Serilog;

namespace EagleEye.Extractor.Amazon
{
    public abstract class ExecuteBase<T>
    {
        public abstract T ExecuteCore(HtmlDocument doc);

        public T Execute(HtmlDocument doc)
        {
            try
            {
                var title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText;

                if (title != null && title.Contains("Robot Check"))
                    throw new ScraperBlockedException("Amazon has blocked scraper.");

                return ExecuteCore(doc);
            }
            catch (Exception e)
            {
                Log.Error(e.Message + " " + e.StackTrace);
                throw;
            }
        }
    }
}