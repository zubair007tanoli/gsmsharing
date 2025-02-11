using gsmsharing.Interfaces;
using gsmsharing.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace gsmsharing.Components
{
    [ViewComponent(Name ="LayoutSideCategory")]
    public class LayoutSideCategory : ViewComponent
    {
        private ICategoryRepository categoryRepository;
        public LayoutSideCategory(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //CategoryFunctions Cate = new CategoryFunctions();
            //var categories = await Cate.GetHierarchicalCategories();
            //return View(categories);
            var categories = await categoryRepository.GetHierarchicalCategoriesAsync();
            return View(categories);

        }
    }
}
