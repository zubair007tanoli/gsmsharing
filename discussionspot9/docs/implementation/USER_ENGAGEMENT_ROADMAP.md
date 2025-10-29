# 🚀 User Engagement Features - Implementation Roadmap

**Date:** October 29, 2025  
**Duration:** 4 weeks  
**Priority:** HIGH - Directly impacts user retention & engagement

---

## 📋 **OVERVIEW**

This roadmap implements 6 major engagement features to transform DiscussionSpot9 from a functional platform to an **addictive, user-focused community**.

### **Features to Implement:**
1. ✅ **Karma System** (Week 1) - Gamification foundation
2. ✅ **Badge & Achievements** (Week 1) - Recognition & collection
3. ✅ **Rich Link Previews in Feed** (Week 2) - Visual appeal
4. ✅ **Enhanced Search** (Week 2) - Better discovery
5. ✅ **Follow System UI** (Week 3) - Social connections
6. ✅ **Stories Updates** (Week 3-4) - Content engagement

---

## 🎯 **PHASE 1: KARMA & BADGES** (Week 1)

### **Priority: CRITICAL** 
*Gamification drives 80% of Reddit's user retention*

---

### 📊 **TASK 1.1: Karma System Integration**

**Status:** 🟡 Partial - KarmaPoints field exists but not integrated

#### **What Exists:**
```csharp
// discussionspot9/Models/Domain/UserProfile.cs
public int KarmaPoints { get; set; }

// discussionspot9/Repositories/UserService.cs (partial methods exist)
await UpdateKarmaPointsAsync(string userId, int points)
```

#### **What's Needed:**

##### **Step 1: Create Comprehensive Karma Service**
**File:** `discussionspot9/Services/KarmaService.cs` (NEW)

```csharp
using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public interface IKarmaService
    {
        Task<int> CalculateUserKarmaAsync(string userId);
        Task UpdatePostKarmaAsync(int postId, int voteType);
        Task UpdateCommentKarmaAsync(int commentId, int voteType);
        Task AwardKarmaAsync(string userId, int karma, string reason);
        Task<Dictionary<string, int>> GetKarmaBreakdownAsync(string userId);
        Task<List<UserKarmaViewModel>> GetKarmaLeaderboardAsync(string period, int limit);
    }

    public class KarmaService : IKarmaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KarmaService> _logger;

        // Karma calculation rules
        private const int POST_UPVOTE_KARMA = 1;
        private const int POST_DOWNVOTE_KARMA = -1;
        private const int COMMENT_UPVOTE_KARMA = 1;
        private const int COMMENT_DOWNVOTE_KARMA = -1;
        private const int AWARD_RECEIVED_KARMA = 10;
        private const int BEST_ANSWER_KARMA = 15;
        private const int POST_CREATED_KARMA = 1;

        public KarmaService(
            ApplicationDbContext context, 
            ILogger<KarmaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Calculate total karma for a user from all sources
        /// </summary>
        public async Task<int> CalculateUserKarmaAsync(string userId)
        {
            try
            {
                // Get user's posts
                var userPosts = await _context.Posts
                    .Where(p => p.UserId == userId && p.Status == "published")
                    .ToListAsync();

                // Calculate post karma (upvotes - downvotes)
                var postKarma = userPosts.Sum(p => 
                    (p.UpvoteCount * POST_UPVOTE_KARMA) + 
                    (p.DownvoteCount * POST_DOWNVOTE_KARMA));

                // Get user's comments
                var userComments = await _context.Comments
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                // Calculate comment karma
                var commentKarma = userComments.Sum(c => 
                    (c.UpvoteCount * COMMENT_UPVOTE_KARMA) + 
                    (c.DownvoteCount * COMMENT_DOWNVOTE_KARMA));

                // Get awards received
                var postAwardKarma = await _context.PostAwards
                    .Where(pa => userPosts.Select(p => p.PostId).Contains(pa.PostId))
                    .SumAsync(pa => AWARD_RECEIVED_KARMA);

                var commentAwardKarma = await _context.CommentAwards
                    .Where(ca => userComments.Select(c => c.CommentId).Contains(ca.CommentId))
                    .SumAsync(ca => AWARD_RECEIVED_KARMA);

                // Post creation bonus (1 karma per post)
                var postCreationKarma = userPosts.Count * POST_CREATED_KARMA;

                var totalKarma = postKarma + commentKarma + postAwardKarma + 
                                commentAwardKarma + postCreationKarma;

                _logger.LogInformation(
                    "Karma calculated for user {UserId}: Post={PostKarma}, Comment={CommentKarma}, " +
                    "PostAwards={PostAwardKarma}, CommentAwards={CommentAwardKarma}, Creation={PostCreationKarma}, Total={TotalKarma}",
                    userId, postKarma, commentKarma, postAwardKarma, commentAwardKarma, postCreationKarma, totalKarma);

                return totalKarma;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating karma for user {UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// Update author's karma when their post gets voted
        /// </summary>
        public async Task UpdatePostKarmaAsync(int postId, int voteType)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.UserProfile)
                    .FirstOrDefaultAsync(p => p.PostId == postId);

                if (post?.UserId == null) return;

                var karmaChange = voteType == 1 ? POST_UPVOTE_KARMA : POST_DOWNVOTE_KARMA;
                
                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == post.UserId);

                if (profile != null)
                {
                    profile.KarmaPoints += karmaChange;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation(
                        "Karma updated for user {UserId}: {Change} (Post vote)", 
                        post.UserId, karmaChange);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post karma for postId {PostId}", postId);
            }
        }

        /// <summary>
        /// Update author's karma when their comment gets voted
        /// </summary>
        public async Task UpdateCommentKarmaAsync(int commentId, int voteType)
        {
            try
            {
                var comment = await _context.Comments
                    .FirstOrDefaultAsync(c => c.CommentId == commentId);

                if (comment?.UserId == null) return;

                var karmaChange = voteType == 1 ? COMMENT_UPVOTE_KARMA : COMMENT_DOWNVOTE_KARMA;
                
                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == comment.UserId);

                if (profile != null)
                {
                    profile.KarmaPoints += karmaChange;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation(
                        "Karma updated for user {UserId}: {Change} (Comment vote)", 
                        comment.UserId, karmaChange);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment karma for commentId {CommentId}", commentId);
            }
        }

        /// <summary>
        /// Award karma manually (admin, achievements, etc.)
        /// </summary>
        public async Task AwardKarmaAsync(string userId, int karma, string reason)
        {
            try
            {
                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile != null)
                {
                    profile.KarmaPoints += karma;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation(
                        "Karma awarded to user {UserId}: {Karma} - Reason: {Reason}", 
                        userId, karma, reason);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding karma to user {UserId}", userId);
            }
        }

        /// <summary>
        /// Get detailed karma breakdown for display
        /// </summary>
        public async Task<Dictionary<string, int>> GetKarmaBreakdownAsync(string userId)
        {
            var breakdown = new Dictionary<string, int>();

            try
            {
                var userPosts = await _context.Posts
                    .Where(p => p.UserId == userId && p.Status == "published")
                    .ToListAsync();

                breakdown["PostUpvotes"] = userPosts.Sum(p => p.UpvoteCount);
                breakdown["PostDownvotes"] = Math.Abs(userPosts.Sum(p => p.DownvoteCount));
                breakdown["PostKarma"] = userPosts.Sum(p => 
                    (p.UpvoteCount * POST_UPVOTE_KARMA) + (p.DownvoteCount * POST_DOWNVOTE_KARMA));

                var userComments = await _context.Comments
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                breakdown["CommentUpvotes"] = userComments.Sum(c => c.UpvoteCount);
                breakdown["CommentDownvotes"] = Math.Abs(userComments.Sum(c => c.DownvoteCount));
                breakdown["CommentKarma"] = userComments.Sum(c => 
                    (c.UpvoteCount * COMMENT_UPVOTE_KARMA) + (c.DownvoteCount * COMMENT_DOWNVOTE_KARMA));

                breakdown["TotalPosts"] = userPosts.Count;
                breakdown["TotalComments"] = userComments.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting karma breakdown for user {UserId}", userId);
            }

            return breakdown;
        }

        /// <summary>
        /// Get karma leaderboard (top users)
        /// </summary>
        public async Task<List<UserKarmaViewModel>> GetKarmaLeaderboardAsync(string period, int limit = 10)
        {
            try
            {
                var query = _context.UserProfiles.AsQueryable();

                // Filter by time period if needed
                // For now, all-time leaderboard
                
                var leaders = await query
                    .OrderByDescending(u => u.KarmaPoints)
                    .Take(limit)
                    .Select(u => new UserKarmaViewModel
                    {
                        UserId = u.UserId,
                        DisplayName = u.DisplayName,
                        KarmaPoints = u.KarmaPoints,
                        AvatarUrl = u.AvatarUrl,
                        IsVerified = u.IsVerified
                    })
                    .ToListAsync();

                return leaders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching karma leaderboard");
                return new List<UserKarmaViewModel>();
            }
        }
    }

    // ViewModel for leaderboard
    public class UserKarmaViewModel
    {
        public string UserId { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public int KarmaPoints { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsVerified { get; set; }
        public int Rank { get; set; }
        public string KarmaLevel { get; set; } = "Newbie";
    }
}
```

