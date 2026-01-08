# Forums & Affiliate Marketing Implementation Plan

## Overview

This document outlines the implementation plan for:
1. **Reddit-style Discussion Forums** with photos and link posts
2. **Affiliate Marketing Integration** with Amazon and AliExpress

---

## Part 1: Reddit-Style Forums with Photos & Links

### Current State Analysis

**Existing:**
- ✅ `ForumThread` model exists (old database, read-only)
- ✅ Basic forum structure in place
- ❌ No post type system (text/image/link)
- ❌ No media attachments for forums
- ❌ No Reddit-style UI

**What Needs to be Added:**

### 1.1 Database Schema Updates (New Database - gsmsharingv4)

#### Create New Forum System in New Database

```sql
-- Post Types Table (for both Posts and Forums)
CREATE TABLE PostTypes (
    PostTypeID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL, -- 'text', 'image', 'link', 'video', 'poll'
    Description NVARCHAR(255)
);

-- Forum Posts Table (New Database)
CREATE TABLE ForumPosts (
    ForumPostID BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId BIGINT NULL,
    CommunityID BIGINT NULL,
    
    -- Post Type
    PostType NVARCHAR(50) NOT NULL DEFAULT 'text', -- 'text', 'image', 'link', 'video'
    
    -- Content
    Title NVARCHAR(500) NOT NULL,
    Content NVARCHAR(MAX) NULL, -- For text posts
    Slug NVARCHAR(500) NULL,
    
    -- Link Post Fields
    LinkUrl NVARCHAR(1000) NULL, -- For link posts
    LinkTitle NVARCHAR(500) NULL,
    LinkDescription NVARCHAR(1000) NULL,
    LinkThumbnail NVARCHAR(1000) NULL,
    
    -- Engagement
    ViewCount INT DEFAULT 0,
    Score INT DEFAULT 0,
    UpvoteCount INT DEFAULT 0,
    DownvoteCount INT DEFAULT 0,
    CommentCount INT DEFAULT 0,
    
    -- Status
    IsPinned BIT DEFAULT 0,
    IsLocked BIT DEFAULT 0,
    IsDeleted BIT DEFAULT 0,
    PostStatus NVARCHAR(50) DEFAULT 'published',
    
    -- Timestamps
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    PublishedAt DATETIME2 NULL,
    DeletedAt DATETIME2 NULL,
    
    -- Indexes
    INDEX IX_ForumPosts_CommunityID (CommunityID),
    INDEX IX_ForumPosts_UserId (UserId),
    INDEX IX_ForumPosts_PostType (PostType),
    INDEX IX_ForumPosts_CreatedAt (CreatedAt DESC),
    INDEX IX_ForumPosts_Score (Score DESC)
);

-- Post Media Attachments (for images, videos)
CREATE TABLE PostMedia (
    MediaID BIGINT IDENTITY(1,1) PRIMARY KEY,
    PostID BIGINT NULL, -- Can link to ForumPosts or Posts
    ForumPostID BIGINT NULL, -- Specific to forum posts
    MediaType NVARCHAR(50) NOT NULL, -- 'image', 'video', 'gif'
    MediaUrl NVARCHAR(1000) NOT NULL,
    ThumbnailUrl NVARCHAR(1000) NULL,
    AltText NVARCHAR(500) NULL,
    DisplayOrder INT DEFAULT 0,
    FileSize BIGINT NULL,
    Width INT NULL,
    Height INT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    INDEX IX_PostMedia_PostID (PostID),
    INDEX IX_PostMedia_ForumPostID (ForumPostID)
);

-- Forum Post Votes (Reddit-style upvote/downvote)
CREATE TABLE ForumPostVotes (
    VoteID BIGINT IDENTITY(1,1) PRIMARY KEY,
    ForumPostID BIGINT NOT NULL,
    UserId BIGINT NOT NULL,
    VoteType NVARCHAR(10) NOT NULL, -- 'upvote', 'downvote'
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    UNIQUE INDEX IX_ForumPostVotes_UserPost (ForumPostID, UserId),
    INDEX IX_ForumPostVotes_ForumPostID (ForumPostID)
);
```

### 1.2 Model Updates

#### Create New Models (NewSchema)

