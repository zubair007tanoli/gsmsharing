# 🏗️ GSMSharing - Architecture Overview

## 📐 System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           CLIENT LAYER                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌─────────────┐│
│  │ Web Browser  │  │ Mobile Web   │  │ REST API     │  │ Mobile App  ││
│  │ (Desktop)    │  │ (Responsive) │  │ (Future)     │  │ (Future)    ││
│  └──────────────┘  └──────────────┘  └──────────────┘  └─────────────┘│
└─────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                      PRESENTATION LAYER (MVC)                           │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                        Controllers                               │   │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐│   │
│  │  │  Home    │ │  Forum   │ │  Market  │ │Community │ │ Admin  ││   │
│  │  │Controller│ │Controller│ │Controller│ │Controller│ │Controller   │
│  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └────────┘│   │
│  └─────────────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                        ViewModels                                │   │
│  │  (Data Transfer between Controllers and Views)                  │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                          Views                                   │   │
│  │  (Razor Pages, Components, Partials)                            │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                      BUSINESS LOGIC LAYER                               │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                         Services                                 │   │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐│   │
│  │  │  User    │ │  Forum   │ │   Ads    │ │  Specs   │ │  SEO   ││   │
│  │  │ Service  │ │ Service  │ │ Service  │ │ Service  │ │Service ││   │
│  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └────────┘│   │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐           │   │
│  │  │  Email   │ │ Notif.   │ │ Search   │ │Analytics │           │   │
│  │  │ Service  │ │ Service  │ │ Service  │ │ Service  │           │   │
│  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘           │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                    Business Logic                                │   │
│  │  • Validation Rules                                              │   │
│  │  • Business Rules & Workflows                                    │   │
│  │  • Data Transformation                                           │   │
│  │  • Authorization Logic                                           │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                       DATA ACCESS LAYER                                 │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                     Unit of Work Pattern                         │   │
│  │  (Manages transactions across multiple repositories)            │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                      Repositories                                │   │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐│   │
│  │  │  Post    │ │  Forum   │ │   Ads    │ │Community │ │  User  ││   │
│  │  │   Repo   │ │   Repo   │ │   Repo   │ │   Repo   │ │  Repo  ││   │
│  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └────────┘│   │
│  └─────────────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                   Entity Framework Core                          │   │
│  │  • DbContext (ApplicationDbContext)                             │   │
│  │  • Entity Models (Domain Models)                                │   │
│  │  • LINQ Queries                                                 │   │
│  │  • Change Tracking                                              │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                      INFRASTRUCTURE LAYER                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌─────────────┐│
│  │   Caching    │  │ File Storage │  │  External    │  │  Logging    ││
│  │   (Redis)    │  │  (Azure/S3)  │  │     APIs     │  │  (Serilog)  ││
│  └──────────────┘  └──────────────┘  └──────────────┘  └─────────────┘│
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌─────────────┐│
│  │    Email     │  │   SignalR    │  │   Hangfire   │  │  Monitoring ││
│  │   Service    │  │ (Real-time)  │  │ (Background) │  │  (App Ins.) ││
│  └──────────────┘  └──────────────┘  └──────────────┘  └─────────────┘│
└─────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                         DATABASE LAYER                                  │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                    SQL Server Database                           │   │
│  │                    (gsmsharing - 50+ tables)                     │   │
│  │                                                                  │   │
│  │  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌───────┐│   │
│  │  │  Users  │  │  Posts  │  │ Forums  │  │  Ads    │  │Specs  ││   │
│  │  │Identity │  │ Blogs   │  │Comments │  │ Parts   │  │Reviews││   │
│  │  └─────────┘  └─────────┘  └─────────┘  └─────────┘  └───────┘│   │
│  │                                                                  │   │
│  │  Features: Indexes, Full-Text Search, Constraints, Query Store  │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Request Flow Diagram

### **Typical User Request Flow**