##### **Step 2: Integrate Karma into Voting**

**Update:** `discussionspot9/Controllers/VoteController.cs` or where voting happens

```csharp
public class VoteController : Controller
{
    private readonly IKarmaService _karmaService;
    // ... other dependencies

    [HttpPost]
    public async Task<IActionResult> VotePost(int postId, int voteType)
    {
        // Existing vote logic...
        var result = await _postService.VotePostAsync(postId, userId, voteType);
        
        // NEW: Update karma
        if (result.Success)
        {
            await _karmaService.UpdatePostKarmaAsync(postId, voteType);
        }
        
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> VoteComment(int commentId, int voteType)
    {
        // Existing vote logic...
        var result = await _commentService.VoteCommentAsync(commentId, userId, voteType);
        
        // NEW: Update karma
        if (result.Success)
        {
            await _karmaService.UpdateCommentKarmaAsync(commentId, voteType);
        }
        
        return Json(result);
    }
}
```

##### **Step 3: Display Karma in UI**

**Update:** Post cards to show author karma

```cshtml
<!-- discussionspot9/Views/Shared/Partials/PostsPartial/_PostCardEnhanced.cshtml -->
<div class="post-author-info">
    <span class="author-name">u/@Model.Username</span>
    
    <!-- NEW: Karma display -->
    <span class="author-karma" title="@Model.AuthorKarma karma points">
        <i class="fas fa-star text-warning"></i>
        @FormatKarma(Model.AuthorKarma)
    </span>
    
    @if (Model.IsVerified)
    {
        <i class="fas fa-check-circle text-primary" title="Verified"></i>
    }
</div>

@functions {
    string FormatKarma(int karma)
    {
        if (karma >= 1000000) return $"{karma / 1000000.0:F1}M";
        if (karma >= 1000) return $"{karma / 1000.0:F1}K";
        return karma.ToString();
    }
}
```

**CSS for karma display:**

```css
/* discussionspot9/wwwroot/css/karma-styles.css (NEW) */
.author-karma {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    font-size: 0.85rem;
    color: #856404;
    background: #fff3cd;
    padding: 2px 8px;
    border-radius: 12px;
    font-weight: 600;
}

.karma-level-badge {
    display: inline-block;
    padding: 4px 12px;
    border-radius: 16px;
    font-size: 0.75rem;
    font-weight: 600;
    text-transform: uppercase;
}

.karma-level-newbie { background: #e3f2fd; color: #1565c0; }
.karma-level-contributor { background: #e8f5e9; color: #2e7d32; }
.karma-level-regular { background: #fff3e0; color: #e65100; }
.karma-level-expert { background: #f3e5f5; color: #6a1b9a; }
.karma-level-legend { background: #ffe0b2; color: #e65100; }
```

##### **Step 4: Karma Leaderboard Widget**

**File:** `discussionspot9/Views/Shared/Components/KarmaLeaderboard/Default.cshtml` (NEW)

```cshtml
@model List<UserKarmaViewModel>

<div class="karma-leaderboard-widget">
    <div class="widget-header">
        <i class="fas fa-trophy text-warning"></i>
        <span>Top Contributors</span>
    </div>
    <div class="leaderboard-list">
        @for (int i = 0; i < Model.Count; i++)
        {
            var user = Model[i];
            var rank = i + 1;
            var medalClass = rank == 1 ? "gold" : rank == 2 ? "silver" : rank == 3 ? "bronze" : "";
            
            <div class="leaderboard-item">
                <span class="rank rank-@medalClass">@rank</span>
                <img src="@(user.AvatarUrl ?? "/images/default-avatar.png")" alt="@user.DisplayName" class="avatar-sm">
                <div class="user-info">
                    <span class="username">@user.DisplayName</span>
                    @if (user.IsVerified)
                    {
                        <i class="fas fa-check-circle text-primary"></i>
                    }
                </div>
                <span class="karma-points">
                    <i class="fas fa-star text-warning"></i>
                    @user.KarmaPoints
                </span>
            </div>
        }
    </div>
    <a href="/leaderboard" class="view-all-link">View Full Leaderboard →</a>
</div>
```

**ViewComponent Code:**

