using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EagleEye.Extractor.Amazon.Models
{
    public class Section
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public Uri Uri => new Uri(Url);

        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
