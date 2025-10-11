// ViewComponents/RightSidebarViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace discussionspot9.Components
{
    public class RightSidebarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RightSidebarViewComponent> _logger;

        public RightSidebarViewComponent(ApplicationDbContext context, ILogger<RightSidebarViewComponent> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? currentPostId = null, string? communitySlug = null)
        {
            _logger.LogInformation("🔵 RightSidebarViewComponent invoked with PostId: {PostId}, CommunitySlug: {CommunitySlug}", 
                currentPostId?.ToString() ?? "null", 
                communitySlug ?? "null");
            
            var viewModel = new RightSidebarViewModel
            {
                RelatedPosts = new List<RelatedPostViewModel>()
            };

            try
            {
                // Get related posts based on community or tags
                if (currentPostId.HasValue)
                {
                    var currentPost = await _context.Posts
                        .Include(p => p.PostTags)
                            .ThenInclude(pt => pt.Tag)
                        .Include(p => p.Community)
                        .FirstOrDefaultAsync(p => p.PostId == currentPostId.Value);

                    if (currentPost != null)
                    {
                        // Get posts from the same community
                        var relatedQuery = _context.Posts
                            .Where(p => p.Status == "published" 
                                && p.PostId != currentPostId.Value 
                                && p.CommunityId == currentPost.CommunityId);

                        // If post has tags, prioritize posts with similar tags
                        var tagIds = currentPost.PostTags?.Select(pt => pt.TagId).ToList() ?? new List<int>();
                        if (tagIds.Any())
                        {
                            relatedQuery = relatedQuery
                                .Where(p => p.PostTags.Any(pt => tagIds.Contains(pt.TagId)));
                        }

                        viewModel.RelatedPosts = await relatedQuery
                            .OrderByDescending(p => p.Score)
                            .Take(5)
                            .Select(p => new RelatedPostViewModel
                            {
                                PostId = p.PostId,
                                Title = p.Title,
                                Slug = p.Slug,
                                CommunitySlug = p.Community.Slug,
                                CommunityName = p.Community.Name,
                                UpvoteCount = p.UpvoteCount,
                                CommentCount = p.CommentCount,
                                PostType = p.PostType
                            })
                            .ToListAsync();
                    }
                }
                else if (!string.IsNullOrEmpty(communitySlug))
                {
                    // Get popular posts from the community
                    viewModel.RelatedPosts = await _context.Posts
                        .Where(p => p.Status == "published" && p.Community.Slug == communitySlug)
                        .OrderByDescending(p => p.Score)
                        .Take(5)
                        .Select(p => new RelatedPostViewModel
                        {
                            PostId = p.PostId,
                            Title = p.Title,
                            Slug = p.Slug,
                            CommunitySlug = p.Community.Slug,
                            CommunityName = p.Community.Name,
                            UpvoteCount = p.UpvoteCount,
                            CommentCount = p.CommentCount,
                            PostType = p.PostType
                        })
                        .ToListAsync();
                }

                // If no related posts found, get trending posts
                if (!viewModel.RelatedPosts.Any())
                {
                    viewModel.RelatedPosts = await _context.Posts
                        .Where(p => p.Status == "published" && p.CreatedAt >= DateTime.Now.AddDays(-7))
                        .OrderByDescending(p => p.Score)
                        .Take(5)
                        .Select(p => new RelatedPostViewModel
                        {
                            PostId = p.PostId,
                            Title = p.Title,
                            Slug = p.Slug,
                            CommunitySlug = p.Community.Slug,
                            CommunityName = p.Community.Name,
                            UpvoteCount = p.UpvoteCount,
                            CommentCount = p.CommentCount,
                            PostType = p.PostType
                        })
                        .ToListAsync();
                }
                
                _logger.LogInformation("✅ RightSidebar: Found {PostCount} related posts", viewModel.RelatedPosts.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in RightSidebarViewComponent");
                // Return empty list on error
                viewModel.RelatedPosts = new List<RelatedPostViewModel>();
            }

            return View(viewModel);
        }
    }
}