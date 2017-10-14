using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EagleEye.Extractor.Amazon.Models
{
    public class Department
    {
        public string Name { get; set; }

        public List<Section> Sections { get; set; } = new List<Section>();
    }
}
