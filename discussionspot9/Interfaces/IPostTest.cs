using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;

namespace discussionspot9.Interfaces
{
    public interface IPostTest
    {
        Task<CreatePostResult> CreatePostUpdatedAsync(CreatePostViewModel model);
        
    }
}
