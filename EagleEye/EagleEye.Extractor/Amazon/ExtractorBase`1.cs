using System;
using HtmlAgilityPack;
using Serilog;

namespace EagleEye.Extractor.Amazon
{
    public abstract class ExtractorBase<T>
    {
        public abstract T ExecuteCore(HtmlDocument doc);

        public T Execute(HtmlDocument doc)
        {
            try
            {
                return ExecuteCore(doc);
            }
            catch (ScrapingException e)
            {
                Log.Error(e.Message + e.StackTrace + "\n" + doc.DocumentNode.OuterHtml);
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e.Message + e.StackTrace);
                throw;
            }
        }
    }
}