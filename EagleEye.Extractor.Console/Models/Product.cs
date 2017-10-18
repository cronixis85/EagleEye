using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace EagleEye.Extractor.Console.Models
{
    public class Product
    {
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(Order = 1)]
        public string Asin { get; set; }

        [Column(Order = 2)]
        public string Name { get; set; }

        [Column(Order = 3)]
        public string Url { get; set; }

        public string Brand { get; set; }

        public string Dimensions { get; set; }

        public string ItemWeight { get; set; }

        public string ShippingWeight { get; set; }

        public string Manufacturer { get; set; }

        public string ModelNumber { get; set; }

        public float Rating { get; set; }

        public int TotalReviews { get; set; }

        public DateTime? FirstAvailableOn { get; set; }

        public string Rank { get; set; }

        public void SetRank(Dictionary<string, int> source)
        {
            var rankArr = source
                .Select(x => new
                {
                    category = x.Key,
                    rank = x.Value
                })
                .ToArray();

            Rank = JsonConvert.SerializeObject(rankArr);
        }

        public string Errors { get; set; }

        //public List<SubcategoryProduct> SubcategoryProducts { get; set; }
    }
}