using Microsoft.AspNetCore.Mvc;

namespace Fluxdox.Controllers
{
    public class PricingController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "FluxDoc Pricing Plans";
            return View();
        }
    }
}