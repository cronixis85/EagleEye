using Microsoft.EntityFrameworkCore;

namespace EagleEye.Extractor.Console.Models
{
    internal class ApplicationDbContext : DbContext
    {
        public DbSet<Department> Departments { get; set; }

        public DbSet<Section> Sections { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Subcategory> Subcategories { get; set; }

        public DbSet<Product> Products { get; set; }

        //public DbSet<ProductDetail> ProductDetails { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<SubcategoryProduct>()
            //            .HasKey(x => new {x.SubcategoryId, x.ProductId});

            //modelBuilder.Entity<SubcategoryProduct>()
            //            .HasOne(x => x.Subcategory)
            //            .WithMany(x => x.SubcategoryProducts)
            //            .HasForeignKey(x => x.SubcategoryId);

            //modelBuilder.Entity<SubcategoryProduct>()
            //            .HasOne(x => x.Product)
            //            .WithMany(x => x.SubcategoryProducts)
            //            .HasForeignKey(x => x.ProductId);

            //// index
            //modelBuilder.Entity<Product>()
            //            .HasIndex(b => b.Asin)
            //            .IsUnique()
            //            .HasName("IX_ASIN");
        }
    }
}