```
User Action (Click "View Post")
         ↓
┌─────────────────────┐
│  1. HTTP Request    │
│  GET /posts/123     │
└─────────────────────┘
         ↓
┌─────────────────────┐
│  2. Routing         │
│  → PostsController  │
└─────────────────────┘
         ↓
┌─────────────────────┐
│  3. Controller      │
│  Action Method      │
│  Details(id)        │
└─────────────────────┘
         ↓
┌─────────────────────┐
│  4. Service Layer   │
│  PostService        │
│  GetPostById(id)    │
└─────────────────────┘
         ↓
    ┌─────────┐
    │ Cache?  │
    └─────────┘
    ↙         ↘
  Yes          No
   ↓            ↓
Return     ┌─────────────────────┐
Cached     │  5. Repository      │
Data       │  PostRepository     │
           │  GetByIdAsync(id)   │
           └─────────────────────┘
                    ↓
           ┌─────────────────────┐
           │  6. EF Core         │
           │  LINQ Query         │
           │  + Includes         │
           └─────────────────────┘
                    ↓
           ┌─────────────────────┐
           │  7. SQL Server      │
           │  Execute Query      │
           │  (with indexes)     │
           └─────────────────────┘
                    ↓
           ┌─────────────────────┐
           │  8. Map to DTO      │
           │  Entity → ViewModel │
           └─────────────────────┘
                    ↓
                Cache Data
                    ↓
         ┌─────────────────────┐
         │  9. Return to       │
         │  Controller         │
         └─────────────────────┘
                    ↓
         ┌─────────────────────┐
         │  10. Pass ViewModel │
         │  to View            │
         └─────────────────────┘
                    ↓
         ┌─────────────────────┐
         │  11. Razor Engine   │
         │  Render HTML        │
         └─────────────────────┘
                    ↓
         ┌─────────────────────┐
         │  12. HTTP Response  │
         │  Return HTML        │
         └─────────────────────┘
                    ↓
         ┌─────────────────────┐
         │  13. User Sees Page │
         └─────────────────────┘
```

---

## 🗂️ Domain Model Structure

### **Core Domain Entities**

