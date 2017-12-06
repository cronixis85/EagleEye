using System;
using System.Collections.Generic;

namespace EagleEye.Extractor.Amazon.Models
{
    public class Product
    {
        public string Name { get; set; }

        public string Asin { get; set; }

        public string Url { get; set; }

        public Uri Uri => new Uri(Url);

        public string Brand { get; set; }

        public decimal? CurrentPrice { get; set; }

        public decimal? OriginalPrice { get; set; }

        public string Dimensions { get; set; }

        public string ItemWeight { get; set; }

        public string ShippingWeight { get; set; }

        public string Manufacturer { get; set; }

        public string ModelNumber { get; set; }

        public float Rating { get; set; }

        public int TotalReviews { get; set; }

        public Dictionary<string, int> Rank { get; set; }

        public DateTime? FirstAvailableOn { get; set; }

        public string Errors { get; set; }

        public List<ProductVariance> Variances { get; set; }
    }
}