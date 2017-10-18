using HtmlAgilityPack;

namespace EagleEye.Extractor.Amazon
{
    public abstract class ExecuteBase<T>
    {
        public abstract T ExecuteCore(HtmlDocument doc);

        public T Execute(HtmlDocument doc)
        {
            return ExecuteCore(doc);
        }
    }
}