```csharp
// Models/NewSchema/ForumPost.cs
namespace GsmsharingV2.Models.NewSchema
{
    public class ForumPost
    {
        public long ForumPostID { get; set; }
        public long? UserId { get; set; }
        public long? CommunityID { get; set; }
        
        // Post Type
        public string PostType { get; set; } = "text"; // text, image, link, video
        
        // Content
        public string Title { get; set; }
        public string Content { get; set; }
        public string Slug { get; set; }
        
        // Link Post Fields
        public string LinkUrl { get; set; }
        public string LinkTitle { get; set; }
        public string LinkDescription { get; set; }
        public string LinkThumbnail { get; set; }
        
        // Engagement
        public int ViewCount { get; set; } = 0;
        public int Score { get; set; } = 0;
        public int UpvoteCount { get; set; } = 0;
        public int DownvoteCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        
        // Status
        public bool IsPinned { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public string PostStatus { get; set; } = "published";
        
        // Timestamps
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        // Navigation
        public ICollection<PostMedia> Media { get; set; } = new List<PostMedia>();
        public ICollection<ForumPostVote> Votes { get; set; } = new List<ForumPostVote>();
    }
}

// Models/NewSchema/PostMedia.cs
namespace GsmsharingV2.Models.NewSchema
{
    public class PostMedia
    {
        public long MediaID { get; set; }
        public long? PostID { get; set; }
        public long? ForumPostID { get; set; }
        public string MediaType { get; set; } // image, video, gif
        public string MediaUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string AltText { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public long? FileSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime? CreatedAt { get; set; }
        
        // Navigation
        public ForumPost ForumPost { get; set; }
    }
}

// Models/NewSchema/ForumPostVote.cs
namespace GsmsharingV2.Models.NewSchema
{
    public class ForumPostVote
    {
        public long VoteID { get; set; }
        public long ForumPostID { get; set; }
        public long UserId { get; set; }
        public string VoteType { get; set; } // upvote, downvote
        public DateTime? CreatedAt { get; set; }
        
        // Navigation
        public ForumPost ForumPost { get; set; }
    }
}
```

### 1.3 Update NewApplicationDbContext

```csharp
// Add to Database/NewApplicationDbContext.cs
public DbSet<ForumPost> ForumPosts { get; set; }
public DbSet<PostMedia> PostMedia { get; set; }
public DbSet<ForumPostVote> ForumPostVotes { get; set; }

// In OnModelCreating:
builder.Entity<ForumPost>(entity =>
{
    entity.ToTable("ForumPosts");
    entity.HasKey(e => e.ForumPostID);
    entity.Property(e => e.ForumPostID).UseIdentityColumn();
    entity.HasIndex(e => e.CommunityID);
    entity.HasIndex(e => e.UserId);
    entity.HasIndex(e => e.PostType);
    entity.HasIndex(e => e.CreatedAt);
    entity.HasIndex(e => e.Score);
});

builder.Entity<PostMedia>(entity =>
{
    entity.ToTable("PostMedia");
    entity.HasKey(e => e.MediaID);
    entity.Property(e => e.MediaID).UseIdentityColumn();
    entity.HasIndex(e => e.ForumPostID);
});

builder.Entity<ForumPostVote>(entity =>
{
    entity.ToTable("ForumPostVotes");
    entity.HasKey(e => e.VoteID);
    entity.Property(e => e.VoteID).UseIdentityColumn();
    entity.HasIndex(e => new { e.ForumPostID, e.UserId }).IsUnique();
});
```

### 1.4 Repository & Service Layer

#### Create IForumPostRepository.cs

```csharp
namespace GsmsharingV2.Interfaces
{
    public interface IForumPostRepository
    {
        Task<ForumPost?> GetByIdAsync(long id);
        Task<ForumPost?> GetBySlugAsync(string slug);
        Task<IEnumerable<ForumPost>> GetAllAsync();
        Task<IEnumerable<ForumPost>> GetByCommunityIdAsync(long communityId);
        Task<IEnumerable<ForumPost>> GetByPostTypeAsync(string postType);
        Task<IEnumerable<ForumPost>> GetHotPostsAsync(int count = 20);
        Task<IEnumerable<ForumPost>> GetTopPostsAsync(int count = 20);
        Task<IEnumerable<ForumPost>> GetNewPostsAsync(int count = 20);
        Task<ForumPost> CreateAsync(ForumPost post);
        Task<ForumPost> UpdateAsync(ForumPost post);
        Task DeleteAsync(long id);
        Task IncrementViewCountAsync(long id);
        Task VoteAsync(long postId, long userId, string voteType);
        Task RemoveVoteAsync(long postId, long userId);
    }
}
```

