namespace EagleEye.Models.Messages
{
    public class ScrapeSettings
    {
        public bool RebuildDatabase { get; set; }
        public bool UpdateDepartments { get; set; }
        public bool UpdateCategories { get; set; }
        public bool UpdateProducts { get; set; }
        public bool UpdateProductDetails { get; set; }
        public bool UpdateProductVariances { get; set; }
        public bool EnableProductDetailBatchScraping { get; set; }
        public int BatchSize { get; set; }
    }
}