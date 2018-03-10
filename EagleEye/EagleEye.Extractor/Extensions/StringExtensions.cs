using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EagleEye.Extractor.Extensions
{
    public static class StringExtensions
    {
        public static string Clean(this string source)
        {
            return source.Replace("\n", "")
                         .Replace("&nbsp;", " ")
                         .Replace("&amp;", "&")
                         .Replace("&#039;", "'")
                         .Replace("&quot;", "\"")
                         .Replace("&gt;", ">")
                         .Trim();
        }
    }
}