```csharp
// discussionspot9/ViewComponents/KarmaLeaderboardViewComponent.cs (NEW)
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Services;

namespace discussionspot9.ViewComponents
{
    public class KarmaLeaderboardViewComponent : ViewComponent
    {
        private readonly IKarmaService _karmaService;

        public KarmaLeaderboardViewComponent(IKarmaService karmaService)
        {
            _karmaService = karmaService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 10)
        {
            var leaderboard = await _karmaService.GetKarmaLeaderboardAsync("all", count);
            return View(leaderboard);
        }
    }
}
```

---

### 🏆 **TASK 1.2: Badge & Achievement System**

**Status:** ❌ Not Started

#### **Step 1: Create Database Models**

**File:** `discussionspot9/Models/Domain/Badge.cs` (NEW)

```csharp
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.Domain
{
    /// <summary>
    /// Represents an achievement badge that can be earned
    /// </summary>
    public class Badge
    {
        public int BadgeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        
        [StringLength(500)]
        public string Description { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = null!; // Activity, Quality, Community, Special
        
        [Required]
        [StringLength(50)]
        public string Rarity { get; set; } = "Common"; // Common, Rare, Epic, Legendary
        
        [StringLength(2048)]
        public string IconUrl { get; set; } = null!;
        
        [StringLength(20)]
        public string IconClass { get; set; } = null!; // Font Awesome class
        
        [StringLength(20)]
        public string Color { get; set; } = "#3b82f6"; // Badge color
        
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
    }

    /// <summary>
    /// Tracks which badges users have earned
    /// </summary>
    public class UserBadge
    {
        public int UserBadgeId { get; set; }
        
        [Required]
        public string UserId { get; set; } = null!;
        
        public int BadgeId { get; set; }
        
        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(500)]
        public string? EarnedReason { get; set; }
        
        public bool IsDisplayed { get; set; } = true; // User can hide badges
        public int DisplayOrder { get; set; } // Order on profile (1, 2, 3 for showcase)
        
        // Navigation properties
        public virtual Badge Badge { get; set; } = null!;
    }

    /// <summary>
    /// Defines conditions for earning badges
    /// </summary>
    public class BadgeRequirement
    {
        public int BadgeRequirementId { get; set; }
        public int BadgeId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string RequirementType { get; set; } = null!;
        // Types: PostCount, CommentCount, Karma, VoteCount, AwardCount, DaysActive, etc.
        
        public int RequiredValue { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        // Navigation properties
        public virtual Badge Badge { get; set; } = null!;
    }
}
```

**Update DbContext:**

```csharp
// discussionspot9/Data/DbContext/ApplicationDbContext.cs
public DbSet<Badge> Badges { get; set; }
public DbSet<UserBadge> UserBadges { get; set; }
public DbSet<BadgeRequirement> BadgeRequirements { get; set; }
```

#### **Step 2: Create Migration**

```bash
# Run in PowerShell from discussionspot9 directory
dotnet ef migrations add AddBadgeSystem -o Migrations
dotnet ef database update
```

#### **Step 3: Seed Initial Badges**

**File:** `discussionspot9/Data/SeedData/BadgeSeedData.cs` (NEW)

```csharp
using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;

namespace discussionspot9.Data.SeedData
{
    public static class BadgeSeedData
    {
        public static async Task SeedBadgesAsync(ApplicationDbContext context)
        {
            if (context.Badges.Any()) return; // Already seeded

            var badges = new List<Badge>
            {
                // ACTIVITY BADGES
                new Badge
                {
                    Name = "First Steps",
                    Description = "Created your first post",
                    Category = "Activity",
                    Rarity = "Common",
                    IconClass = "fas fa-baby-carriage",
                    Color = "#10b981",
                    SortOrder = 1
                },
                new Badge
                {
                    Name = "Century Club",
                    Description = "Created 100 posts",
                    Category = "Activity",
                    Rarity = "Rare",
                    IconClass = "fas fa-hundred-points",
                    Color = "#8b5cf6",
                    SortOrder = 2
                },
                new Badge
                {
                    Name = "Comment King",
                    Description = "Posted 1,000 comments",
                    Category = "Activity",
                    Rarity = "Epic",
                    IconClass = "fas fa-crown",
                    Color = "#f59e0b",
                    SortOrder = 3
                },
                new Badge
                {
                    Name = "Daily Devotee",
                    Description = "Active for 30 consecutive days",
                    Category = "Activity",
                    Rarity = "Rare",
                    IconClass = "fas fa-fire",
                    Color = "#ef4444",
                    SortOrder = 4
                },
                new Badge
                {
                    Name = "Early Bird",
                    Description = "One of the first 100 members",
                    Category = "Activity",
                    Rarity = "Legendary",
                    IconClass = "fas fa-egg",
                    Color = "#06b6d4",
                    SortOrder = 5
                },

                // QUALITY BADGES
                new Badge
                {
                    Name = "Golden Post",
                    Description = "Post reached 1,000 upvotes",
                    Category = "Quality",
                    Rarity = "Epic",
                    IconClass = "fas fa-award",
                    Color = "#fbbf24",
                    SortOrder = 10
                },
                new Badge
                {
                    Name = "Viral Creator",
                    Description = "Post reached trending page",
                    Category = "Quality",
                    Rarity = "Epic",
                    IconClass = "fas fa-rocket",
                    Color = "#ec4899",
                    SortOrder = 11
                },
                new Badge
                {
                    Name = "Award Collector",
                    Description = "Received 50 awards",
                    Category = "Quality",
                    Rarity = "Rare",
                    IconClass = "fas fa-medal",
                    Color = "#a855f7",
                    SortOrder = 12
                },
                new Badge
                {
                    Name = "Quality Contributor",
                    Description = "10 posts with 85%+ upvote ratio",
                    Category = "Quality",
                    Rarity = "Rare",
                    IconClass = "fas fa-star",
                    Color = "#f59e0b",
                    SortOrder = 13
                },

                // COMMUNITY BADGES
                new Badge
                {
                    Name = "Community Founder",
                    Description = "Created a community",
                    Category = "Community",
                    Rarity = "Rare",
                    IconClass = "fas fa-users",
                    Color = "#3b82f6",
                    SortOrder = 20
                },
                new Badge
                {
                    Name = "Super Moderator",
                    Description = "Moderator of 3+ communities",
                    Category = "Community",
                    Rarity = "Epic",
                    IconClass = "fas fa-shield-alt",
                    Color = "#10b981",
                    SortOrder = 21
                },
                new Badge
                {
                    Name = "Helpful Helper",
                    Description = "500+ helpful comments",
                    Category = "Community",
                    Rarity = "Rare",
                    IconClass = "fas fa-hands-helping",
                    Color = "#14b8a6",
                    SortOrder = 22
                },
                new Badge
                {
                    Name = "Ambassador",
                    Description = "Invited 10+ users who stayed active",
                    Category = "Community",
                    Rarity = "Epic",
                    IconClass = "fas fa-handshake",
                    Color = "#6366f1",
                    SortOrder = 23
                },

                // SPECIAL BADGES
                new Badge
                {
                    Name = "Verified Expert",
                    Description = "Verified by moderators as expert",
                    Category = "Special",
                    Rarity = "Legendary",
                    IconClass = "fas fa-check-circle",
                    Color = "#0ea5e9",
                    SortOrder = 30
                },
                new Badge
                {
                    Name = "Staff Member",
                    Description = "DiscussionSpot team member",
                    Category = "Special",
                    Rarity = "Legendary",
                    IconClass = "fas fa-briefcase",
                    Color = "#7c3aed",
                    SortOrder = 31
                },
                new Badge
                {
                    Name = "Beta Tester",
                    Description = "Tested features before launch",
                    Category = "Special",
                    Rarity = "Legendary",
                    IconClass = "fas fa-flask",
                    Color = "#06b6d4",
                    SortOrder = 32
                },
                new Badge
                {
                    Name = "1 Year Club",
                    Description = "Member for 1 year",
                    Category = "Special",
                    Rarity = "Rare",
                    IconClass = "fas fa-birthday-cake",
                    Color = "#ec4899",
                    SortOrder = 33
                },
                new Badge
                {
                    Name = "OG Member",
                    Description = "Member for 5+ years",
                    Category = "Special",
                    Rarity = "Legendary",
                    IconClass = "fas fa-gem",
                    Color = "#f59e0b",
                    SortOrder = 34
                }
            };

            await context.Badges.AddRangeAsync(badges);
            await context.SaveChangesAsync();
        }
    }
}
```

