using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EagleEye.Extractor.Console.Models
{
    public class Section
    {
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public List<Category> Categories { get; set; }
    }
}