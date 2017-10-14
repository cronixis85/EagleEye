using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EagleEye.Extractor.Console.Models
{
    public class Department
    {
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(Order = 1)]
        public string Name { get; set; }

        public List<Section> Sections { get; set; }
    }
}