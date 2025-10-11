// ViewComponents/LeftSidebarViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;

namespace discussionspot9.Components
{
    [ViewComponent(Name = "LeftSideBar")]
    public class LeftSidebarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        private readonly ILogger<LeftSidebarViewComponent> _logger;

        public LeftSidebarViewComponent(ApplicationDbContext context, ILogger<LeftSidebarViewComponent> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(string? communitySlug = null)
        {
            _logger.LogInformation("🔵 LeftSidebarViewComponent invoked with communitySlug: {CommunitySlug}", communitySlug ?? "null");
            
            var viewModel = new LeftSidebarViewModel
            {
                CurrentCommunitySlug = communitySlug,
                LatestNews = new List<NewsItemViewModel>(),
                TodayStats = new TodayStatsViewModel()
            };

            try
            {
                // Get latest news (most recent posts from last 24 hours)
                var yesterday = DateTime.Now.AddDays(-1);
                viewModel.LatestNews = await _context.Posts
                    .Where(p => p.Status == "published" && p.CreatedAt >= yesterday)
                    .OrderByDescending(p => p.Score)
                    .Take(4)
                    .Select(p => new NewsItemViewModel
                    {
                        Title = p.Title,
                        Slug = p.Slug,
                        CommunitySlug = p.Community.Slug,
                        Category = p.Community.Category != null ? p.Community.Category.Name : "General",
                        TimeAgo = GetTimeAgo(p.CreatedAt)
                    })
                    .ToListAsync();

                // Get today's stats
                var today = DateTime.Today;
                viewModel.TodayStats = new TodayStatsViewModel
                {
                    NewPostsCount = await _context.Posts
                        .Where(p => p.CreatedAt >= today && p.Status == "published")
                        .CountAsync(),
                    
                    ActiveUsersCount = await _context.UserProfiles
                        .Where(u => u.LastActive >= today)
                        .CountAsync(),
                    
                    CommentsCount = await _context.Comments
                        .Where(c => c.CreatedAt >= today && !c.IsDeleted)
                        .CountAsync()
                };
                
                _logger.LogInformation("✅ LeftSidebar: Found {NewsCount} news items and stats (Posts: {Posts}, Users: {Users}, Comments: {Comments})", 
                    viewModel.LatestNews.Count, 
                    viewModel.TodayStats.NewPostsCount, 
                    viewModel.TodayStats.ActiveUsersCount, 
                    viewModel.TodayStats.CommentsCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in LeftSidebarViewComponent");
                // Return empty data on error
                viewModel.LatestNews = new List<NewsItemViewModel>();
                viewModel.TodayStats = new TodayStatsViewModel();
            }

            return View(viewModel);
        }

        private string GetTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.Now - createdAt;
            if (timeSpan.TotalMinutes < 1) return "just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
            return createdAt.ToString("MMM dd");
        }
    }
}