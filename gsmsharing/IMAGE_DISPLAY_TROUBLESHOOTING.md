# Image Display Troubleshooting Guide

## 🐛 Problem
Images stored in database are not displaying on post detail pages.

## ✅ What Was Fixed

### 1. Added Missing Route (`Program.cs`)
```csharp
app.MapControllerRoute(
    name: "postBySlug",
    pattern: "r/{community}/posts/{slug}",
    defaults: new { controller = "Posts", action = "Details" });
```

### 2. Created Details Action (`PostsController.cs`)
```csharp
[HttpGet]
public async Task<IActionResult> Details(string community, string slug)
{
    var post = await _postRepository.GetBySlugAndCommunityAsync(slug, community);
    if (post == null) return NotFound();
    return View("GetPost", post);
}
```

### 3. Implemented Repository Method (`PostRepository.cs`)
- Added `GetBySlugAndCommunityAsync()` method
- Retrieves post with all SEO data, reactions, and community info

### 4. Fixed Image Display in View (`GetPost.cshtml`)
```cshtml
@if (!string.IsNullOrEmpty(Model.FeaturedImage))
{
    <img src="@Model.FeaturedImage" alt="@Model.Title" 
         class="post-image img-fluid mb-3" 
         onerror="this.style.display='none';">
}
```

### 5. Made Posts Clickable (`Index.cshtml`)
```cshtml
<a href="/r/@item.CommunitySlug/posts/@item.Slug">
    @item.Title
</a>
```

---

## 🔍 Diagnosing Image Issues

### Step 1: Check Database Value
Run this SQL query to see what's stored:

```sql
SELECT PostID, Title, FeaturedImage, Slug 
FROM Posts 
WHERE Slug = 'ios-261-update-everything-you-need-to-know-2025';
```

**Expected values:**
- ✅ `/uploads/posts/featured/20250127143022_abc123.jpg`
- ✅ `uploads/posts/featured/20250127143022_abc123.jpg`
- ❌ `20250127143022_abc123.jpg` (missing path)
- ❌ `NULL` (no image saved)
- ❌ `posts/featured/image.jpg` (wrong format)

### Step 2: Check Physical File Exists
Navigate to:
```
E:\Repo\gsmsharing\wwwroot\uploads\posts\featured\
```

Verify the image file exists with the exact name from database.

### Step 3: Check File Storage Configuration
In `appsettings.json`:
```json
"FileStorage": {
  "RootDirectory": "wwwroot/uploads",
  "BaseUrl": "/uploads"
}
```

### Step 4: Verify Image URL in Browser
Open developer tools (F12) and check:
1. **Network tab** - Is the image request failing?
2. **Console** - Any 404 errors?
3. **Elements** - What's the actual `src` attribute?

---

## 🛠️ Common Issues & Solutions

### Issue 1: Image Path is Just Filename
**Symptom:** Database has `20250127143022_abc123.jpg`  
**Cause:** Old data or direct database insert  
**Solution:**
```sql
UPDATE Posts 
SET FeaturedImage = '/uploads/posts/featured/' + FeaturedImage
WHERE FeaturedImage NOT LIKE '/%' 
  AND FeaturedImage IS NOT NULL;
```

### Issue 2: Image Path Has Backslashes
**Symptom:** Database has `uploads\posts\featured\image.jpg`  
**Cause:** Windows path separators  
**Solution:**
```sql
UPDATE Posts 
SET FeaturedImage = REPLACE(FeaturedImage, '\', '/')
WHERE FeaturedImage LIKE '%\%';
```

### Issue 3: Image Path Missing Leading Slash
**Symptom:** Database has `uploads/posts/featured/image.jpg`  
**Cause:** Missing leading slash  
**Solution:**
```sql
UPDATE Posts 
SET FeaturedImage = '/' + FeaturedImage
WHERE FeaturedImage NOT LIKE '/%' 
  AND FeaturedImage LIKE 'uploads/%';
```

### Issue 4: Image File Doesn't Exist
**Symptom:** Path is correct but file missing  
**Cause:** File never uploaded or deleted  
**Solution:**
1. Re-upload the image through the Create Post form
2. Or manually copy the file to the correct directory

### Issue 5: Wrong File Permissions
**Symptom:** 403 Forbidden error  
**Cause:** IIS/Web server can't read the file  
**Solution:**
```powershell
# Give IIS_IUSRS read permission
icacls "E:\Repo\gsmsharing\wwwroot\uploads" /grant "IIS_IUSRS:(OI)(CI)R" /T
```

### Issue 6: Static Files Not Configured
**Symptom:** All images return 404  
**Cause:** Static file middleware not configured  
**Solution:** Already configured in `Program.cs`:
```csharp
app.MapStaticAssets();
app.UseStaticFiles(); // If using .NET 8 or earlier
```

---

## 🧪 Testing the Fix

### Test 1: Access Post by URL
```
https://localhost:7025/r/gsmsharing/posts/ios-261-update-everything-you-need-to-know-2025
```

**Expected:** Post displays with image

### Test 2: Check Image URL Directly
```
https://localhost:7025/uploads/posts/featured/[IMAGE_NAME].jpg
```

**Expected:** Image displays in browser

### Test 3: Upload New Post
1. Go to `/Posts/Create`
2. Upload an image
3. Submit post
4. Verify image shows on post detail page

