using Microsoft.AspNetCore.Mvc;

namespace gsmsharing.Controllers
{
    public class BaseController : Controller
    {
        protected void SetSEOMetadata(string title, string description, string ogTitle = null, string ogDescription = null, string ogImage = null)
        {
            ViewData["Title"] = title;
            ViewData["MetaDescription"] = description;
            ViewData["OgTitle"] = ogTitle ?? title;
            ViewData["OgDescription"] = ogDescription ?? description;
            ViewData["OgImage"] = ogImage;
            ViewData["CanonicalUrl"] = GetCanonicalUrl();

            // Set Twitter card data
            ViewData["TwitterTitle"] = ogTitle ?? title;
            ViewData["TwitterDescription"] = ogDescription ?? description;
            ViewData["TwitterImage"] = ogImage;
        }

        private string GetCanonicalUrl()
        {
            return $"{Request.Scheme}://{Request.Host}{Request.Path}";
        }
    }
}