### 1.5 Controller Updates

#### Create/Update ForumController.cs

```csharp
[Authorize]
[HttpPost]
public async Task<IActionResult> Create(CreateForumPostViewModel model)
{
    var post = new ForumPost
    {
        UserId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
        CommunityID = model.CommunityID,
        PostType = model.PostType, // "text", "image", "link"
        Title = model.Title,
        Content = model.Content,
        LinkUrl = model.LinkUrl,
        LinkTitle = model.LinkTitle,
        LinkDescription = model.LinkDescription,
        PostStatus = "published"
    };
    
    // Handle media uploads for image posts
    if (model.PostType == "image" && model.MediaFiles != null)
    {
        foreach (var file in model.MediaFiles)
        {
            var mediaUrl = await _imageUploadService.UploadImageAsync(file, "forum-posts");
            post.Media.Add(new PostMedia
            {
                MediaType = "image",
                MediaUrl = mediaUrl,
                DisplayOrder = post.Media.Count
            });
        }
    }
    
    // Handle link preview for link posts
    if (model.PostType == "link" && !string.IsNullOrEmpty(model.LinkUrl))
    {
        var linkPreview = await _linkPreviewService.GetLinkPreviewAsync(model.LinkUrl);
        post.LinkTitle = linkPreview.Title;
        post.LinkDescription = linkPreview.Description;
        post.LinkThumbnail = linkPreview.ImageUrl;
    }
    
    var createdPost = await _forumPostService.CreateAsync(post);
    return RedirectToAction("Details", new { id = createdPost.ForumPostID });
}
```

### 1.6 Reddit-Style UI Components

#### Create Views/Forum/RedditStyle.cshtml

```html
<!-- Reddit-style post card -->
<article class="reddit-post-card">
    <div class="post-voting">
        <button class="vote-btn upvote" data-post-id="@post.ForumPostID">
            <i class="fas fa-arrow-up"></i>
        </button>
        <span class="post-score">@post.Score</span>
        <button class="vote-btn downvote" data-post-id="@post.ForumPostID">
            <i class="fas fa-arrow-down"></i>
        </button>
    </div>
    
    <div class="post-content">
        <div class="post-header">
            <span class="community-name">r/@post.Community?.Name</span>
            <span class="post-author">u/@post.User?.UserName</span>
            <span class="post-time">@TimeAgo(post.CreatedAt)</span>
        </div>
        
        <h2 class="post-title">@post.Title</h2>
        
        @if (post.PostType == "image" && post.Media.Any())
        {
            <div class="post-images">
                @foreach (var media in post.Media)
                {
                    <img src="@media.MediaUrl" alt="@media.AltText" />
                }
            </div>
        }
        
        @if (post.PostType == "link")
        {
            <div class="link-preview">
                <a href="@post.LinkUrl" target="_blank">
                    <img src="@post.LinkThumbnail" alt="@post.LinkTitle" />
                    <div class="link-info">
                        <h3>@post.LinkTitle</h3>
                        <p>@post.LinkDescription</p>
                        <span class="link-domain">@new Uri(post.LinkUrl).Host</span>
                    </div>
                </a>
            </div>
        }
        
        @if (post.PostType == "text" && !string.IsNullOrEmpty(post.Content))
        {
            <div class="post-text">@Html.Raw(post.Content)</div>
        }
        
        <div class="post-actions">
            <button class="action-btn">
                <i class="fas fa-comment"></i>
                <span>@post.CommentCount Comments</span>
            </button>
            <button class="action-btn">
                <i class="fas fa-share"></i>
                <span>Share</span>
            </button>
            <button class="action-btn">
                <i class="fas fa-bookmark"></i>
                <span>Save</span>
            </button>
        </div>
    </div>
</article>
```

---

## Part 2: Affiliate Marketing - Amazon & AliExpress

### 2.1 Database Schema Updates

#### Update Affiliate Tables (Already exists, but need enhancements)