#### **Step 4: Badge Service**

**File:** `discussionspot9/Services/BadgeService.cs` (NEW)

```csharp
using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public interface IBadgeService
    {
        Task CheckAndAwardBadgesAsync(string userId);
        Task AwardBadgeAsync(string userId, int badgeId, string reason);
        Task<List<Badge>> GetUserBadgesAsync(string userId);
        Task<List<Badge>> GetAllBadgesAsync();
        Task SetDisplayedBadgesAsync(string userId, List<int> badgeIds);
    }

    public class BadgeService : IBadgeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BadgeService> _logger;
        private readonly INotificationService _notificationService;

        public BadgeService(
            ApplicationDbContext context,
            ILogger<BadgeService> logger,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Check user's stats and award eligible badges
        /// </summary>
        public async Task CheckAndAwardBadgesAsync(string userId)
        {
            try
            {
                var userStats = await GetUserStatsAsync(userId);
                var existingBadges = await _context.UserBadges
                    .Where(ub => ub.UserId == userId)
                    .Select(ub => ub.BadgeId)
                    .ToListAsync();

                // Check each badge type
                await CheckActivityBadges(userId, userStats, existingBadges);
                await CheckQualityBadges(userId, userStats, existingBadges);
                await CheckCommunityBadges(userId, userStats, existingBadges);
                await CheckSpecialBadges(userId, userStats, existingBadges);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking badges for user {UserId}", userId);
            }
        }

        private async Task CheckActivityBadges(string userId, UserStats stats, List<int> existing)
        {
            // First Steps - Created first post
            if (stats.PostCount >= 1 && !existing.Contains(1))
            {
                await AwardBadgeAsync(userId, 1, "Created your first post!");
            }

            // Century Club - 100 posts
            if (stats.PostCount >= 100 && !existing.Contains(2))
            {
                await AwardBadgeAsync(userId, 2, "Created 100 posts!");
            }

            // Comment King - 1000 comments
            if (stats.CommentCount >= 1000 && !existing.Contains(3))
            {
                await AwardBadgeAsync(userId, 3, "Posted 1,000 comments!");
            }

            // Daily Devotee - 30 day streak
            if (stats.ConsecutiveDays >= 30 && !existing.Contains(4))
            {
                await AwardBadgeAsync(userId, 4, "Active for 30 consecutive days!");
            }
        }

        private async Task CheckQualityBadges(string userId, UserStats stats, List<int> existing)
        {
            // Golden Post - 1000 upvotes on a post
            if (stats.MaxPostUpvotes >= 1000 && !existing.Contains(10))
            {
                await AwardBadgeAsync(userId, 10, "Post reached 1,000 upvotes!");
            }

            // Award Collector - 50 awards
            if (stats.AwardsReceived >= 50 && !existing.Contains(12))
            {
                await AwardBadgeAsync(userId, 12, "Received 50 awards!");
            }

            // Quality Contributor - 10 posts with 85%+ ratio
            if (stats.HighQualityPosts >= 10 && !existing.Contains(13))
            {
                await AwardBadgeAsync(userId, 13, "10 posts with 85%+ upvote ratio!");
            }
        }

        private async Task CheckCommunityBadges(string userId, UserStats stats, List<int> existing)
        {
            // Community Founder
            if (stats.CommunitiesCreated >= 1 && !existing.Contains(20))
            {
                await AwardBadgeAsync(userId, 20, "Created a community!");
            }

            // Super Moderator
            if (stats.CommunitiesModerated >= 3 && !existing.Contains(21))
            {
                await AwardBadgeAsync(userId, 21, "Moderator of 3+ communities!");
            }
        }

        private async Task CheckSpecialBadges(string userId, UserStats stats, List<int> existing)
        {
            // 1 Year Club
            var accountAge = DateTime.UtcNow - stats.JoinDate;
            if (accountAge.TotalDays >= 365 && !existing.Contains(33))
            {
                await AwardBadgeAsync(userId, 33, "Member for 1 year!");
            }

            // OG Member - 5 years
            if (accountAge.TotalDays >= 1825 && !existing.Contains(34))
            {
                await AwardBadgeAsync(userId, 34, "Member for 5+ years!");
            }
        }

        public async Task AwardBadgeAsync(string userId, int badgeId, string reason)
        {
            try
            {
                // Check if already has badge
                var exists = await _context.UserBadges
                    .AnyAsync(ub => ub.UserId == userId && ub.BadgeId == badgeId);

                if (exists) return;

                var userBadge = new UserBadge
                {
                    UserId = userId,
                    BadgeId = badgeId,
                    EarnedReason = reason,
                    EarnedAt = DateTime.UtcNow
                };

                _context.UserBadges.Add(userBadge);
                await _context.SaveChangesAsync();

                // Get badge details for notification
                var badge = await _context.Badges.FindAsync(badgeId);
                if (badge != null)
                {
                    // Send notification
                    await _notificationService.CreateNotificationAsync(
                        userId,
                        $"🏆 You earned the '{badge.Name}' badge!",
                        reason,
                        $"/profile/{userId}",
                        "Badge"
                    );

                    _logger.LogInformation(
                        "Badge awarded: {BadgeName} to user {UserId} - {Reason}",
                        badge.Name, userId, reason);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding badge {BadgeId} to user {UserId}", badgeId, userId);
            }
        }

        public async Task<List<Badge>> GetUserBadgesAsync(string userId)
        {
            return await _context.UserBadges
                .Where(ub => ub.UserId == userId)
                .Include(ub => ub.Badge)
                .OrderByDescending(ub => ub.EarnedAt)
                .Select(ub => ub.Badge)
                .ToListAsync();
        }

        public async Task<List<Badge>> GetAllBadgesAsync()
        {
            return await _context.Badges
                .Where(b => b.IsActive)
                .OrderBy(b => b.SortOrder)
                .ToListAsync();
        }

        public async Task SetDisplayedBadgesAsync(string userId, List<int> badgeIds)
        {
            var userBadges = await _context.UserBadges
                .Where(ub => ub.UserId == userId)
                .ToListAsync();

            foreach (var ub in userBadges)
            {
                ub.IsDisplayed = badgeIds.Contains(ub.BadgeId);
                ub.DisplayOrder = badgeIds.IndexOf(ub.BadgeId) + 1;
            }

            await _context.SaveChangesAsync();
        }

        // Helper to get user stats
        private async Task<UserStats> GetUserStatsAsync(string userId)
        {
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var posts = await _context.Posts
                .Where(p => p.UserId == userId && p.Status == "published")
                .ToListAsync();

            var comments = await _context.Comments
                .Where(c => c.UserId == userId)
                .CountAsync();

            var communities = await _context.Communities
                .Where(c => c.CreatedBy == userId)
                .CountAsync();

            var moderated = await _context.CommunityMembers
                .Where(cm => cm.UserId == userId && (cm.Role == "Admin" || cm.Role == "Moderator"))
                .CountAsync();

            var awardsReceived = await _context.PostAwards
                .Where(pa => posts.Select(p => p.PostId).Contains(pa.PostId))
                .CountAsync();

            var highQualityPosts = posts.Count(p =>
            {
                var total = p.UpvoteCount + Math.Abs(p.DownvoteCount);
                if (total == 0) return false;
                var ratio = (double)p.UpvoteCount / total;
                return ratio >= 0.85;
            });

            return new UserStats
            {
                PostCount = posts.Count,
                CommentCount = comments,
                CommunitiesCreated = communities,
                CommunitiesModerated = moderated,
                AwardsReceived = awardsReceived,
                MaxPostUpvotes = posts.Any() ? posts.Max(p => p.UpvoteCount) : 0,
                HighQualityPosts = highQualityPosts,
                JoinDate = profile?.JoinDate ?? DateTime.UtcNow,
                ConsecutiveDays = 0 // TODO: Calculate from UserActivity table
            };
        }
    }

    // Helper class
    public class UserStats
    {
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public int CommunitiesCreated { get; set; }
        public int CommunitiesModerated { get; set; }
        public int AwardsReceived { get; set; }
        public int MaxPostUpvotes { get; set; }
        public int HighQualityPosts { get; set; }
        public DateTime JoinDate { get; set; }
        public int ConsecutiveDays { get; set; }
    }
}
```

