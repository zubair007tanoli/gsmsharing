# Database Structure Explanation

This document explains the database table structure and how they map to the application models.

## 📚 Main Content Tables (With Existing Data)

### 1. **GsmBlog Table** (Blog Posts)
- **Database Table**: `[dbo].[GsmBlog]`
- **EF Core DbSet**: `GsmBlogs`
- **Model**: `GsmBlog.cs`
- **Purpose**: Main blog posts table containing published blog articles
- **Key Properties**:
  - `BlogId` (Primary Key)
  - `BlogTitle` - Blog post title
  - `BlogDiscription` - Blog description
  - `BlogContent` - Full blog content
  - `BlogViews`, `BlogLikes`, `BlogDisLikes` - Engagement metrics
  - `Publish` (bool) - Publishing status (true = published)
  - `PublishDate` - When the blog was published
  - `ThumbNailLink` - Blog thumbnail image

**Usage**: This is the **main blog table** with existing data that should be displayed on the landing page.

---

### 2. **userforum Table** (Forum Threads)
- **Database Table**: `[gsmsharing].[userforum]` (in gsmsharing schema)
- **EF Core DbSet**: `UsersFourm`
- **Model**: `ForumThread.cs`
- **Purpose**: Forum discussion threads
- **Key Properties**:
  - `UserFourmID` (Primary Key)
  - `Title` - Forum thread title
  - `Content` - Thread content
  - `Tags` - Tags for categorization
  - `Views`, `Likes`, `Dislikes` - Engagement metrics
  - `Publish` (byte) - Publishing status (1 = published, 0 = draft)
  - `CreationDate` - When the thread was created

**Usage**: This is the **main forum table** with existing data that should be displayed on the landing page.

**Note**: The table is in the `gsmsharing` schema, so the full table name is `gsmsharing.userforum`.

---

### 3. **AffiliationProgram Table** (Affiliate Products)
- **Database Table**: `[dbo].[AffiliationProgram]`
- **EF Core DbSet**: `AffiliationProducts`
- **Model**: `AffiliationProduct.cs`
- **Purpose**: Affiliate marketing products for sale
- **Key Properties**:
  - `ProductId` (Primary Key)
  - `Title` - Product title
  - `ProductDiscription` - Product description
  - `Content` - Full product content
  - `Price` - Product price
  - `ImageLink` - Product image URL
  - `BuyLink` - Affiliate purchase link
  - `Views`, `Likes`, `DisLikes` - Engagement metrics
  - `CreationDate` - When the product was added

**Usage**: This is the **main products table** with existing data that should be displayed on the landing page.

---

## 🔄 New Tables (Modern Features)

### Posts Table (New Modern Posts)
- **Database Table**: `[dbo].[Posts]`
- **EF Core DbSet**: `Posts`
- **Model**: `Post.cs`
- **Purpose**: New modern post system with communities, SEO, voting, etc.
- **Status**: This is a NEW table structure, may not have data yet

### Comments Table
- **Database Table**: `[dbo].[Comments]`
- **EF Core DbSet**: `Comments`
- **Model**: `Comment.cs`
- **Purpose**: Comments on posts

---

## 📝 Other Tables

### MobilePosts
- **Database Table**: `[dbo].[MobilePosts]`
- **Purpose**: Another blog-like table (legacy)
- **Note**: Different from `GsmBlog` - use `GsmBlog` for main blog content

### Communities
- **Database Table**: `[dbo].[Communities]`
- **Purpose**: Community/Subreddit-like groups for the new Posts system

### Categories
- **Database Table**: `[dbo].[Categories]`
- **Purpose**: Content categories for organization

---

## 🎯 Landing Page Data Sources

For the landing page to show random content, use these tables:

1. **Random Blog**: `GsmBlog` table (where `Publish == true`)
2. **Random Forum**: `userforum` table (where `Publish == 1`)
3. **Random Product**: `AffiliationProgram` table

### Current Implementation

The `EngagingSidebarViewComponent` now correctly uses:
- ✅ `GsmBlog` for random blogs and popular blogs
- ✅ `UsersFourm` (userforum table) for random forums and trending forums
- ✅ `AffiliationProducts` (AffiliationProgram table) for random products

---

## 🔧 Database Mappings in ApplicationDbContext

```csharp
// Old tables with data
builder.Entity<GsmBlog>().ToTable("GsmBlog").HasKey(gb => gb.BlogId);
builder.Entity<ForumThread>().ToTable("userforum", "gsmsharing").HasKey(f => f.UserFourmID);
builder.Entity<AffiliationProduct>().ToTable("AffiliationProgram").HasKey(ap => ap.ProductId);

// New modern tables
builder.Entity<Post>().ToTable("Posts").HasKey(p => p.PostID);
builder.Entity<Comment>().ToTable("Comments").HasKey(c => c.CommentID);
```

---

## 📊 Table Summary

| Table Name | Schema | Model | Purpose | Has Data? |
|-----------|--------|-------|---------|-----------|
| `GsmBlog` | `dbo` | `GsmBlog` | Blog posts | ✅ Yes |
| `userforum` | `gsmsharing` | `ForumThread` | Forum threads | ✅ Yes |
| `AffiliationProgram` | `dbo` | `AffiliationProduct` | Affiliate products | ✅ Yes |
| `Posts` | `dbo` | `Post` | Modern posts | ⚠️ New |
| `MobilePosts` | `dbo` | `MobilePost` | Legacy blog posts | ⚠️ Check |
| `Communities` | `dbo` | `Community` | Post communities | ⚠️ New |

---

## ✅ Recommendation

**Use these tables for landing page content:**
1. **Blogs**: `GsmBlog` (has existing data)
2. **Forums**: `userforum` in `gsmsharing` schema (has existing data)
3. **Products**: `AffiliationProgram` (has existing data)

The `EngagingSidebarViewComponent` has been updated to use these correct tables.