---

## 📊 Database Schema Check

Ensure your `Posts` table has the correct structure:

```sql
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Posts' 
  AND COLUMN_NAME = 'FeaturedImage';
```

**Expected:**
- Column: `FeaturedImage`
- Type: `nvarchar` or `varchar`
- Length: At least `500` (to store full paths)

---

## 🔧 Quick Fixes

### Fix 1: Update Specific Post
```sql
UPDATE Posts 
SET FeaturedImage = '/uploads/posts/featured/YOUR_IMAGE.jpg'
WHERE PostID = 123;  -- Your post ID
```

### Fix 2: Check All Posts with Images
```sql
SELECT 
    PostID,
    Title,
    FeaturedImage,
    LEN(FeaturedImage) as PathLength,
    CASE 
        WHEN FeaturedImage LIKE '/%' THEN 'OK'
        ELSE 'NEEDS FIX'
    END as Status
FROM Posts 
WHERE FeaturedImage IS NOT NULL;
```

### Fix 3: Bulk Fix All Image Paths
```sql
-- Backup first!
SELECT * INTO Posts_Backup FROM Posts;

-- Fix paths
UPDATE Posts 
SET FeaturedImage = 
    CASE 
        WHEN FeaturedImage LIKE '/%' THEN FeaturedImage
        WHEN FeaturedImage LIKE 'uploads/%' THEN '/' + FeaturedImage
        WHEN FeaturedImage NOT LIKE '%/%' THEN '/uploads/posts/featured/' + FeaturedImage
        ELSE FeaturedImage
    END
WHERE FeaturedImage IS NOT NULL;
```

---

## 📝 Proper Image Upload Flow

### When Creating a Post:

1. **User uploads file** → `PostViewModel.FeaturedImage` (IFormFile)

2. **ImageRepository.SaveImageAsync()** saves file:
   ```
   Physical: E:\Repo\gsmsharing\wwwroot\uploads\posts\featured\20250127_abc123.jpg
   Returns: /uploads/posts/featured/20250127_abc123.jpg
   ```

3. **Controller saves to database**:
   ```csharp
   post.FeaturedImage = uploadResult.FileUrl;  // "/uploads/posts/featured/..."
   await _postRepository.CreateAsync(post);
   ```

4. **View displays image**:
   ```cshtml
   <img src="@Model.FeaturedImage" />
   ```
   Renders as:
   ```html
   <img src="/uploads/posts/featured/20250127_abc123.jpg" />
   ```

---

## 🚀 Deployment Checklist

Before deploying to production:

- [ ] Verify `wwwroot/uploads` directory exists
- [ ] Check folder permissions (IIS_IUSRS can write)
- [ ] Test image upload
- [ ] Test image display
- [ ] Check existing posts with images
- [ ] Run database path fix SQL if needed
- [ ] Clear browser cache
- [ ] Test on mobile device

---

## 📞 Still Not Working?

### Check Browser Console (F12)

**If you see 404 error:**
```
GET https://localhost:7025/uploads/posts/featured/image.jpg 404 (Not Found)
```

1. File doesn't exist at that path
2. Check physical file location
3. Verify filename matches database

**If you see 403 error:**
```
GET https://localhost:7025/uploads/posts/featured/image.jpg 403 (Forbidden)
```

1. Permission issue
2. Run `icacls` command above
3. Restart IIS/Kestrel

**If image tag is missing:**
```html
<!-- No img tag rendered -->
```

1. `Model.FeaturedImage` is null or empty
2. Check database value
3. Check repository query

---

## 🔍 Advanced Debugging

### Enable Detailed Logging

In `appsettings.Development.json`:
```json
"Logging": {
  "LogLevel": {
    "gsmsharing.Controllers.PostsController": "Debug",
    "gsmsharing.Repositories.PostRepository": "Debug",
    "Microsoft.AspNetCore.StaticFiles": "Information"
  }
}
```

### Add Debug Output to View

```cshtml
<!-- Debug Info (remove in production) -->
<div class="alert alert-info">
    <h5>Debug Info:</h5>
    <p><strong>FeaturedImage:</strong> @(Model.FeaturedImage ?? "NULL")</p>
    <p><strong>Full URL:</strong> @Url.Content(Model.FeaturedImage ?? "")</p>
    <p><strong>PostID:</strong> @Model.PostID</p>
    <p><strong>Slug:</strong> @Model.Slug</p>
</div>
```

---

## ✅ Success Criteria

Your image display is working correctly when:

1. ✅ Post URL loads: `/r/gsmsharing/posts/your-slug`
2. ✅ Image displays on page (not broken icon)
3. ✅ Image URL is correct: `/uploads/posts/featured/file.jpg`
4. ✅ Direct image URL works: `https://localhost:7025/uploads/posts/featured/file.jpg`
5. ✅ New uploads save and display correctly
6. ✅ No 404 errors in browser console
7. ✅ onerror handler doesn't trigger (image loads successfully)

---

**Fixed on:** October 27, 2025  
**Files Modified:**
- `Program.cs` - Added route
- `PostsController.cs` - Added Details action
- `PostRepository.cs` - Added GetBySlugAndCommunityAsync
- `IPostRepository.cs` - Added interface method
- `GetPost.cshtml` - Fixed image display
- `Index.cshtml` - Made posts clickable

