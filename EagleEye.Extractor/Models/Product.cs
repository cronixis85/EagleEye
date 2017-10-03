using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EagleEye.Extractor.Models
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 0)]
        public int Id { get; set; }

        [Column(Order = 2)]
        public string Name { get; set; }

        [Column(Order = 3)]
        public string Url { get; set; }

        [NotMapped]
        public Uri Uri => new Uri(Url);

        public virtual SubCategory SubCategory { get; set; }
    }
}