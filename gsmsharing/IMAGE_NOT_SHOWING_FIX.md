# Image Not Showing - Troubleshooting Guide

## 🐛 Issue
Images are not displaying on post detail pages (e.g., `https://localhost:7025/r/gsmsharing/posts/ios-261-update-everything-you-need-to-know-2025`)

## ✅ Fixes Applied

### 1. Added `UseStaticFiles()` Middleware
**Location:** `Program.cs` (Line 69)

**Problem:** Without this middleware, ASP.NET Core cannot serve static files (images, CSS, JS) from `wwwroot`.

**Fix:**
```csharp
// Serve static files from wwwroot
app.UseStaticFiles();
```

**What this does:**
- Enables serving of files from `wwwroot` directory
- Makes `/uploads/posts/featured/image.jpg` accessible as a URL

---

### 2. Enhanced Image Path Handling
**Location:** `Views/Posts/GetPost.cshtml` (Lines 120-147)

**Problem:** Image paths might be stored in different formats (relative, absolute, with/without leading slash).

**Fix:**
```cshtml
@if (!string.IsNullOrEmpty(Model.FeaturedImage))
{
    var imagePath = Model.FeaturedImage;
    // If it's a relative path, ensure it starts with /
    if (!imagePath.StartsWith("http") && !imagePath.StartsWith("/"))
    {
        imagePath = "/" + imagePath;
    }
    <img src="@imagePath" alt="@Model.Title" class="post-image img-fluid mb-3" 
         onerror="console.error('Image failed to load:', this.src); this.style.display='none';">
}
```

**What this does:**
- Ensures relative paths start with `/`
- Handles absolute URLs (http/https)
- Logs errors to browser console for debugging

---

### 3. Added Debug Information
**Location:** `Views/Posts/GetPost.cshtml` (Lines 120-126)
**Location:** `Controllers/PostsController.cs` (Line 101)

**Debug alerts in view (localhost only):**
```cshtml
@if (ViewContext.HttpContext.Request.Host.Host.Contains("localhost"))
{
    <div class="alert alert-info">
        <strong>Debug:</strong> FeaturedImage = "@Model.FeaturedImage"
    </div>
}
```

**Server-side logging:**
```csharp
_logger.LogInformation($"Post {post.PostID} - FeaturedImage from DB: '{post.FeaturedImage}'");
```

---

## 🔍 How to Debug

### Step 1: Check What's in the Database
Run this SQL query:
```sql
SELECT PostID, Title, Slug, FeaturedImage 
FROM Posts 
WHERE Slug = 'ios-261-update-everything-you-need-to-know-2025'
```

**Expected values:**
- `FeaturedImage` should contain something like:
  - `/uploads/posts/featured/image.jpg` ✅ (with leading slash)
  - `uploads/posts/featured/image.jpg` ⚠️ (without leading slash - will be fixed by view)
  - `https://example.com/image.jpg` ✅ (absolute URL)
  - `NULL` or empty ❌ (no image)

---

### Step 2: Check If Image File Exists
1. Look in your file system:
   ```
   E:\Repo\gsmsharing\wwwroot\uploads\posts\featured\
   ```

2. Verify the filename matches what's in the database

3. Check file permissions (should be readable)

---

### Step 3: View Debug Information
1. Navigate to: `https://localhost:7025/r/gsmsharing/posts/ios-261-update-everything-you-need-to-know-2025`

2. You should see a blue alert box showing:
   ```
   Debug: FeaturedImage = /uploads/posts/featured/your-image.jpg
   ```

3. If no alert appears, the FeaturedImage field is empty in the database

---

### Step 4: Check Browser Console
1. Press F12 to open browser developer tools
2. Go to Console tab
3. Look for error messages like:
   ```
   Image failed to load: /uploads/posts/featured/missing-image.jpg
   GET https://localhost:7025/uploads/posts/featured/missing-image.jpg 404 (Not Found)
   ```

4. If you see a 404 error, the file doesn't exist at that path

---

### Step 5: Check Server Logs
Look for this log message:
```
Post [ID] - FeaturedImage from DB: '/uploads/posts/featured/image.jpg'
```

This shows exactly what was retrieved from the database.

---

## 🛠️ Common Issues & Solutions

### Issue 1: Image Path is Empty/NULL
**Symptom:** No debug alert, no image shown  
**Check:**
```sql
SELECT FeaturedImage FROM Posts WHERE PostID = [your_post_id]
```
**Solution:** The image was never uploaded. Re-create the post with an image.

---

### Issue 2: Image Path is Relative Without Leading Slash
**Symptom:** Debug shows `uploads/posts/featured/image.jpg` (no leading `/`)  
**Status:** ✅ **Auto-fixed** by the view code  
**The view adds the leading `/` automatically**

---

### Issue 3: File Doesn't Exist
**Symptom:** Browser console shows 404 error  
**Check:** Verify file exists at:
```
E:\Repo\gsmsharing\wwwroot\uploads\posts\featured\[filename]
```
**Solution:** 
- Re-upload the image, OR
- Copy the image file to the correct location, OR
- Update the database with the correct path

---

