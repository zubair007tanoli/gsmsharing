# Database Architecture Guide

## Overview

GSMSharing V2 uses a **dual-database architecture** to separate legacy data from new content.

---

## Database Configuration

### Old Database: `gsmsharingv3`
- **Connection String**: `GsmsharingConnection`
- **DbContext**: `ApplicationDbContext`
- **Access Level**: **READ-ONLY**
- **Purpose**: Legacy data repository

### New Database: `gsmsharingv4`
- **Connection String**: `GsmsharingConnectionNew`
- **DbContext**: `NewApplicationDbContext`
- **Access Level**: **READ/WRITE**
- **Purpose**: All new content

---

## Database Access Rules

### ✅ Old Database (gsmsharingv3) - READ ONLY

**What to READ from old database:**
- Legacy forum threads and replies
- Legacy marketplace ads
- Legacy blog posts (MobilePost, GsmBlog)
- Legacy product reviews
- Legacy mobile specs
- Historical user data (if needed for reference)
- Legacy posts, comments, communities (for display only)

**What NOT to do:**
- ❌ No INSERT operations
- ❌ No UPDATE operations
- ❌ No DELETE operations
- ❌ No CREATE operations

**Use Cases:**
- Displaying historical content
- Showing legacy blog posts
- Referencing old forum discussions
- Displaying historical marketplace listings

### ✅ New Database (gsmsharingv4) - READ/WRITE

**What to WRITE to new database:**
- All new posts (including Reddit-style forum posts)
- All new comments
- All new communities
- All new classified ads
- All new chat messages
- All new file uploads
- All new affiliate products
- All new forum posts with photos and links
- All new post media attachments
- All new votes and reactions
- All new user-generated content

**What to READ from new database:**
- All new posts (including forum posts)
- All new comments
- All new communities
- All new classified ads
- All chat conversations
- All file repository items
- All affiliate products
- All forum posts (text, image, link types)
- All post media attachments
- All votes and engagement data
- System settings

---

## Table Mapping

### Old Database Tables (Read-Only)

| Table | Purpose | Access |
|-------|---------|--------|
| `userforum` | Forum threads | Read only |
| `ForumCategory` | Forum categories | Read only |
| `ForumReplys` | Forum replies | Read only |
| `FourmComments` | Forum comments | Read only |
| `MobileAds` | Mobile ads | Read only |
| `MobilePartAds` | Mobile part ads | Read only |
| `AdsImage` | Ad images | Read only |
| `MobileSpecs` | Mobile specifications | Read only |
| `MobilePosts` | Blog posts | Read only |
| `GsmBlog` | GSM blog posts | Read only |
| `AffiliationProgram` | Affiliate products (old) | Read only |
| `BlogSEO` | Blog SEO data | Read only |
| `BlogComments` | Blog comments | Read only |
| `ProductReview` | Product reviews | Read only |
| `BlogSpecContainer` | Blog spec containers | Read only |
| `Posts` | Legacy posts | Read only |
| `Comments` | Legacy comments | Read only |
| `Communities` | Legacy communities | Read only |
| `Categories` | Legacy categories | Read only |
| `Tags` | Legacy tags | Read only |
| `Reactions` | Legacy reactions | Read only |
| `UserProfiles` | Legacy user profiles | Read only |

### New Database Tables (Read/Write)

| Table | Purpose | Access |
|-------|---------|--------|
| `ClassifiedAds` | New classified ads | Read/Write |
| `AdCategories` | Ad categories | Read/Write |
| `AdImages` | Ad images (new) | Read/Write |
| `SavedAds` | Saved ads | Read/Write |
| `ChatConversations` | Chat conversations | Read/Write |
| `ChatMessages` | Chat messages | Read/Write |
| `FileRepository` | File repository | Read/Write |
| `FileCategories` | File categories | Read/Write |
| `AffiliatePartners` | Affiliate partners (Amazon, AliExpress) | Read/Write |
| `AffiliateProducts` | Affiliate products (new) | Read/Write |
| `AffiliateClicks` | Affiliate click tracking | Read/Write |
| `Communities` | New communities | Read/Write |
| `Posts` | New posts | Read/Write |
| `ForumPosts` | Reddit-style forum posts (text/image/link) | Read/Write |
| `PostMedia` | Media attachments for posts/forum posts | Read/Write |
| `ForumPostVotes` | Upvote/downvote for forum posts | Read/Write |
| `SystemSettings` | System settings | Read/Write |
| `AdminLogs` | Admin activity logs | Read/Write |

