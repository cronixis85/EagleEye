using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EagleEye.Extractor.Amazon.Models
{
    public class Category
    {
        public string Name { get; set; }

        public List<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
    }
}
