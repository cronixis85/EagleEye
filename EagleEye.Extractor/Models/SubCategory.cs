using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EagleEye.Extractor.Models
{
    public class SubCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 0)]
        public int Id { get; set; }

        [Column(Order = 1)]
        public string Department { get; set; }

        [Column(Order = 2)]
        public string Section { get; set; }

        [Column(Order = 3)]
        public string Category { get; set; }

        [Column(Order = 4)]
        public string Name { get; set; }

        [Column(Order = 5)]
        public string Url { get; set; }

        [NotMapped]
        public Uri Uri => new Uri(Url);

        public virtual ICollection<Product> Products { get; set; }
    }
}