#### New Forum System Tables (Reddit-Style)

| Table | Purpose | Key Fields |
|-------|---------|------------|
| `ForumPosts` | Reddit-style discussion posts | PostType (text/image/link), LinkUrl, LinkPreview, Score, UpvoteCount, DownvoteCount |
| `PostMedia` | Image/video attachments | MediaType, MediaUrl, ForumPostID, DisplayOrder |
| `ForumPostVotes` | User votes on forum posts | VoteType (upvote/downvote), ForumPostID, UserId |

#### Enhanced Affiliate Marketing Tables

| Table | Purpose | Enhancements |
|-------|---------|--------------|
| `AffiliatePartners` | Affiliate program partners | PartnerType (amazon/aliexpress), TrackingId, ApiKey, CommissionRate |
| `AffiliateProducts` | Affiliate products | ASIN, AliExpressProductId, Rating, ReviewCount, PrimeEligible, BestSeller, AmazonChoice |
| `AffiliateClicks` | Click tracking | IPAddress, UserAgent, ReferrerUrl, Converted, CommissionAmount |

---

## Repository Implementation Pattern

### Example: PostRepository

```csharp
public class PostRepository : IPostRepository
{
    private readonly NewApplicationDbContext _newContext; // For writes
    private readonly ApplicationDbContext _oldContext;    // For legacy reads
    
    // Read new posts from new database
    public async Task<Post?> GetByIdAsync(int id)
    {
        return await _newContext.Posts
            .FirstOrDefaultAsync(p => p.PostID == id);
    }
    
    // Read legacy posts from old database (if needed)
    public async Task<Post?> GetLegacyPostAsync(int id)
    {
        return await _oldContext.Posts
            .AsNoTracking() // Important: read-only
            .FirstOrDefaultAsync(p => p.PostID == id);
    }
    
    // Write new posts to new database
    public async Task<Post> CreateAsync(Post post)
    {
        _newContext.Posts.Add(post);
        await _newContext.SaveChangesAsync();
        return post;
    }
}
```

### Example: CommunityRepository

```csharp
public class CommunityRepository : ICommunityRepository
{
    private readonly NewApplicationDbContext _newContext; // For new communities
    private readonly ApplicationDbContext _oldContext;    // For legacy communities
    
    // Read from both databases and merge
    public async Task<IEnumerable<Community>> GetAllAsync()
    {
        var newCommunities = await _newContext.Communities
            .ToListAsync();
            
        var legacyCommunities = await _oldContext.Communities
            .AsNoTracking() // Read-only
            .ToListAsync();
            
        // Merge and return (mark legacy ones appropriately)
        return newCommunities.Concat(legacyCommunities);
    }
    
    // Write only to new database
    public async Task<Community> CreateAsync(Community community)
    {
        _newContext.Communities.Add(community);
        await _newContext.SaveChangesAsync();
        return community;
    }
}
```

---

## Service Layer Pattern

### Example: PostService

```csharp
public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ILegacyDataService _legacyDataService;
    
    public async Task<PostDto> GetPostAsync(int id)
    {
        // Try new database first
        var post = await _postRepository.GetByIdAsync(id);
        
        // If not found, try legacy database
        if (post == null)
        {
            post = await _legacyDataService.GetLegacyPostAsync(id);
            if (post != null)
            {
                // Mark as legacy for UI display
                post.IsLegacy = true;
            }
        }
        
        return _mapper.Map<PostDto>(post);
    }
    
    public async Task<PostDto> CreatePostAsync(CreatePostDto dto, string userId)
    {
        // ALWAYS create in new database
        var post = _mapper.Map<Post>(dto);
        post.UserId = userId;
        post.CreatedAt = DateTime.UtcNow;
        
        var createdPost = await _postRepository.CreateAsync(post);
        return _mapper.Map<PostDto>(createdPost);
    }
}
```

---

## Migration Strategy

### What Stays in Old Database
- All existing forum threads
- All existing marketplace ads
- All existing blog posts
- All existing product reviews
- Historical data

### What Migrates to New Database
- **Option 1**: Migrate nothing (keep old DB as archive)
- **Option 2**: Migrate active communities (if needed)
- **Option 3**: Migrate user accounts (if using new identity system)

### What Goes to New Database (New Content Only)
- All new posts (including Reddit-style forum posts)
- All new forum posts with photos and links
- All new post media attachments
- All new votes and reactions
- All new comments
- All new communities
- All new classified ads
- All new chat messages
- All new file uploads
- All new affiliate products and clicks

