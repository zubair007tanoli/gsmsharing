// ViewComponents/CommentFormViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using discussionspot9.Models;
using System.Threading.Tasks;

namespace discussionspot9.ViewComponents
{
    public class CommentFormViewComponent : ViewComponent
    {
        private readonly UserManager<IdentityUser> _userManager;

        public CommentFormViewComponent(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(int postId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            ViewData["PostId"] = postId;
            ViewData["UserAvatar"] = user?.ProfilePictureUrl ?? string.Empty;
            ViewData["UserDisplayName"] = user?.DisplayName ?? "User";

            return View();
        }
    }
}