using Microsoft.AspNetCore.Mvc;

namespace GsmsharingV2.ViewComponents
{
    public class AdSenseViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string position = "default", string adSlot = "")
        {
            ViewData["AdUnitId"] = "ca-pub-xxxxxxxxxxxxxxxx"; // Replace with your AdSense Publisher ID
            ViewData["AdSlot"] = adSlot;
            ViewData["AdFormat"] = position.Contains("banner") ? "horizontal" : "auto";
            
            return View((object)position);
        }
    }
}