```sql
-- Update AffiliatePartner table
ALTER TABLE AffiliatePartners
ADD 
    PartnerType NVARCHAR(50) NOT NULL DEFAULT 'amazon', -- 'amazon', 'aliexpress', 'other'
    ApiKey NVARCHAR(500) NULL,
    ApiSecret NVARCHAR(500) NULL,
    TrackingId NVARCHAR(100) NULL, -- Amazon Associate Tag / AliExpress PID
    CommissionRate DECIMAL(5,2) NULL,
    IsActive BIT DEFAULT 1;

-- Add Amazon-specific fields
ALTER TABLE AffiliateProducts
ADD
    ASIN NVARCHAR(20) NULL, -- Amazon ASIN
    AliExpressProductId NVARCHAR(50) NULL, -- AliExpress Product ID
    OriginalPrice DECIMAL(10,2) NULL,
    DiscountPrice DECIMAL(10,2) NULL,
    Currency NVARCHAR(10) DEFAULT 'USD',
    Rating DECIMAL(3,2) NULL,
    ReviewCount INT NULL,
    Availability NVARCHAR(50) NULL,
    PrimeEligible BIT NULL, -- Amazon Prime
    BestSeller BIT NULL,
    AmazonChoice BIT NULL,
    ProductCategory NVARCHAR(100) NULL,
    Brand NVARCHAR(100) NULL;

-- Create Affiliate Link Tracking
CREATE TABLE AffiliateLinkClicks (
    ClickID BIGINT IDENTITY(1,1) PRIMARY KEY,
    ProductID BIGINT NOT NULL,
    UserId BIGINT NULL,
    IPAddress NVARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL,
    ReferrerUrl NVARCHAR(1000) NULL,
    ClickDate DATETIME2 DEFAULT GETUTCDATE(),
    Converted BIT DEFAULT 0, -- If purchase was made
    ConversionDate DATETIME2 NULL,
    CommissionAmount DECIMAL(10,2) NULL,
    
    INDEX IX_AffiliateLinkClicks_ProductID (ProductID),
    INDEX IX_AffiliateLinkClicks_UserId (UserId),
    INDEX IX_AffiliateLinkClicks_ClickDate (ClickDate)
);
```

### 2.2 Model Updates

#### Update AffiliateProductNew.cs

```csharp
namespace GsmsharingV2.Models.NewSchema
{
    public class AffiliateProductNew
    {
        public long ProductID { get; set; }
        public int? PartnerID { get; set; }
        
        public string Title { get; set; }
        public string Category { get; set; }
        
        // Links
        public string OriginalLink { get; set; }
        public string AffiliateLink { get; set; }
        
        // Media
        public string ImageUrl { get; set; }
        
        // Pricing
        public decimal? PriceDisplay { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string Currency { get; set; } = "USD";
        
        // Amazon-specific
        public string ASIN { get; set; }
        public decimal? Rating { get; set; }
        public int? ReviewCount { get; set; }
        public string Availability { get; set; }
        public bool? PrimeEligible { get; set; }
        public bool? BestSeller { get; set; }
        public bool? AmazonChoice { get; set; }
        public string ProductCategory { get; set; }
        public string Brand { get; set; }
        
        // AliExpress-specific
        public string AliExpressProductId { get; set; }
        
        // Stats
        public int Clicks { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation
        public AffiliatePartner Partner { get; set; }
        public ICollection<AffiliateClick> ClicksData { get; set; } = new List<AffiliateClick>();
    }
}

// Update AffiliatePartner.cs
namespace GsmsharingV2.Models.NewSchema
{
    public class AffiliatePartner
    {
        public int PartnerID { get; set; }
        public string Name { get; set; }
        public string PartnerType { get; set; } = "amazon"; // amazon, aliexpress
        public string AffiliateTag { get; set; } // Amazon Associate Tag
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string TrackingId { get; set; }
        public decimal? CommissionRate { get; set; }
        public bool IsActive { get; set; } = true;
        
        public ICollection<AffiliateProductNew> Products { get; set; } = new List<AffiliateProductNew>();
    }
}
```

### 2.3 Affiliate Link Generation Service

#### Create Services/AffiliateLinkService.cs

