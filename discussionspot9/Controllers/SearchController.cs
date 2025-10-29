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
        public async Task<IActionResult> Index(
            string? q, 
            string type = "all",
            string sort = "relevance",
            string postType = "all",
            bool hasMedia = false,
            string timeRange = "all",
            int minKarma = 0,
            int minScore = 0,
            bool verifiedOnly = false,
            int page = 1)
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
                CurrentPage = page,
                SortBy = sort,
                PostType = postType,
                HasMedia = hasMedia,
                TimeRange = timeRange,
                MinKarma = minKarma,
                MinScore = minScore,
                VerifiedOnly = verifiedOnly
            };

            try
            {
                // Search Posts with advanced filters
                if (type == "all" || type == "posts")
                {
                    var postsQuery = _context.Posts
                        .Where(p => p.Status == "published" &&
                            (p.Title.ToLower().Contains(query) || 
                             (p.Content != null && p.Content.ToLower().Contains(query))))
                        .Include(p => p.Community)
                        .Include(p => p.UserProfile)
                        .AsQueryable();

                    // Apply post type filter
                    if (postType != "all")
                    {
                        postsQuery = postsQuery.Where(p => p.PostType == postType);
                    }

                    // Apply media filter
                    if (hasMedia)
                    {
                        var postsWithMedia = _context.Media
                            .Select(m => m.PostId)
                            .Distinct();
                        postsQuery = postsQuery.Where(p => postsWithMedia.Contains(p.PostId));
                    }

                    // Apply time range filter
                    if (timeRange != "all")
                    {
                        var cutoffDate = GetTimeRangeCutoff(timeRange);
                        postsQuery = postsQuery.Where(p => p.CreatedAt >= cutoffDate);
                    }

                    // Apply score filter
                    if (minScore > 0)
                    {
                        postsQuery = postsQuery.Where(p => p.Score >= minScore);
                    }

                    // Apply sorting
                    postsQuery = sort switch
                    {
                        "new" => postsQuery.OrderByDescending(p => p.CreatedAt),
                        "hot" => postsQuery.OrderByDescending(p => p.Score)
                                          .ThenByDescending(p => p.CreatedAt),
                        "top" => postsQuery.OrderByDescending(p => p.UpvoteCount - p.DownvoteCount),
                        _ => postsQuery.OrderByDescending(p => p.Score) // relevance
                    };

                    model.TotalPosts = await postsQuery.CountAsync();

                    if (type == "posts")
                    {
                        model.Posts = await postsQuery
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
                                AuthorKarma = p.UserProfile != null ? p.UserProfile.KarmaPoints : 0,
                                Score = p.Score,
                                CommentCount = p.CommentCount,
                                CreatedAt = p.CreatedAt,
                                PostType = p.PostType,
                                ThumbnailUrl = p.Media.Any() ? p.Media.First().ThumbnailUrl : null,
                                LinkPreviewDomain = null
                            })
                            .ToListAsync();
                    }
                    else
                    {
                        model.Posts = await postsQuery
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
                                AuthorKarma = p.UserProfile != null ? p.UserProfile.KarmaPoints : 0,
                                Score = p.Score,
                                CommentCount = p.CommentCount,
                                CreatedAt = p.CreatedAt,
                                PostType = p.PostType,
                                ThumbnailUrl = p.Media.Any() ? p.Media.First().ThumbnailUrl : null,
                                LinkPreviewDomain = null
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

                // Search Users with filters
                if (type == "all" || type == "users")
                {
                    var usersQuery = _context.UserProfiles
                        .Where(u => u.DisplayName.ToLower().Contains(query) ||
                                    (u.Bio != null && u.Bio.ToLower().Contains(query)))
                        .AsQueryable();

                    // Apply karma filter
                    if (minKarma > 0)
                    {
                        usersQuery = usersQuery.Where(u => u.KarmaPoints >= minKarma);
                    }

                    // Apply verified filter
                    if (verifiedOnly)
                    {
                        usersQuery = usersQuery.Where(u => u.IsVerified);
                    }

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

        private DateTime GetTimeRangeCutoff(string timeRange)
        {
            return timeRange switch
            {
                "hour" => DateTime.UtcNow.AddHours(-1),
                "day" => DateTime.UtcNow.AddDays(-1),
                "week" => DateTime.UtcNow.AddDays(-7),
                "month" => DateTime.UtcNow.AddMonths(-1),
                "year" => DateTime.UtcNow.AddYears(-1),
                _ => DateTime.MinValue
            };
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

