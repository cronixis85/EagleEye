using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EagleEye.Extractor.Models
{
    public class Subcategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 0)]
        public int Id { get; set; }

        [Column(Order = 1)]
        [Required]
        public string Name { get; set; }

        [Column(Order = 2)]
        [Required]
        public string Url { get; set; }

        public List<SubcategoryProduct> SubcategoryProducts { get; set; }
    }
}