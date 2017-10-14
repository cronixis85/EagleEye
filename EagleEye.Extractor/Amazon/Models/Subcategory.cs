using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EagleEye.Extractor.Amazon.Models
{
    public class Subcategory
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public Uri Uri => new Uri(Url);

        public List<Product> Products { get; set; }
    }
}