```
┌──────────────────────────────────────────────────────────────┐
│                      User Domain                             │
├──────────────────────────────────────────────────────────────┤
│ ApplicationUser                                              │
│  - Id, Username, Email, PasswordHash                         │
│  - ProfilePhoto, Bio, Skills, Reputation                     │
│  - CreatedAt, LastLoginAt                                    │
│                                                              │
│ UserProfile                                                  │
│  - ProfileId, UserId, Bio, Website, Location                │
│  - Skills, Specialization, SocialLinks                       │
│                                                              │
│ Relationships:                                               │
│  - Posts (1:Many)                                           │
│  - Comments (1:Many)                                        │
│  - Communities (Many:Many)                                  │
│  - Notifications (1:Many)                                   │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                    Content Domain                            │
├──────────────────────────────────────────────────────────────┤
│ Post                                                         │
│  - PostID, UserId, CommunityID, Title, Content              │
│  - Slug, Tags, FeaturedImage, ViewCount                     │
│  - PostStatus, IsFeatured, CreatedAt, PublishedAt           │
│                                                              │
│ Comment                                                      │
│  - CommentID, PostId, UserId, Content                       │
│  - ParentCommentId (for threading)                          │
│  - CreatedAt, UpdatedAt                                      │
│                                                              │
│ Reaction                                                     │
│  - ReactionID, PostId/CommentId, UserId                     │
│  - ReactionType (Like, Love, Helpful, etc.)                 │
│                                                              │
│ Relationships:                                               │
│  - Post → User (Many:1)                                     │
│  - Post → Community (Many:1)                                │
│  - Post → Comments (1:Many)                                 │
│  - Post → Reactions (1:Many)                                │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                    Forum Domain                              │
├──────────────────────────────────────────────────────────────┤
│ UsersFourm (Thread)                                          │
│  - UserFourmID, UserId, Title, Content                      │
│  - Tags, MetaDescription, Views, Likes                       │
│  - ParantId, Publish, CreationDate                          │
│                                                              │
│ ForumCategory                                                │
│  - CategoryId, UserFourmID, CategoryName                    │
│  - ParantId, CreationDate                                    │
│                                                              │
│ ForumReplys                                                  │
│  - Id, ThreadId, UserId, ForumContent                       │
│  - Like, DisLike, Views, PublishDate                        │
│                                                              │
│ Relationships:                                               │
│  - Thread → User (Many:1)                                   │
│  - Thread → Category (Many:1)                               │
│  - Thread → Replies (1:Many)                                │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                  Marketplace Domain                          │
├──────────────────────────────────────────────────────────────┤
│ MobileAds                                                    │
│  - AdsId, UserId, Title, Description, Price                 │
│  - Tags, Views, Likes, Publish, CreationDate                │
│                                                              │
│ MobilePartAds                                                │
│  - MobileAdsId, UserId, Title, Description                  │
│  - Price, Tags, Views, CreationDate                         │
│                                                              │
│ AdsImage                                                     │
│  - SalePicId, AdsId, Pics (varbinary), ImageDate           │
│                                                              │
│ AdCategory                                                   │
│  - Id, CategoryName, CreationDate                           │
│                                                              │
│ Relationships:                                               │
│  - Ad → User (Many:1)                                       │
│  - Ad → Category (Many:Many)                                │
│  - Ad → Images (1:Many)                                     │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                Mobile Specs Domain                           │
├──────────────────────────────────────────────────────────────┤
│ MobileSpecs                                                  │
│  - Specid, UserId, ModelName, NetworkInfo                   │
│  - Launched, Body, Display, OS, Processor                   │
│  - Memory, MainCamera, SelfiCam, Battery                    │
│  - Price, MetaInfo, Tags                                     │
│                                                              │
│ BlogSpecContainer                                            │
│  - ContainerId, BlogId, SpecId                              │
│  (Links blog posts to mobile specs)                         │
│                                                              │
│ Relationships:                                               │
│  - Spec → User (Many:1)                                     │
│  - Spec → Posts (Many:Many via container)                   │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                   Community Domain                           │
├──────────────────────────────────────────────────────────────┤
│ Community                                                    │
│  - CommunityID, Name, Slug, Description                     │
│  - Rules, CoverImage, IconImage                             │
│  - CreatorId, IsPrivate, MemberCount                        │
│  - CategoryID, CreatedAt                                     │
│                                                              │
│ CommunityMember                                              │
│  - UserId, CommunityId, JoinedAt                            │
│  - Role (Admin, Moderator, Member)                          │
│                                                              │
│ ChatRoom                                                     │
│  - ChatRoomID, CommunityID, Name                            │
│  - IsPrivate, CreatedAt                                      │
│                                                              │
│ Relationships:                                               │
│  - Community → Creator (Many:1)                             │
│  - Community → Members (Many:Many)                          │
│  - Community → Posts (1:Many)                               │
│  - Community → ChatRooms (1:Many)                           │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                     Blog Domain                              │
├──────────────────────────────────────────────────────────────┤
│ GsmBlog                                                      │
│  - BlogId, UserId, BlogTitle, BlogContent                   │
│  - BlogKeywords, BlogViews, BlogLikes                        │
│  - Publish, PublishDate, CategoryId                         │
│  - ThumbNailLink                                             │
│                                                              │
│ GsmBlogCategory                                              │
│  - CategoryId, Name, ParantId                               │
│                                                              │
│ GsmBlogComments                                              │
│  - CommentId, UserId, BlogId, Comment                       │
│  - PublishDate                                               │
│                                                              │
│ BlogSEO                                                      │
│  - SEOID, BlogId, BlogDiscription                           │
│  - BlogKeywords, Canonical                                   │
│                                                              │
│ Relationships:                                               │
│  - Blog → User (Many:1)                                     │
│  - Blog → Category (Many:1)                                 │
│  - Blog → Comments (1:Many)                                 │
│  - Blog → SEO (1:1)                                         │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                Code Sharing Domain                           │
├──────────────────────────────────────────────────────────────┤
│ gsmsharing.code                                              │
│  - idcode, UserId, title, content                           │
│  - Tags, likes, dislike, views                              │
│  - published, publishdate                                    │
│                                                              │
│ gsmsharing.codecategory                                      │
│  - catid, CatName, PublishDate                              │
│                                                              │
│ gsmsharing.codecomments                                      │
│  - commentid, Userid, codeid, comment                       │
│  - parantid, publishDate                                     │
│                                                              │
│ Relationships:                                               │
│  - Code → User (Many:1)                                     │
│  - Code → Category (Many:Many)                              │
│  - Code → Comments (1:Many)                                 │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│              Product Review Domain                           │
├──────────────────────────────────────────────────────────────┤
│ AmazonProducts                                               │
│  - product_id, asin, title, current_price                   │
│  - star_rating, num_ratings, url                            │
│  - is_best_seller, is_amazon_choice                         │
│                                                              │
│ Review                                                       │
│  - ReviewId, ReviewTitle, ReviewComment                     │
│  - ReviewStarRating, ReviewLink                             │
│  - ReviewAuthor, ReviewDate                                  │
│  - IsVerifiedPurchase, ProductAsin                          │
│                                                              │
│ ReviewImage                                                  │
│  - ReviewImageId, ReviewId, ImageUrl                        │
│                                                              │
│ AffiliationProgram                                           │
│  - ProductId, UserId, Title, Content                        │
│  - Price, ImageLink, BuyLink                                │
│  - Views, Likes, DisLikes                                    │
│                                                              │
│ Relationships:                                               │
│  - Product → Reviews (1:Many)                               │
│  - Review → Images (1:Many)                                 │
│  - AffiliateProduct → User (Many:1)                         │
└──────────────────────────────────────────────────────────────┘
```

