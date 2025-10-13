using discussionspot9.Data.DbContext;
using discussionspot9.Models.ViewModels.SearchViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ApplicationDbContext context, ILogger<SearchController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> Index(string? q, string type = "all", int page = 1)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return View(new SearchResultsViewModel { Query = "" });
            }

            const int pageSize = 20;
            var query = q.Trim().ToLower();

            var model = new SearchResultsViewModel
            {
                Query = q,
                CurrentType = type,
                CurrentPage = page
            };

            try
            {
                // Search Posts
                if (type == "all" || type == "posts")
                {
                    var postsQuery = _context.Posts
                        .Where(p => p.Status == "published" &&
                            (p.Title.ToLower().Contains(query) || 
                             (p.Content != null && p.Content.ToLower().Contains(query))))
                        .Include(p => p.Community)
                        .Include(p => p.UserProfile);

                    model.TotalPosts = await postsQuery.CountAsync();

                    if (type == "posts")
                    {
                        model.Posts = await postsQuery
                            .OrderByDescending(p => p.Score)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .Select(p => new SearchPostResult
                            {
                                PostId = p.PostId,
                                Title = p.Title,
                                Slug = p.Slug,
                                Excerpt = p.Content != null ? p.Content.Substring(0, Math.Min(200, p.Content.Length)) : "",
                                CommunityName = p.Community.Name,
                                CommunitySlug = p.Community.Slug,
                                AuthorName = p.UserProfile != null ? p.UserProfile.DisplayName : "Unknown",
                                Score = p.Score,
                                CommentCount = p.CommentCount,
                                CreatedAt = p.CreatedAt
                            })
                            .ToListAsync();
                    }
                    else
                    {
                        model.Posts = await postsQuery
                            .OrderByDescending(p => p.Score)
                            .Take(5)
                            .Select(p => new SearchPostResult
                            {
                                PostId = p.PostId,
                                Title = p.Title,
                                Slug = p.Slug,
                                Excerpt = p.Content != null ? p.Content.Substring(0, Math.Min(150, p.Content.Length)) : "",
                                CommunityName = p.Community.Name,
                                CommunitySlug = p.Community.Slug,
                                AuthorName = p.UserProfile != null ? p.UserProfile.DisplayName : "Unknown",
                                Score = p.Score,
                                CommentCount = p.CommentCount,
                                CreatedAt = p.CreatedAt
                            })
                            .ToListAsync();
                    }
                }

                // Search Communities
                if (type == "all" || type == "communities")
                {
                    var communitiesQuery = _context.Communities
                        .Where(c => !c.IsDeleted &&
                            (c.Name.ToLower().Contains(query) ||
                             (c.Description != null && c.Description.ToLower().Contains(query))));

                    model.TotalCommunities = await communitiesQuery.CountAsync();

                    if (type == "communities")
                    {
                        model.Communities = await communitiesQuery
                            .OrderByDescending(c => c.MemberCount)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .Select(c => new SearchCommunityResult
                            {
                                CommunityId = c.CommunityId,
                                Name = c.Name,
                                Slug = c.Slug,
                                Description = c.ShortDescription ?? c.Description ?? "",
                                MemberCount = c.MemberCount,
                                PostCount = c.PostCount,
                                IconUrl = c.IconUrl
                            })
                            .ToListAsync();
                    }
                    else
                    {
                        model.Communities = await communitiesQuery
                            .OrderByDescending(c => c.MemberCount)
                            .Take(3)
                            .Select(c => new SearchCommunityResult
                            {
                                CommunityId = c.CommunityId,
                                Name = c.Name,
                                Slug = c.Slug,
                                Description = c.ShortDescription ?? c.Description ?? "",
                                MemberCount = c.MemberCount,
                                PostCount = c.PostCount,
                                IconUrl = c.IconUrl
                            })
                            .ToListAsync();
                    }
                }

                // Search Users
                if (type == "all" || type == "users")
                {
                    var usersQuery = _context.UserProfiles
                        .Where(u => u.DisplayName.ToLower().Contains(query) ||
                                    (u.Bio != null && u.Bio.ToLower().Contains(query)));

                    model.TotalUsers = await usersQuery.CountAsync();

                    if (type == "users")
                    {
                        model.Users = await usersQuery
                            .OrderByDescending(u => u.KarmaPoints)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .Select(u => new SearchUserResult
                            {
                                UserId = u.UserId,
                                DisplayName = u.DisplayName,
                                Bio = u.Bio ?? "",
                                KarmaPoints = u.KarmaPoints,
                                IsVerified = u.IsVerified
                            })
                            .ToListAsync();
                    }
                    else
                    {
                        model.Users = await usersQuery
                            .OrderByDescending(u => u.KarmaPoints)
                            .Take(3)
                            .Select(u => new SearchUserResult
                            {
                                UserId = u.UserId,
                                DisplayName = u.DisplayName,
                                Bio = u.Bio ?? "",
                                KarmaPoints = u.KarmaPoints,
                                IsVerified = u.IsVerified
                            })
                            .ToListAsync();
                    }
                }

                model.TotalPages = (int)Math.Ceiling((type switch
                {
                    "posts" => model.TotalPosts,
                    "communities" => model.TotalCommunities,
                    "users" => model.TotalUsers,
                    _ => Math.Max(model.TotalPosts, Math.Max(model.TotalCommunities, model.TotalUsers))
                }) / (double)pageSize);

                // Track search query for analytics
                await TrackSearchAsync(q, model.TotalResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search for query: {Query}", q);
            }

            return View(model);
        }

        [HttpGet]
        [Route("api/search/suggestions")]
        public async Task<IActionResult> GetSuggestions(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Json(new { suggestions = new List<object>() });
            }

            var query = q.Trim().ToLower();
            var suggestions = new List<object>();

            try
            {
                // Get top 3 matching posts
                var posts = await _context.Posts
                    .Where(p => p.Status == "published" && p.Title.ToLower().Contains(query))
                    .OrderByDescending(p => p.Score)
                    .Take(3)
                    .Select(p => new
                    {
                        type = "post",
                        title = p.Title,
                        url = $"/r/{p.Community.Slug}/posts/{p.Slug}",
                        icon = "📝",
                        meta = $"{p.CommentCount} comments"
                    })
                    .ToListAsync();

                suggestions.AddRange(posts);

                // Get top 2 matching communities
                var communities = await _context.Communities
                    .Where(c => !c.IsDeleted && c.Name.ToLower().Contains(query))
                    .OrderByDescending(c => c.MemberCount)
                    .Take(2)
                    .Select(c => new
                    {
                        type = "community",
                        title = $"r/{c.Name}",
                        url = $"/r/{c.Slug}",
                        icon = "👥",
                        meta = $"{c.MemberCount} members"
                    })
                    .ToListAsync();

                suggestions.AddRange(communities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions");
            }

            return Json(new { suggestions });
        }

        private async Task TrackSearchAsync(string query, int resultsCount)
        {
            try
            {
                // Track search in UserActivities for analytics
                var userId = User.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;
                
                _context.UserActivities.Add(new Models.Domain.UserActivity
                {
                    UserId = userId,
                    ActivityType = "Search",
                    TimeSpentSeconds = 0,
                    ScrollDepthPercent = 0,
                    ActivityAt = DateTime.UtcNow,
                    Metadata = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        Query = query,
                        ResultsCount = resultsCount
                    })
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to track search query");
            }
        }
    }
}