```csharp
namespace GsmsharingV2.Services
{
    public interface IAffiliateLinkService
    {
        string GenerateAmazonLink(string productUrl, string associateTag);
        string GenerateAliExpressLink(string productUrl, string pid);
        Task<string> GetProductInfoAsync(string productUrl, string partnerType);
        Task TrackClickAsync(long productId, long? userId, HttpContext httpContext);
    }
    
    public class AffiliateLinkService : IAffiliateLinkService
    {
        private readonly IConfiguration _configuration;
        private readonly NewApplicationDbContext _context;
        
        public AffiliateLinkService(IConfiguration configuration, NewApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        
        public string GenerateAmazonLink(string productUrl, string associateTag)
        {
            if (string.IsNullOrEmpty(associateTag))
                return productUrl;
                
            var uri = new Uri(productUrl);
            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            queryParams["tag"] = associateTag;
            
            var newUri = new UriBuilder(uri)
            {
                Query = queryParams.ToString()
            };
            
            return newUri.ToString();
        }
        
        public string GenerateAliExpressLink(string productUrl, string pid)
        {
            if (string.IsNullOrEmpty(pid))
                return productUrl;
                
            // AliExpress affiliate format: ?p=YOUR_PID
            var uri = new Uri(productUrl);
            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            queryParams["p"] = pid;
            
            var newUri = new UriBuilder(uri)
            {
                Query = queryParams.ToString()
            };
            
            return newUri.ToString();
        }
        
        public async Task TrackClickAsync(long productId, long? userId, HttpContext httpContext)
        {
            var click = new AffiliateClick
            {
                ProductID = productId,
                UserId = userId,
                IPAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext.Request.Headers["User-Agent"].ToString(),
                ReferrerUrl = httpContext.Request.Headers["Referer"].ToString(),
                ClickDate = DateTime.UtcNow
            };
            
            _context.AffiliateClicks.Add(click);
            
            // Update product click count
            var product = await _context.AffiliateProducts.FindAsync(productId);
            if (product != null)
            {
                product.Clicks++;
            }
            
            await _context.SaveChangesAsync();
        }
    }
}
```

### 2.4 Amazon Product API Integration

#### Create Services/AmazonProductService.cs

```csharp
namespace GsmsharingV2.Services
{
    public interface IAmazonProductService
    {
        Task<AmazonProductInfo> GetProductByASINAsync(string asin);
        Task<AmazonProductInfo> SearchProductsAsync(string keyword, int count = 10);
    }
    
    public class AmazonProductService : IAmazonProductService
    {
        // Use Amazon Product Advertising API (PA-API 5.0)
        // Or use third-party services like RapidAPI Amazon Product API
        
        public async Task<AmazonProductInfo> GetProductByASINAsync(string asin)
        {
            // Implementation using Amazon PA-API or third-party service
            // This requires API credentials from Amazon Associates
        }
    }
}
```

### 2.5 AliExpress Product Integration

#### Create Services/AliExpressProductService.cs

```csharp
namespace GsmsharingV2.Services
{
    public interface IAliExpressProductService
    {
        Task<AliExpressProductInfo> GetProductByIdAsync(string productId);
        Task<AliExpressProductInfo> SearchProductsAsync(string keyword);
    }
    
    public class AliExpressProductService : IAliExpressProductService
    {
        // Use AliExpress Affiliate API
        // Requires API credentials from AliExpress Affiliate Program
        
        public async Task<AliExpressProductInfo> GetProductByIdAsync(string productId)
        {
            // Implementation using AliExpress API
        }
    }
}
```

### 2.6 UI Components for Affiliate Products

#### Create ViewComponents/AffiliateProductCard.cs

```csharp
namespace GsmsharingV2.ViewComponents
{
    public class AffiliateProductCard : ViewComponent
    {
        public IViewComponentResult Invoke(AffiliateProductNew product)
        {
            return View(product);
        }
    }
}
```

#### Create Views/Shared/Components/AffiliateProductCard/Default.cshtml