#### **Step 5: Display Badges in UI**

**Badge showcase component:**

```cshtml
<!-- discussionspot9/Views/Shared/Components/UserBadges/Default.cshtml -->
@model List<Badge>

@if (Model.Any())
{
    <div class="user-badges-showcase">
        @foreach (var badge in Model.Take(3)) // Show max 3 in showcase
        {
            <div class="badge-item" 
                 title="@badge.Name - @badge.Description"
                 data-bs-toggle="tooltip"
                 style="background: linear-gradient(135deg, @badge.Color 0%, @AdjustColor(badge.Color, -20) 100%);">
                <i class="@badge.IconClass badge-icon"></i>
                <span class="badge-name">@badge.Name</span>
            </div>
        }
        
        @if (Model.Count > 3)
        {
            <div class="badge-more">
                +@(Model.Count - 3) more
            </div>
        }
    </div>
}

@functions {
    string AdjustColor(string hex, int percent)
    {
        // Simple color darkening function
        return hex; // Simplified - implement proper color adjustment if needed
    }
}
```

---

## 📝 **PHASE 2: RICH LINK PREVIEWS & SEARCH** (Week 2)

### **TASK 2.1: Rich Link Previews in Feed**

**Status:** 🟡 Partial - Works on detail page, not in feed

#### **Implementation:**

##### **Step 1: Fetch metadata on post creation (Background)**

```csharp
// discussionspot9/Services/PostService.cs - Update CreatePostAsync

public async Task<int> CreatePostAsync(CreatePostViewModel model, string userId)
{
    // ... existing post creation code ...
    
    // NEW: Fetch link metadata in background if URL exists
    if (!string.IsNullOrWhiteSpace(newPost.Url))
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var metadata = await _linkMetadataService.GetMetadataAsync(newPost.Url);
                
                // Store in Post model (add new fields to Post)
                newPost.LinkPreviewTitle = metadata.Title;
                newPost.LinkPreviewDescription = metadata.Description;
                newPost.LinkPreviewImage = metadata.ThumbnailUrl;
                newPost.LinkPreviewDomain = metadata.Domain;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Link metadata cached for post {PostId}", newPost.PostId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching link metadata for post {PostId}", newPost.PostId);
            }
        });
    }
    
    return newPost.PostId;
}
```

##### **Step 2: Add fields to Post model**

```csharp
// discussionspot9/Models/Domain/Post.cs - Add properties

public class Post
{
    // ... existing properties ...
    
    // Link Preview Cache (to avoid fetching on every page load)
    public string? LinkPreviewTitle { get; set; }
    public string? LinkPreviewDescription { get; set; }
    public string? LinkPreviewImage { get; set; }
    public string? LinkPreviewDomain { get; set; }
    public DateTime? LinkPreviewFetchedAt { get; set; }
}
```

##### **Step 3: Create migration**

```bash
dotnet ef migrations add AddLinkPreviewCacheToPost -o Migrations
dotnet ef database update
```

##### **Step 4: Display in post cards**

```cshtml
<!-- discussionspot9/Views/Shared/Partials/PostsPartial/_PostCardEnhanced.cshtml -->

@if (Model.PostType == "link" && !string.IsNullOrEmpty(Model.LinkPreviewImage))
{
    <!-- Large link preview card -->
    <div class="post-link-preview-large">
        <div class="preview-image-container">
            <img src="@Model.LinkPreviewImage" 
                 alt="@Model.LinkPreviewTitle" 
                 class="preview-image"
                 loading="lazy"
                 onerror="this.style.display='none'; this.parentElement.style.display='none';">
        </div>
        <div class="preview-content">
            <div class="preview-domain">
                <img src="@GetFaviconUrl(Model.Url)" 
                     alt="" 
                     class="domain-favicon"
                     onerror="this.style.display='none';">
                <span>@Model.LinkPreviewDomain</span>
            </div>
            <h4 class="preview-title">@Model.LinkPreviewTitle</h4>
            <p class="preview-description">@Model.LinkPreviewDescription</p>
        </div>
    </div>
}
else if (Model.PostType == "image" && Model.MediaUrls.Any())
{
    <!-- Large image preview -->
    <div class="post-image-preview-large">
        <img src="@Model.MediaUrls.First()" 
             alt="@Model.Title" 
             class="post-image-large"
             loading="lazy">
        @if (Model.MediaUrls.Count > 1)
        {
            <div class="image-count-badge">
                <i class="fas fa-images"></i>
                @Model.MediaUrls.Count images
            </div>
        }
    </div>
}

@functions {
    string GetFaviconUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}/favicon.ico";
        }
        catch
        {
            return "/images/default-favicon.png";
        }
    }
}
```

