using discussionspot9.Models.ViewModels.HomePage;
using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.Components
{
    public class TrendingTopicsViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var topics = new List<TrendingTopicViewModel>
            {
                new() { Title = "Artificial Intelligence", Slug = "artificial-intelligence", CategoryName = "Technology", ReplyCount = 124 },
                new() { Title = "Machine Learning", Slug = "machine-learning", CategoryName = "Technology", ReplyCount = 87 },
                new() { Title = "Neural Networks", Slug = "neural-networks", CategoryName = "Technology", ReplyCount = 65 },
                new() { Title = "Deep Learning", Slug = "deep-learning", CategoryName = "Technology", ReplyCount = 42 },
                new() { Title = "Computer Vision", Slug = "computer-vision", CategoryName = "Technology", ReplyCount = 31 }
            };

            return View(topics);
        }
    }
}
