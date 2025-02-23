using gsmsharing.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace gsmsharing.Components
{
    [ViewComponent(Name = "FeaturedTop")]
    public class FeaturedComponents : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {            
            return View();

        }
    }
}