```html
@model GsmsharingV2.Models.NewSchema.AffiliateProductNew

<div class="affiliate-product-card">
    <div class="product-image">
        <img src="@Model.ImageUrl" alt="@Model.Title" />
        @if (Model.PrimeEligible == true)
        {
            <span class="prime-badge">Prime</span>
        }
        @if (Model.BestSeller == true)
        {
            <span class="bestseller-badge">Best Seller</span>
        }
    </div>
    
    <div class="product-info">
        <h3 class="product-title">@Model.Title</h3>
        
        @if (Model.Rating.HasValue)
        {
            <div class="product-rating">
                @for (int i = 0; i < 5; i++)
                {
                    <i class="fas fa-star @(i < (int)Model.Rating ? "filled" : "")"></i>
                }
                <span>(@Model.ReviewCount reviews)</span>
            </div>
        }
        
        <div class="product-price">
            @if (Model.DiscountPrice.HasValue && Model.OriginalPrice.HasValue)
            {
                <span class="current-price">@Model.Currency @Model.DiscountPrice</span>
                <span class="original-price">@Model.OriginalPrice</span>
                <span class="discount">
                    @((int)((1 - Model.DiscountPrice.Value / Model.OriginalPrice.Value) * 100))% off
                </span>
            }
            else if (Model.PriceDisplay.HasValue)
            {
                <span class="current-price">@Model.Currency @Model.PriceDisplay</span>
            }
        </div>
        
        <div class="product-availability">
            @if (!string.IsNullOrEmpty(Model.Availability))
            {
                <span class="availability-badge">@Model.Availability</span>
            }
        </div>
        
        <a href="@Model.AffiliateLink" 
           target="_blank" 
           rel="nofollow sponsored"
           class="btn btn-primary buy-button"
           data-product-id="@Model.ProductID">
            <i class="fas fa-shopping-cart"></i>
            Buy Now
        </a>
        
        <div class="affiliate-disclosure">
            <small>As an Amazon Associate, we earn from qualifying purchases.</small>
        </div>
    </div>
</div>
```

---

## Implementation Checklist

### Phase 1: Database & Models (Week 1)
- [ ] Create ForumPosts table in new database
- [ ] Create PostMedia table
- [ ] Create ForumPostVotes table
- [ ] Update AffiliatePartner table
- [ ] Update AffiliateProductNew table
- [ ] Create AffiliateLinkClicks table
- [ ] Create C# models for all new tables
- [ ] Update NewApplicationDbContext

### Phase 2: Repository & Services (Week 1-2)
- [ ] Create IForumPostRepository
- [ ] Implement ForumPostRepository
- [ ] Create IForumPostService
- [ ] Implement ForumPostService
- [ ] Create IAffiliateLinkService
- [ ] Implement AffiliateLinkService
- [ ] Create ILinkPreviewService (for link posts)
- [ ] Implement LinkPreviewService

### Phase 3: Controllers & API (Week 2)
- [ ] Update ForumController for Reddit-style posts
- [ ] Add Create action with post type support
- [ ] Add Vote action for upvote/downvote
- [ ] Create AffiliateController
- [ ] Add product import endpoints
- [ ] Add click tracking endpoint

### Phase 4: UI Components (Week 2-3)
- [ ] Create Reddit-style post card component
- [ ] Create post creation form with type selector
- [ ] Create image upload component
- [ ] Create link preview component
- [ ] Create voting UI component
- [ ] Create affiliate product card component
- [ ] Create affiliate product listing page

### Phase 5: Integration (Week 3-4)
- [ ] Integrate Amazon Product API (or third-party)
- [ ] Integrate AliExpress Affiliate API
- [ ] Set up affiliate link generation
- [ ] Implement click tracking
- [ ] Add affiliate disclosure components

### Phase 6: Testing & Polish (Week 4)
- [ ] Test all post types (text, image, link)
- [ ] Test voting system
- [ ] Test affiliate link generation
- [ ] Test click tracking
- [ ] Mobile responsiveness
- [ ] Performance optimization

---

## Configuration

### appsettings.json Updates

```json
{
  "AffiliateMarketing": {
    "Amazon": {
      "AssociateTag": "your-associate-tag",
      "ApiKey": "your-api-key",
      "ApiSecret": "your-api-secret",
      "IsActive": true
    },
    "AliExpress": {
      "Pid": "your-pid",
      "ApiKey": "your-api-key",
      "ApiSecret": "your-api-secret",
      "IsActive": true
    }
  },
  "LinkPreview": {
    "ApiKey": "your-link-preview-api-key",
    "Provider": "rapidapi" // or "microlink", "opengraph"
  }
}
```

---

## Key Features Summary

### Reddit-Style Forums
✅ Multiple post types (text, image, link, video)  
✅ Image uploads with multiple images per post  
✅ Link posts with automatic preview  
✅ Upvote/Downvote system  
✅ Score calculation  
✅ Hot/Top/New sorting  
✅ Reddit-style UI/UX  

### Affiliate Marketing
✅ Amazon integration  
✅ AliExpress integration  
✅ Automatic affiliate link generation  
✅ Click tracking  
✅ Product import from APIs  
✅ Product cards with ratings, prices  
✅ Commission tracking  
✅ Affiliate disclosure  

---

**Last Updated**: 2025-01-26  
**Version**: 1.0  
**Status**: Implementation Plan














