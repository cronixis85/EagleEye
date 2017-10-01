using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EagleEye.Extractor.Models
{
    public class Section
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Department { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        [NotMapped]
        public Uri Uri => new Uri(Url);

        public virtual ICollection<Product> Products { get; set; }
    }
}