using System.ComponentModel.DataAnnotations.Schema;
using EagleEye.Extractor.Console.Models;

namespace EagleEye.Extractor.Models
{
    public class ProductDetail
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Dimension { get; set; }

        public string Weight { get; set; }

        public string ShippingWeight { get; set; }

        public string Manufacturer { get; set; }

        public string ModelNumber { get; set; }

        public float Rating { get; set; }

        public int TotalReviews { get; set; }

        public Product Product { get; set; }
    }
}