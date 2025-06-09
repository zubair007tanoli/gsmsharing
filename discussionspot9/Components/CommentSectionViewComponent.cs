// ViewComponents/CommentSectionViewComponent.cs
using discussionspot9.Models.ViewModels.CreativeViewModels;
using Microsoft.AspNetCore.Mvc;

public class CommentSectionViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(List<CommentTreeViewModel> comments, int postId)
    {
        ViewData["PostId"] = postId;
        return View();
    }
}