### Issue 4: Wrong File Path in Database
**Symptom:** Debug shows path, but 404 error in console  
**Example:** Database has `/images/post.jpg` but file is at `/uploads/posts/featured/post.jpg`  
**Solution:** Update the database:
```sql
UPDATE Posts 
SET FeaturedImage = '/uploads/posts/featured/correct-filename.jpg'
WHERE PostID = [your_post_id]
```

---

### Issue 5: UseStaticFiles Not Working
**Symptom:** All static files (CSS, JS, images) not loading  
**Check:** `Program.cs` should have:
```csharp
app.UseStaticFiles();
```
**Solution:** Already fixed! Restart the application.

---

## 📋 Image Upload Flow

### When Creating a Post:
1. User uploads image via `Create.cshtml`
2. Controller receives `IFormFile` in `PostViewModel.FeaturedImage`
3. `ImageRepository.SaveImageAsync()` saves file to:
   ```
   wwwroot/uploads/posts/featured/[unique-filename].jpg
   ```
4. Returns `FileUploadResult` with:
   - `FilePath`: `uploads/posts/featured/image.jpg`
   - `FileUrl`: `/uploads/posts/featured/image.jpg`
5. Controller sets:
   ```csharp
   postViewModel.Post.FeaturedImagePath = uploadResult.FilePath;
   postViewModel.Post.FeaturedImageUrl = uploadResult.FileUrl;
   ```
6. `ViewModelExtensions.ToModel()` maps:
   ```csharp
   post.FeaturedImage = viewModel.FeaturedImageUrl; // "/uploads/posts/featured/image.jpg"
   ```
7. Repository saves to database:
   ```sql
   INSERT INTO Posts (FeaturedImage, ...) VALUES (@FeaturedImage, ...)
   ```

### When Displaying a Post:
1. Repository retrieves post from database
2. `FeaturedImage` property contains: `/uploads/posts/featured/image.jpg`
3. View renders:
   ```html
   <img src="/uploads/posts/featured/image.jpg" />
   ```
4. Browser requests: `https://localhost:7025/uploads/posts/featured/image.jpg`
5. `UseStaticFiles()` middleware serves file from `wwwroot`

---

## 🧪 Quick Test

### Test 1: Create a Test Image
1. Create a test image file
2. Copy it to: `E:\Repo\gsmsharing\wwwroot\uploads\posts\featured\test.jpg`
3. Update a post in database:
   ```sql
   UPDATE Posts 
   SET FeaturedImage = '/uploads/posts/featured/test.jpg'
   WHERE PostID = 1
   ```
4. Visit the post page
5. **Expected:** Image shows correctly

### Test 2: Check Static Files
1. Navigate to: `https://localhost:7025/uploads/posts/featured/test.jpg`
2. **Expected:** Image displays directly
3. **If 404:** `UseStaticFiles()` not working - check `Program.cs`

---

## 📝 Configuration

### Current Settings (appsettings.json):
```json
"FileStorage": {
    "RootDirectory": "wwwroot/uploads",
    "MaxFileSizeInBytes": 5242880,  // 5MB
    "AllowedExtensions": [ ".jpg", ".jpeg", ".png", ".gif" ],
    "AllowedMimeTypes": [ "image/jpeg", "image/png", "image/gif" ],
    "PreserveFileName": false,
    "BaseUrl": "/uploads"
}
```

**What this means:**
- Files saved to: `wwwroot/uploads/[directory]/[filename]`
- URLs will be: `/uploads/[directory]/[filename]`
- Max size: 5MB
- Allowed types: JPG, PNG, GIF

---

## ✅ Checklist

After applying fixes:
- [ ] Restart the application
- [ ] Navigate to the problem post URL
- [ ] Check if debug alert shows
- [ ] Check what value is shown in debug alert
- [ ] Open browser console (F12)
- [ ] Look for any errors
- [ ] Check server logs for the debug message
- [ ] Verify file exists in file system
- [ ] Test accessing image directly in browser

---

## 🔄 If Issue Persists

### Run This SQL Query:
```sql
-- Check all posts with images
SELECT 
    PostID, 
    Title, 
    Slug,
    FeaturedImage,
    CASE 
        WHEN FeaturedImage IS NULL THEN 'NULL - No image'
        WHEN FeaturedImage = '' THEN 'EMPTY - No image'
        WHEN FeaturedImage LIKE '/%' THEN 'OK - Starts with /'
        WHEN FeaturedImage LIKE 'http%' THEN 'OK - Absolute URL'
        ELSE 'WARNING - Relative path without /'
    END AS ImageStatus
FROM Posts
WHERE Slug = 'ios-261-update-everything-you-need-to-know-2025'
```

This will tell you exactly what's in your database.

---

## 📧 Support Information

**Files Modified:**
1. `Program.cs` - Added `UseStaticFiles()`
2. `Views/Posts/GetPost.cshtml` - Added debug info and path handling
3. `Controllers/PostsController.cs` - Added logging

**No Database Changes Required** ✅  
**No Breaking Changes** ✅  
**Safe to Deploy** ✅

---

**Last Updated:** October 27, 2025  
**Status:** Fixed - Requires testing

