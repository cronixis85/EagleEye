namespace EagleEye.Extractor.Console.Models
{
    public class SubcategoryProduct
    {
        public int SubcategoryId { get; set; }

        public Subcategory Subcategory { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }
    }
}