---

## Best Practices

### 1. Always Use AsNoTracking() for Old Database
```csharp
var legacyPosts = await _oldContext.Posts
    .AsNoTracking() // Prevents tracking, ensures read-only
    .ToListAsync();
```

### 2. Never Write to Old Database
```csharp
// ❌ WRONG
_oldContext.Posts.Add(newPost);
await _oldContext.SaveChangesAsync();

// ✅ CORRECT
_newContext.Posts.Add(newPost);
await _newContext.SaveChangesAsync();
```

### 3. Mark Legacy Data in UI
```csharp
// In your DTO or ViewModel
public bool IsLegacy { get; set; }

// In your view
@if (Model.IsLegacy)
{
    <span class="badge">Legacy Content</span>
}
```

### 4. Use Separate DbContexts
```csharp
// In Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(oldConnectionString));

builder.Services.AddDbContext<NewApplicationDbContext>(options =>
    options.UseSqlServer(newConnectionString));
```

### 5. Create Legacy Data Service
```csharp
public interface ILegacyDataService
{
    Task<Post?> GetLegacyPostAsync(int id);
    Task<IEnumerable<Post>> GetLegacyPostsAsync();
    // ... other read-only methods
}
```

---

## Common Patterns

### Pattern 1: Read from Both, Merge Results
```csharp
public async Task<IEnumerable<Post>> GetAllPostsAsync()
{
    var newPosts = await _newContext.Posts.ToListAsync();
    var legacyPosts = await _oldContext.Posts
        .AsNoTracking()
        .ToListAsync();
    
    return newPosts.Concat(legacyPosts)
        .OrderByDescending(p => p.CreatedAt);
}
```

### Pattern 2: Try New First, Fallback to Old
```csharp
public async Task<Post?> GetPostAsync(int id)
{
    var post = await _newContext.Posts
        .FirstOrDefaultAsync(p => p.PostID == id);
    
    if (post == null)
    {
        post = await _oldContext.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PostID == id);
    }
    
    return post;
}
```

### Pattern 3: Write Only to New
```csharp
public async Task<Post> CreatePostAsync(Post post)
{
    // Always use new database for writes
    _newContext.Posts.Add(post);
    await _newContext.SaveChangesAsync();
    return post;
}
```

### Pattern 4: Forum Post with Media Attachments
```csharp
public async Task<ForumPost> CreateForumPostAsync(ForumPost post, List<IFormFile> mediaFiles)
{
    // Create forum post in new database
    _newContext.ForumPosts.Add(post);
    
    // Handle media uploads for image posts
    if (post.PostType == "image" && mediaFiles != null)
    {
        foreach (var file in mediaFiles)
        {
            var mediaUrl = await _imageUploadService.UploadImageAsync(file, "forum-posts");
            post.Media.Add(new PostMedia
            {
                MediaType = "image",
                MediaUrl = mediaUrl,
                ForumPostID = post.ForumPostID,
                DisplayOrder = post.Media.Count
            });
        }
    }
    
    await _newContext.SaveChangesAsync();
    return post;
}
```

### Pattern 5: Affiliate Link Generation
```csharp
public async Task<string> GenerateAffiliateLinkAsync(string productUrl, string partnerType)
{
    var partner = await _newContext.AffiliatePartners
        .FirstOrDefaultAsync(p => p.PartnerType == partnerType && p.IsActive);
    
    if (partner == null)
        return productUrl;
    
    // Generate affiliate link based on partner type
    if (partnerType == "amazon")
    {
        return GenerateAmazonLink(productUrl, partner.TrackingId);
    }
    else if (partnerType == "aliexpress")
    {
        return GenerateAliExpressLink(productUrl, partner.TrackingId);
    }
    
    return productUrl;
}
```

---

## Troubleshooting

### Issue: Getting write errors on old database
**Solution**: Ensure you're using `NewApplicationDbContext` for all write operations.

### Issue: Legacy data not showing
**Solution**: Check that you're reading from `ApplicationDbContext` with `AsNoTracking()`.

### Issue: Duplicate data appearing
**Solution**: Ensure proper filtering when merging results from both databases.

### Issue: Performance issues
**Solution**: 
- Use `AsNoTracking()` for read-only queries
- Add proper indexes
- Consider caching legacy data
- Use pagination

---

## Checklist

When implementing new features:

- [ ] Identify which database to use (old = read-only, new = read/write)
- [ ] Use `AsNoTracking()` for old database queries
- [ ] All writes go to new database only
- [ ] Mark legacy data appropriately in UI
- [ ] Test read operations from both databases
- [ ] Test write operations (should only work on new database)
- [ ] Document any special cases

