using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using EagleEye.Extractor.Models;

namespace EagleEye.Extractor.Console.Models
{
    public class Category
    {
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(Order = 1)]
        public string Name { get; set; }

        public List<Subcategory> Subcategories { get; set; }
    }
}