---

## 🔧 Technical Implementation Details

### **Dependency Injection Container Setup**

```csharp
// Program.cs - Service Registration

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IForumRepository, ForumRepository>();
builder.Services.AddScoped<IAdsRepository, AdsRepository>();
builder.Services.AddScoped<ICommunityRepository, CommunityRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IReactionRepository, ReactionRepository>();
// ... more repositories

// Services
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IForumService, ForumService>();
builder.Services.AddScoped<IAdsService, AdsService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ISeoService, SeoService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
// ... more services

// Infrastructure
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IAIContentGenerator, AIContentGenerator>();

// Caching
builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = configuration["Redis:Configuration"];
});

// Background Jobs
builder.Services.AddHangfire(config => 
    config.UseSqlServerStorage(connectionString));

// SignalR (Real-time)
builder.Services.AddSignalR();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
```

---

### **Repository Pattern Implementation**

```csharp
// Base Repository Interface
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}

// Specific Repository Interface
public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetPublishedPostsAsync(int pageNumber, int pageSize);
    Task<IEnumerable<Post>> GetPostsByCommunityAsync(int communityId);
    Task<IEnumerable<Post>> GetPostsByUserAsync(string userId);
    Task<IEnumerable<Post>> GetTrendingPostsAsync();
    Task<Post> GetPostWithDetailsAsync(int postId);
    Task IncrementViewCountAsync(int postId);
}

// Unit of Work Interface
public interface IUnitOfWork : IDisposable
{
    IPostRepository Posts { get; }
    IForumRepository Forums { get; }
    IAdsRepository Ads { get; }
    ICommunityRepository Communities { get; }
    IUserRepository Users { get; }
    ICommentRepository Comments { get; }
    // ... more repositories
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

---

### **Service Layer Pattern**

```csharp
public interface IPostService
{
    Task<PostViewModel> GetPostByIdAsync(int postId);
    Task<IEnumerable<PostViewModel>> GetAllPostsAsync(int pageNumber, int pageSize);
    Task<PostViewModel> CreatePostAsync(CreatePostViewModel model, string userId);
    Task<bool> UpdatePostAsync(int postId, EditPostViewModel model);
    Task<bool> DeletePostAsync(int postId, string userId);
    Task<IEnumerable<PostViewModel>> GetTrendingPostsAsync();
    Task<IEnumerable<PostViewModel>> SearchPostsAsync(string searchTerm);
    Task<bool> ReactToPostAsync(int postId, string userId, ReactionType reactionType);
}

public class PostService : IPostService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    private readonly INotificationService _notificationService;
    private readonly ISeoService _seoService;
    
    public PostService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cache,
        INotificationService notificationService,
        ISeoService seoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _notificationService = notificationService;
        _seoService = seoService;
    }
    
    public async Task<PostViewModel> GetPostByIdAsync(int postId)
    {
        // Check cache first
        var cacheKey = $"post_{postId}";
        var cachedPost = await _cache.GetAsync<PostViewModel>(cacheKey);
        if (cachedPost != null) return cachedPost;
        
        // Get from database
        var post = await _unitOfWork.Posts.GetPostWithDetailsAsync(postId);
        if (post == null) return null;
        
        // Increment view count (async, don't wait)
        _ = _unitOfWork.Posts.IncrementViewCountAsync(postId);
        
        // Map to ViewModel
        var viewModel = _mapper.Map<PostViewModel>(post);
        
        // Cache the result
        await _cache.SetAsync(cacheKey, viewModel, TimeSpan.FromMinutes(10));
        
        return viewModel;
    }
    
    // ... more methods
}
```

---

## 🔐 Security Architecture

### **Authentication Flow**

```
User Login Request
      ↓
