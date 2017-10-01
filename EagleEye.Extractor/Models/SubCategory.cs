using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EagleEye.Extractor.Models
{
    public class SubCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Department { get; set; }

        public string Section { get; set; }

        public string Category { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        [NotMapped]
        public Uri Uri => new Uri(Url);
    }
}