---

## New Features: Forum Posts & Affiliate Marketing

### Forum Posts System (Reddit-Style)

#### Database Tables (New Database Only)

**ForumPosts Table:**
- Supports multiple post types: `text`, `image`, `link`, `video`
- Link posts include: `LinkUrl`, `LinkTitle`, `LinkDescription`, `LinkThumbnail`
- Engagement metrics: `Score`, `UpvoteCount`, `DownvoteCount`, `ViewCount`
- Status flags: `IsPinned`, `IsLocked`, `IsDeleted`

**PostMedia Table:**
- Stores image/video attachments for forum posts
- Supports multiple media per post (via `DisplayOrder`)
- Includes metadata: `FileSize`, `Width`, `Height`, `AltText`

**ForumPostVotes Table:**
- Tracks user votes (upvote/downvote) on forum posts
- Unique constraint on (ForumPostID, UserId) prevents duplicate votes
- Used to calculate post score

#### Repository Pattern for Forum Posts

```csharp
public class ForumPostRepository : IForumPostRepository
{
    private readonly NewApplicationDbContext _context;
    
    public ForumPostRepository(NewApplicationDbContext context)
    {
        _context = context; // Always use new database
    }
    
    public async Task<ForumPost?> GetByIdAsync(long id)
    {
        return await _context.ForumPosts
            .Include(fp => fp.Media)
            .Include(fp => fp.Votes)
            .FirstOrDefaultAsync(fp => fp.ForumPostID == id);
    }
    
    public async Task<IEnumerable<ForumPost>> GetHotPostsAsync(int count = 20)
    {
        // Hot posts: Score calculation based on upvotes, time decay
        return await _context.ForumPosts
            .Where(fp => fp.PostStatus == "published" && !fp.IsDeleted)
            .OrderByDescending(fp => fp.Score)
            .ThenByDescending(fp => fp.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
    
    public async Task<ForumPost> CreateAsync(ForumPost post)
    {
        post.CreatedAt = DateTime.UtcNow;
        post.Score = 0; // Initial score
        
        _context.ForumPosts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }
    
    public async Task VoteAsync(long postId, long userId, string voteType)
    {
        // Remove existing vote if any
        var existingVote = await _context.ForumPostVotes
            .FirstOrDefaultAsync(v => v.ForumPostID == postId && v.UserId == userId);
        
        if (existingVote != null)
        {
            _context.ForumPostVotes.Remove(existingVote);
        }
        
        // Add new vote
        var vote = new ForumPostVote
        {
            ForumPostID = postId,
            UserId = userId,
            VoteType = voteType,
            CreatedAt = DateTime.UtcNow
        };
        _context.ForumPostVotes.Add(vote);
        
        // Update post vote counts
        var post = await _context.ForumPosts.FindAsync(postId);
        if (post != null)
        {
            if (voteType == "upvote")
            {
                post.UpvoteCount++;
                post.Score = post.UpvoteCount - post.DownvoteCount;
            }
            else if (voteType == "downvote")
            {
                post.DownvoteCount++;
                post.Score = post.UpvoteCount - post.DownvoteCount;
            }
        }
        
        await _context.SaveChangesAsync();
    }
}
```

### Affiliate Marketing System

#### Enhanced AffiliatePartner Table

**New Fields:**
- `PartnerType`: `amazon`, `aliexpress`, or `other`
- `TrackingId`: Amazon Associate Tag or AliExpress PID
- `ApiKey` / `ApiSecret`: For API integrations
- `CommissionRate`: Commission percentage

#### Enhanced AffiliateProductNew Table

**Amazon-Specific Fields:**
- `ASIN`: Amazon Standard Identification Number
- `Rating`: Product rating (1-5 stars)
- `ReviewCount`: Number of reviews
- `PrimeEligible`: Amazon Prime availability
- `BestSeller`: Best seller badge
- `AmazonChoice`: Amazon's Choice badge
- `Availability`: Stock status

**AliExpress-Specific Fields:**
- `AliExpressProductId`: AliExpress product identifier

**Pricing Fields:**
- `OriginalPrice`: Original price before discount
- `DiscountPrice`: Current discounted price
- `Currency`: Currency code (USD, EUR, etc.)

#### Affiliate Link Generation Pattern