Check Credentials (ASP.NET Identity)
      ↓
Generate Authentication Cookie
      ↓
Set HttpOnly, Secure, SameSite Cookies
      ↓
Return to User
      ↓
Subsequent Requests Include Cookie
      ↓
Middleware Validates Cookie
      ↓
Set User Principal/Claims
      ↓
Authorize Controller Action
```

### **Authorization Layers**

1. **Controller Level:** `[Authorize(Roles = "Admin")]`
2. **Action Level:** `[Authorize(Policy = "CanModerateContent")]`
3. **Service Level:** Check user permissions in business logic
4. **View Level:** `@if (User.IsInRole("Admin"))`

---

## 📦 Caching Strategy

### **Multi-Level Caching**

```
┌─────────────────────────────────────┐
│      Browser Cache (Client)         │
│  Static assets, CSS, JS, Images     │
│  Cache-Control headers               │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│      CDN Cache (Edge)                │
│  Static content delivery             │
│  CloudFlare / Azure CDN              │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   Application Cache (Memory)         │
│  Frequently accessed data            │
│  Short TTL (5-15 minutes)            │
│  MemoryCache (In-Process)            │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  Distributed Cache (Redis)           │
│  Shared across instances             │
│  User sessions, temp data            │
│  Medium TTL (30-60 minutes)          │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│        Database (SQL Server)         │
│  Persistent storage                  │
│  Query Store enabled                 │
│  Indexed & optimized                 │
└─────────────────────────────────────┘
```

### **Cache Invalidation Strategy**

- **Time-based:** Set TTL based on content type
- **Event-based:** Invalidate on updates
- **Manual:** Admin can clear specific caches
- **Stampede prevention:** Lock pattern for cache refresh

---

## 🎨 Frontend Architecture

### **View Component Structure**

```
Views/
├── Shared/
│   ├── _Layout.cshtml (Main layout)
│   ├── _LoginPartial.cshtml (Auth status)
│   ├── _Sidebar.cshtml (Navigation)
│   ├── _Footer.cshtml
│   └── Components/
│       ├── FeaturedPosts/
│       │   └── Default.cshtml
│       ├── PopularTags/
│       │   └── Default.cshtml
│       ├── UserStats/
│       │   └── Default.cshtml
│       └── Notifications/
│           └── Default.cshtml
├── Home/
│   ├── Index.cshtml (Homepage)
│   └── Privacy.cshtml
├── Posts/
│   ├── Index.cshtml (List)
│   ├── Details.cshtml (View)
│   ├── Create.cshtml
│   └── Edit.cshtml
├── Forum/
│   ├── Index.cshtml
│   ├── Thread.cshtml
│   └── Create.cshtml
└── ... (more feature views)
```

---

## 📱 Responsive Design Approach

- **Mobile-First:** Design for mobile, enhance for desktop
- **Breakpoints:** 
  - Mobile: < 768px
  - Tablet: 768px - 1024px
  - Desktop: > 1024px
- **Progressive Enhancement:** Core functionality works everywhere
- **Touch-Friendly:** 44px minimum touch targets

---

## 🚀 Deployment Architecture

### **Production Environment**

```
┌──────────────────────────────────────┐
│        Load Balancer (Azure)         │
│     Distributes traffic evenly       │
└──────────────────────────────────────┘
         ↓                  ↓
┌─────────────────┐  ┌─────────────────┐
│  Web Server 1   │  │  Web Server 2   │
│  (ASP.NET Core) │  │  (ASP.NET Core) │
└─────────────────┘  └─────────────────┘
         ↓                  ↓
┌──────────────────────────────────────┐
│         Redis Cache Cluster          │
│     (Session & Distributed Cache)    │
└──────────────────────────────────────┘
         ↓                  ↓
┌─────────────────┐  ┌─────────────────┐
│  Primary DB     │  │  Read Replica   │
│  (SQL Server)   │  │  (SQL Server)   │
└─────────────────┘  └─────────────────┘
         ↓
┌──────────────────────────────────────┐
│         Azure Blob Storage           │
│     (Images, Files, Backups)         │
└──────────────────────────────────────┘
```

---

This architecture provides a **solid foundation** for building a scalable, maintainable, and high-performance mobile repair community platform! 🚀

