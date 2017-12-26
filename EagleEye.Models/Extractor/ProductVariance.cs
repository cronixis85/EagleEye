using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EagleEye.Models.Extractor
{
    public class ProductVariance
    {
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Asin { get; set; }

        public decimal? CurrentPrice { get; set; }

        public decimal? OriginalPrice { get; set; }

        public string Status { get; set; }

        public string Url { get; set; }

        public Uri Uri => new Uri(Url);

        public DateTime UpdatedOn { get; set; }

        public string Errors { get; set; }
    }
}