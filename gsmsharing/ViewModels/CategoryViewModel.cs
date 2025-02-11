using gsmsharing.Models;

namespace gsmsharing.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }        
        public int? ParentCategoryID { get; set; }      
        public int Level { get; set; }
        public string IconClass { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public int? DisplayOrder { get; set; }
        public int ItemCount { get; set; }
        public List<CategoryViewModel> SubCategories { get; set; } = new List<CategoryViewModel>();
    }
}
