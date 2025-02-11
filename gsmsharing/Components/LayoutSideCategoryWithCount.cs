using gsmsharing.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace gsmsharing.Components
{
    [ViewComponent(Name = "LayoutSideCategoryCout")]
    public class LayoutSideCategoryWithCount :ViewComponent
    {
        private ICategoryRepository categoryRepository;
        public LayoutSideCategoryWithCount(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //CategoryFunctions Cate = new CategoryFunctions();
            //var categories = await Cate.GetHierarchicalCategories();
            //return View(categories);
            var categories = await categoryRepository.GetHierarchicalCategoriesTestAsync();
            return View(categories);

        }
    }
}
