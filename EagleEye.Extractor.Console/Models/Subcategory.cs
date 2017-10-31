using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EagleEye.Extractor.Console.Models
{
    public class Subcategory
    {
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(Order = 1)]
        [Required]
        public string Name { get; set; }

        [Column(Order = 2)]
        [Required]
        public string Url { get; set; }

        public Uri Uri => new Uri(Url);

        public bool Enabled { get; set; }

        public List<Product> Products { get; set; }

        //public List<SubcategoryProduct> SubcategoryProducts { get; set; }
    }
}