##### **Step 5: CSS for large previews**

```css
/* discussionspot9/wwwroot/css/link-preview-feed.css (NEW) */

.post-link-preview-large {
    display: flex;
    gap: 1rem;
    padding: 1rem;
    background: var(--card-bg);
    border: 1px solid var(--border-color);
    border-radius: 8px;
    transition: all 0.2s;
    cursor: pointer;
}

.post-link-preview-large:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    transform: translateY(-2px);
}

.preview-image-container {
    flex-shrink: 0;
    width: 280px;
    height: 200px;
    border-radius: 6px;
    overflow: hidden;
    background: #f0f2f5;
}

.preview-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.preview-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    min-width: 0;
}

.preview-domain {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.85rem;
    color: var(--text-muted);
}

.domain-favicon {
    width: 16px;
    height: 16px;
}

.preview-title {
    font-size: 1.1rem;
    font-weight: 600;
    color: var(--text-dark);
    margin: 0;
    line-height: 1.4;
    overflow: hidden;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
}

.preview-description {
    font-size: 0.9rem;
    color: var(--text-muted);
    line-height: 1.5;
    margin: 0;
    overflow: hidden;
    display: -webkit-box;
    -webkit-line-clamp: 3;
    -webkit-box-orient: vertical;
}

/* Large image previews */
.post-image-preview-large {
    position: relative;
    width: 100%;
    max-height: 500px;
    overflow: hidden;
    border-radius: 8px;
}

.post-image-large {
    width: 100%;
    height: auto;
    display: block;
}

.image-count-badge {
    position: absolute;
    bottom: 12px;
    right: 12px;
    background: rgba(0, 0, 0, 0.75);
    color: white;
    padding: 6px 12px;
    border-radius: 20px;
    font-size: 0.85rem;
    display: flex;
    align-items: center;
    gap: 6px;
}

/* Responsive */
@media (max-width: 768px) {
    .post-link-preview-large {
        flex-direction: column;
    }
    
    .preview-image-container {
        width: 100%;
        height: 180px;
    }
}
```

---

### **TASK 2.2: Enhanced Search**

**Status:** 🟡 Basic search exists, needs filters

#### **Implementation:**

##### **Step 1: Add search filters to ViewModel**

```csharp
// discussionspot9/Models/ViewModels/SearchViewModels/SearchResultsViewModel.cs - Update

public class SearchResultsViewModel
{
    // Existing properties...
    public string Query { get; set; } = "";
    public string CurrentType { get; set; } = "all";
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    
    // NEW: Filter properties
    public string SortBy { get; set; } = "relevance"; // relevance, new, hot, top
    public string PostType { get; set; } = "all"; // all, text, link, image, video, poll
    public bool HasMedia { get; set; } = false;
    public string TimeRange { get; set; } = "all"; // hour, day, week, month, year, all
    public int MinKarma { get; set; } = 0;
    public bool VerifiedOnly { get; set; } = false;
    
    // Results
    public List<SearchPostResult> Posts { get; set; } = new();
    public List<SearchCommunityResult> Communities { get; set; } = new();
    public List<SearchUserResult> Users { get; set; } = new();
    
    // Stats
    public int TotalPosts { get; set; }
    public int TotalCommunities { get; set; }
    public int TotalUsers { get; set; }
    public int TotalResults => TotalPosts + TotalCommunities + TotalUsers;
}
```

##### **Step 2: Update SearchController with filters**

```csharp
// discussionspot9/Controllers/SearchController.cs - Update Index action

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
        VerifiedOnly = verifiedOnly
    };

    try
    {
        // Search Posts with filters
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
                        AuthorName = p.UserProfile.DisplayName,
                        Score = p.Score,
                        CommentCount = p.CommentCount,
                        CreatedAt = p.CreatedAt,
                        PostType = p.PostType,
                        ThumbnailUrl = p.LinkPreviewImage // Use cached thumbnail
                    })
                    .ToListAsync();
            }
            else
            {
                // For "all" type, show preview (top 3)
                model.Posts = await postsQuery
                    .Take(3)
                    .Select(p => new SearchPostResult
                    {
                        PostId = p.PostId,
                        Title = p.Title,
                        Slug = p.Slug,
                        Excerpt = p.Content != null ? p.Content.Substring(0, Math.Min(200, p.Content.Length)) : "",
                        CommunityName = p.Community.Name,
                        CommunitySlug = p.Community.Slug,
                        AuthorName = p.UserProfile.DisplayName,
                        Score = p.Score,
                        CommentCount = p.CommentCount,
                        CreatedAt = p.CreatedAt,
                        PostType = p.PostType,
                        ThumbnailUrl = p.LinkPreviewImage
                    })
                    .ToListAsync();
            }
        }

        // Search Communities (existing logic)
        if (type == "all" || type == "communities")
        {
            // ... existing community search code ...
        }

        // Search Users with karma filter
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

            // ... rest of user search logic ...
        }

        model.TotalPages = (int)Math.Ceiling((type switch
        {
            "posts" => model.TotalPosts,
            "communities" => model.TotalCommunities,
            "users" => model.TotalUsers,
            _ => Math.Max(model.TotalPosts, Math.Max(model.TotalCommunities, model.TotalUsers))
        }) / (double)pageSize);

        await TrackSearchAsync(q, model.TotalResults);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error performing search for query: {Query}", q);
    }

    return View(model);
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
```

##### **Step 3: Update Search View with filters**

