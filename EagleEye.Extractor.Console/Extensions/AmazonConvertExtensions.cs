using System.Collections.Generic;
using System.Linq;
using EagleEye.Extractor.Console.Models;
using EagleEye.Extractor.Models;

namespace EagleEye.Extractor.Console.Extensions
{
    public static class AmazonConvertExtensions
    {
        public static IEnumerable<Department> ToDbDepartments(this IEnumerable<Amazon.Models.Department> source)
        {
            return source.Select(x => x.ToDbDepartment());
        }

        public static Department ToDbDepartment(this Amazon.Models.Department source)
        {
            return new Department
            {
                Name = source.Name,
                Sections = source.Sections?.ToDbSections().ToList()
            };
        }

        public static IEnumerable<Section> ToDbSections(this IEnumerable<Amazon.Models.Section> source)
        {
            return source.Select(x => x.ToDbSection());
        }

        public static Section ToDbSection(this Amazon.Models.Section source)
        {
            return new Section
            {
                Name = source.Name,
                Url = source.Url,
                Categories = source.Categories?.ToDbCategories().ToList()
            };
        }

        public static IEnumerable<Category> ToDbCategories(this IEnumerable<Amazon.Models.Category> source)
        {
            return source.Select(x => x.ToDbCategory());
        }

        public static Category ToDbCategory(this Amazon.Models.Category source)
        {
            return new Category
            {
                Name = source.Name,
                Subcategories = source.Subcategories?.ToDbSubcategories().ToList()
            };
        }

        public static IEnumerable<Subcategory> ToDbSubcategories(this IEnumerable<Amazon.Models.Subcategory> source)
        {
            return source.Select(x => x.ToDbSubcategory());
        }

        public static Subcategory ToDbSubcategory(this Amazon.Models.Subcategory source)
        {
            return new Subcategory
            {
                Name = source.Name,
                Url = source.Url
            };
        }
    }
}