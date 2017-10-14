using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EagleEye.Extractor.Models;

namespace EagleEye.Extractor.Console.Models
{
    public class Product
    {
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(Order = 1)]
        [Required]
        public string Asin { get; set; }

        [Column(Order = 2)]
        [Required]
        public string Name { get; set; }

        [Column(Order = 3)]
        [Required]
        public string Url { get; set; }
        
        public List<SubcategoryProduct> SubcategoryProducts { get; set; }
    }
}