```cshtml
@* discussionspot9/Views/Search/Index.cshtml *@
@model SearchResultsViewModel

<div class="search-page">
    <div class="container">
        <div class="row">
            <!-- Search Filters Sidebar -->
            <div class="col-lg-3">
                <div class="search-filters-sidebar">
                    <h5>Filters</h5>
                    
                    <form method="get" action="/search" id="searchFilterForm">
                        <input type="hidden" name="q" value="@Model.Query">
                        <input type="hidden" name="type" value="@Model.CurrentType">
                        
                        <!-- Sort By -->
                        <div class="filter-group">
                            <label>Sort By</label>
                            <select name="sort" class="form-select form-select-sm">
                                <option value="relevance" selected="@(Model.SortBy == "relevance")">Relevance</option>
                                <option value="new" selected="@(Model.SortBy == "new")">New</option>
                                <option value="hot" selected="@(Model.SortBy == "hot")">Hot</option>
                                <option value="top" selected="@(Model.SortBy == "top")">Top</option>
                            </select>
                        </div>

                        <!-- Post Type (only for posts) -->
                        @if (Model.CurrentType == "all" || Model.CurrentType == "posts")
                        {
                            <div class="filter-group">
                                <label>Post Type</label>
                                <select name="postType" class="form-select form-select-sm">
                                    <option value="all">All Types</option>
                                    <option value="text" selected="@(Model.PostType == "text")">Text</option>
                                    <option value="link" selected="@(Model.PostType == "link")">Link</option>
                                    <option value="image" selected="@(Model.PostType == "image")">Image</option>
                                    <option value="video" selected="@(Model.PostType == "video")">Video</option>
                                    <option value="poll" selected="@(Model.PostType == "poll")">Poll</option>
                                </select>
                            </div>

                            <div class="filter-group">
                                <div class="form-check">
                                    <input class="form-check-input" type="checkbox" name="hasMedia" 
                                           value="true" id="hasMedia" @(Model.HasMedia ? "checked" : "")>
                                    <label class="form-check-label" for="hasMedia">
                                        Has Media
                                    </label>
                                </div>
                            </div>
                        }

                        <!-- Time Range -->
                        <div class="filter-group">
                            <label>Time Range</label>
                            <select name="timeRange" class="form-select form-select-sm">
                                <option value="all">All Time</option>
                                <option value="hour" selected="@(Model.TimeRange == "hour")">Past Hour</option>
                                <option value="day" selected="@(Model.TimeRange == "day")">Past 24 Hours</option>
                                <option value="week" selected="@(Model.TimeRange == "week")">Past Week</option>
                                <option value="month" selected="@(Model.TimeRange == "month")">Past Month</option>
                                <option value="year" selected="@(Model.TimeRange == "year")">Past Year</option>
                            </select>
                        </div>

                        <!-- Karma Filter (for users) -->
                        @if (Model.CurrentType == "all" || Model.CurrentType == "users")
                        {
                            <div class="filter-group">
                                <label>Minimum Karma</label>
                                <input type="number" name="minKarma" class="form-control form-control-sm" 
                                       value="@Model.MinKarma" min="0" step="10">
                            </div>

                            <div class="filter-group">
                                <div class="form-check">
                                    <input class="form-check-input" type="checkbox" name="verifiedOnly" 
                                           value="true" id="verifiedOnly" @(Model.VerifiedOnly ? "checked" : "")>
                                    <label class="form-check-label" for="verifiedOnly">
                                        Verified Users Only
                                    </label>
                                </div>
                            </div>
                        }

                        <button type="submit" class="btn btn-primary w-100 mt-3">
                            Apply Filters
                        </button>
                        <a href="/search?q=@Model.Query&type=@Model.CurrentType" class="btn btn-link w-100">
                            Clear Filters
                        </a>
                    </form>
                </div>
            </div>

            <!-- Search Results -->
            <div class="col-lg-9">
                <!-- Results summary -->
                <div class="search-results-header">
                    <h3>Search Results for "@Model.Query"</h3>
                    <p class="text-muted">
                        Found @Model.TotalResults results
                        @if (!string.IsNullOrEmpty(Model.TimeRange) && Model.TimeRange != "all")
                        {
                            <span>in @Model.TimeRange</span>
                        }
                    </p>
                </div>

                <!-- Type tabs -->
                <ul class="nav nav-tabs mb-4">
                    <li class="nav-item">
                        <a class="nav-link @(Model.CurrentType == "all" ? "active" : "")" 
                           href="/search?q=@Model.Query&type=all">
                            All (@Model.TotalResults)
                        </a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link @(Model.CurrentType == "posts" ? "active" : "")" 
                           href="/search?q=@Model.Query&type=posts">
                            Posts (@Model.TotalPosts)
                        </a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link @(Model.CurrentType == "communities" ? "active" : "")" 
                           href="/search?q=@Model.Query&type=communities">
                            Communities (@Model.TotalCommunities)
                        </a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link @(Model.CurrentType == "users" ? "active" : "")" 
                           href="/search?q=@Model.Query&type=users">
                            Users (@Model.TotalUsers)
                        </a>
                    </li>
                </ul>

                <!-- Post results with rich previews -->
                @if (Model.Posts.Any())
                {
                    <div class="search-results-section">
                        @foreach (var post in Model.Posts)
                        {
                            <div class="search-post-card">
                                @if (!string.IsNullOrEmpty(post.ThumbnailUrl))
                                {
                                    <div class="post-thumbnail-large">
                                        <img src="@post.ThumbnailUrl" alt="@post.Title" loading="lazy">
                                    </div>
                                }
                                
                                <div class="post-details">
                                    <div class="post-meta-small">
                                        <a href="/r/@post.CommunitySlug" class="community-link">
                                            r/@post.CommunityName
                                        </a>
                                        <span>•</span>
                                        <span>Posted by u/@post.AuthorName</span>
                                        <span>•</span>
                                        <span>@post.CreatedAt.ToRelativeTime()</span>
                                    </div>
                                    
                                    <h4 class="post-title-search">
                                        <a href="/r/@post.CommunitySlug/posts/@post.Slug">
                                            @Html.Raw(HighlightQuery(post.Title, Model.Query))
                                        </a>
                                    </h4>
                                    
                                    @if (!string.IsNullOrEmpty(post.Excerpt))
                                    {
                                        <p class="post-excerpt-search">
                                            @Html.Raw(HighlightQuery(post.Excerpt, Model.Query))
                                        </p>
                                    }
                                    
                                    <div class="post-stats-small">
                                        <span><i class="fas fa-arrow-up"></i> @post.Score</span>
                                        <span><i class="fas fa-comment"></i> @post.CommentCount comments</span>
                                        @if (post.PostType != "text")
                                        {
                                            <span class="post-type-badge">@post.PostType</span>
                                        }
                                    </div>
                                </div>
                            </div>
                        }
                    </div>
                }

                <!-- Community & User results (existing) -->
                <!-- ... -->

                <!-- Pagination -->
                @if (Model.TotalPages > 1)
                {
                    <nav aria-label="Search pagination">
                        <ul class="pagination">
                            <!-- Pagination links -->
                        </ul>
                    </nav>
                }
            </div>
        </div>
    </div>
</div>

@functions {
    string HighlightQuery(string text, string query)
    {
        if (string.IsNullOrEmpty(query)) return text;
        
        var regex = new System.Text.RegularExpressions.Regex(
            $"({System.Text.RegularExpressions.Regex.Escape(query)})", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        return regex.Replace(text, "<mark>$1</mark>");
    }
}
```