```csharp
public class AffiliateLinkService : IAffiliateLinkService
{
    private readonly NewApplicationDbContext _context;
    
    public async Task<string> GenerateAffiliateLinkAsync(
        string productUrl, 
        string partnerType, 
        long? userId = null)
    {
        // Get active partner
        var partner = await _context.AffiliatePartners
            .FirstOrDefaultAsync(p => 
                p.PartnerType == partnerType && 
                p.IsActive);
        
        if (partner == null || string.IsNullOrEmpty(partner.TrackingId))
            return productUrl; // Return original if no partner configured
        
        // Generate affiliate link
        string affiliateLink;
        if (partnerType == "amazon")
        {
            affiliateLink = GenerateAmazonLink(productUrl, partner.TrackingId);
        }
        else if (partnerType == "aliexpress")
        {
            affiliateLink = GenerateAliExpressLink(productUrl, partner.TrackingId);
        }
        else
        {
            affiliateLink = productUrl;
        }
        
        // Track click if user is provided
        if (userId.HasValue)
        {
            await TrackClickAsync(productUrl, affiliateLink, userId.Value, partnerType);
        }
        
        return affiliateLink;
    }
    
    private string GenerateAmazonLink(string productUrl, string associateTag)
    {
        var uri = new Uri(productUrl);
        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        queryParams["tag"] = associateTag;
        
        return new UriBuilder(uri) { Query = queryParams.ToString() }.ToString();
    }
    
    private string GenerateAliExpressLink(string productUrl, string pid)
    {
        var uri = new Uri(productUrl);
        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        queryParams["p"] = pid;
        
        return new UriBuilder(uri) { Query = queryParams.ToString() }.ToString();
    }
    
    private async Task TrackClickAsync(
        string originalUrl, 
        string affiliateLink, 
        long userId, 
        string partnerType)
    {
        var click = new AffiliateClick
        {
            ProductID = null, // Can be linked to product if available
            UserId = userId,
            ClickDate = DateTime.UtcNow,
            // Additional tracking fields can be added
        };
        
        _context.AffiliateClicks.Add(click);
        await _context.SaveChangesAsync();
    }
}
```

### Link Preview Service (for Link Posts)

```csharp
public class LinkPreviewService : ILinkPreviewService
{
    public async Task<LinkPreview> GetLinkPreviewAsync(string url)
    {
        // Use Open Graph, oEmbed, or third-party API
        // Returns: Title, Description, ImageUrl, SiteName
        
        // Example using Open Graph
        var preview = new LinkPreview
        {
            Url = url,
            Title = await ExtractOpenGraphTag(url, "og:title"),
            Description = await ExtractOpenGraphTag(url, "og:description"),
            ImageUrl = await ExtractOpenGraphTag(url, "og:image"),
            SiteName = await ExtractOpenGraphTag(url, "og:site_name")
        };
        
        return preview;
    }
}
```

---

## Implementation Checklist for New Features

### Forum Posts System
- [ ] Create `ForumPosts` table in new database
- [ ] Create `PostMedia` table
- [ ] Create `ForumPostVotes` table
- [ ] Create `ForumPost` model (NewSchema)
- [ ] Create `PostMedia` model
- [ ] Create `ForumPostVote` model
- [ ] Update `NewApplicationDbContext` with new DbSets
- [ ] Create `IForumPostRepository` interface
- [ ] Implement `ForumPostRepository`
- [ ] Create `IForumPostService` interface
- [ ] Implement `ForumPostService`
- [ ] Update `ForumController` with new actions
- [ ] Create Reddit-style UI components

### Affiliate Marketing System
- [ ] Update `AffiliatePartner` table (add PartnerType, TrackingId, etc.)
- [ ] Update `AffiliateProductNew` table (add ASIN, Rating, etc.)
- [ ] Create `AffiliateLinkClicks` table
- [ ] Update `AffiliatePartner` model
- [ ] Update `AffiliateProductNew` model
- [ ] Create `AffiliateClick` model
- [ ] Update `NewApplicationDbContext`
- [ ] Create `IAffiliateLinkService` interface
- [ ] Implement `AffiliateLinkService`
- [ ] Create `ILinkPreviewService` interface
- [ ] Implement `LinkPreviewService`
- [ ] Create `IAffiliateProductService` interface
- [ ] Implement `AffiliateProductService`
- [ ] Create affiliate product card components
- [ ] Set up Amazon Associates account
- [ ] Set up AliExpress Affiliate account

---

**Last Updated**: 2025-01-26
**Version**: 2.0
**Changes**: Added Forum Posts system and enhanced Affiliate Marketing documentation

