using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.Components
{
    public class BreadcrumbViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<BreadcrumbItem>? items = null)
        {
            // Auto-generate breadcrumbs if not provided
            if (items == null || !items.Any())
            {
                items = GenerateBreadcrumbsFromRoute();
            }

            return View(items);
        }

        private List<BreadcrumbItem> GenerateBreadcrumbsFromRoute()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Url = "/" }
            };

            var routeData = ViewContext.RouteData.Values;
            var controller = routeData["controller"]?.ToString();
            var action = routeData["action"]?.ToString();

            // Add controller-based breadcrumbs
            if (!string.IsNullOrEmpty(controller) && controller != "Home")
            {
                breadcrumbs.Add(new BreadcrumbItem
                {
                    Text = controller,
                    Url = $"/{controller}"
                });
            }

            // Add action-based breadcrumb if it's not Index
            if (!string.IsNullOrEmpty(action) && action != "Index")
            {
                breadcrumbs.Add(new BreadcrumbItem
                {
                    Text = action,
                    Url = null // Current page, no link
                });
            }

            return breadcrumbs;
        }
    }

    public class BreadcrumbItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string? Icon { get; set; }
    }
}