---

## 🔄 **PHASE 3: FOLLOW SYSTEM UI & STORIES** (Week 3-4)

### **TASK 3.1: Follow System UI**

**Status:** ✅ Backend exists, needs UI

#### **Quick Implementation:**

##### **Step 1: Follow Button Component**

```cshtml
@* discussionspot9/Views/Shared/Components/FollowButton/Default.cshtml *@
@model FollowButtonViewModel

<button class="btn btn-follow @(Model.IsFollowing ? "btn-following" : "btn-not-following")"
        data-user-id="@Model.TargetUserId"
        onclick="toggleFollow('@Model.TargetUserId', this)">
    @if (Model.IsFollowing)
    {
        <i class="fas fa-user-check"></i>
        <span>Following</span>
    }
    else
    {
        <i class="fas fa-user-plus"></i>
        <span>Follow</span>
    }
</button>

<script>
async function toggleFollow(userId, button) {
    button.disabled = true;
    
    try {
        const response = await fetch('/api/follow/toggle', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ userId: userId })
        });
        
        const result = await response.json();
        
        if (result.success) {
            // Update button state
            if (result.isFollowing) {
                button.classList.remove('btn-not-following');
                button.classList.add('btn-following');
                button.innerHTML = '<i class="fas fa-user-check"></i> <span>Following</span>';
            } else {
                button.classList.remove('btn-following');
                button.classList.add('btn-not-following');
                button.innerHTML = '<i class="fas fa-user-plus"></i> <span>Follow</span>';
            }
            
            // Show toast notification
            showToast(result.message);
        }
    } catch (error) {
        console.error('Error toggling follow:', error);
        showToast('Error updating follow status', 'error');
    } finally {
        button.disabled = false;
    }
}

function showToast(message, type = 'success') {
    // Use your existing toast notification system
    console.log(message);
}
</script>
```

##### **Step 2: Follow API Endpoint**

```csharp
// discussionspot9/Controllers/FollowController.cs (NEW)

[ApiController]
[Route("api/follow")]
public class FollowController : ControllerBase
{
    private readonly IFollowService _followService;
    private readonly ILogger<FollowController> _logger;

    public FollowController(IFollowService followService, ILogger<FollowController> logger)
    {
        _followService = followService;
        _logger = logger;
    }

    [HttpPost("toggle")]
    [Authorize]
    public async Task<IActionResult> ToggleFollow([FromBody] FollowRequest request)
    {
        try
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == request.UserId)
            {
                return BadRequest(new { success = false, message = "You cannot follow yourself" });
            }

            var isFollowing = await _followService.IsFollowingAsync(currentUserId, request.UserId);
            
            if (isFollowing)
            {
                await _followService.UnfollowUserAsync(currentUserId, request.UserId);
                return Ok(new { success = true, isFollowing = false, message = "Unfollowed user" });
            }
            else
            {
                await _followService.FollowUserAsync(currentUserId, request.UserId);
                return Ok(new { success = true, isFollowing = true, message = "Following user" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling follow for user {UserId}", request.UserId);
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }

    [HttpGet("followers/{userId}")]
    public async Task<IActionResult> GetFollowers(string userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var followers = await _followService.GetFollowersAsync(userId, page, pageSize);
            return Ok(followers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting followers for user {UserId}", userId);
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }

    [HttpGet("following/{userId}")]
    public async Task<IActionResult> GetFollowing(string userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var following = await _followService.GetFollowingAsync(userId, page, pageSize);
            return Ok(following);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting following for user {UserId}", userId);
            return StatusCode(500, new { success = false, message = "An error occurred" });
        }
    }
}

public class FollowRequest
{
    public string UserId { get; set; } = null!;
}
```

---

### **TASK 3.2: Stories Updates**

**Status:** ✅ Stories exist, adding engagement features

#### **Quick Wins:**

##### **1. Story View Count (Already tracking ViewCount)**
- Display view count on story cards
- Add trending stories widget

##### **2. Story Reactions**
Add simple reactions (like, love, wow) - Quick implementation using existing Award system or new StoryReaction table

##### **3. Story Sharing**
Use existing share functionality, add "Share Story" buttons

##### **4. Story Discovery Feed**
Create a dedicated stories feed page with:
- Trending stories (by view count)
- Recent stories
- Stories from followed users
- Category-based stories

---

## 📝 **SERVICE REGISTRATION**

Add all new services to `Program.cs`:

```csharp
// discussionspot9/Program.cs

// Karma & Badges
builder.Services.AddScoped<IKarmaService, KarmaService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();

// Follow system (if not already registered)
builder.Services.AddScoped<IFollowService, FollowService>();

// Background service for badge checking
builder.Services.AddHostedService<BadgeCheckBackgroundService>();
```

---

## 🗓️ **IMPLEMENTATION SCHEDULE**

### **Week 1: Karma & Badges**
- **Mon-Tue:** Karma service + integration with voting
- **Wed-Thu:** Badge system + database models
- **Fri:** UI components + testing

### **Week 2: Previews & Search**
- **Mon-Tue:** Link preview caching + feed display
- **Wed-Thu:** Enhanced search with filters
- **Fri:** Testing + polish

### **Week 3: Social Features**
- **Mon-Tue:** Follow UI + API
- **Wed:** Story reactions
- **Thu-Fri:** Story discovery feed

### **Week 4: Polish & Testing**
- **Mon-Wed:** Bug fixes, performance optimization
- **Thu-Fri:** User testing + documentation

---

## 📊 **SUCCESS METRICS**

After implementation, track these KPIs:

1. **Engagement Rate:**
   - Time on site (+2-3x expected)
   - Pages per session (+50% expected)
   - Return visitor rate (+40% expected)

2. **User Activity:**
   - Daily active users (+150% expected)
   - Posts per day (+100% expected)
   - Comments per day (+200% expected)

3. **Gamification:**
   - % users with badges (target: 60%+)
   - Average karma per user (target: 50+)
   - Badge earning rate (target: 2+ per week per active user)

4. **Social:**
   - Follow actions per day (target: 50+)
   - Average followers per user (target: 5+)

---

## 🚨 **CRITICAL NOTES**

1. **Performance:** Karma calculations can be expensive - use background jobs
2. **Caching:** Cache link previews and user karma for 5-15 minutes
3. **Notifications:** Badge earning should trigger real-time notifications
4. **Mobile:** All features MUST work on mobile (60%+ traffic)
5. **Testing:** Test with 1000+ users before going live

---

## 📚 **NEXT STEPS AFTER THIS ROADMAP**

1. **Personalized Feed Algorithm** (Week 5-6)
2. **Content Recommendations** (Week 7)
3. **Mobile App** (Week 8+)
4. **Advanced Analytics** (Week 9+)

---

**End of Roadmap**

