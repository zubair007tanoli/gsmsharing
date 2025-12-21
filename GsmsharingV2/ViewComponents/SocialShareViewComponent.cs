using Microsoft.AspNetCore.Mvc;

namespace GsmsharingV2.ViewComponents
{
    public class SocialShareViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string contentType, int contentId, string title, string url = null)
        {
            var model = new SocialShareViewModel
            {
                ContentType = contentType,
                ContentID = contentId,
                Title = title ?? "",
                Url = url ?? $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}{HttpContext.Request.QueryString}"
            };
            return View(model);
        }
    }

    public class SocialShareViewModel
    {
        public string ContentType { get; set; }
        public int ContentID { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }
}

