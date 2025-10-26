# Image Upload Issue Diagnostic Guide

## Issue Description
Images and URLs are not being saved/retrieved when creating a post on the server at:
https://discussionspot.com/r/gsmsharing/posts/new-browser-of-chatgpt-everything-you-need-to-know

## Investigation Steps

### 1. Check Database Records
Run the SQL script `DIAGNOSE_IMAGE_UPLOAD_ISSUE.sql` to verify:
- ✓ Post was created successfully
- ✓ Media records exist in the database
- ✓ URLs are stored correctly

### 2. Verify Server File System
Check if files are physically saved on the server:

```bash
# On the server, check if the uploads folder exists
ls -la /path/to/wwwroot/uploads/

# Check for recent image uploads
ls -la /path/to/wwwroot/uploads/posts/images/

# Check folder permissions
stat /path/to/wwwroot/uploads/
```

**Expected path:** `/wwwroot/uploads/posts/images/` or `/wwwroot/uploads/posts/videos/`

### 3. Common Issues & Solutions

#### Issue A: Files Not Uploaded to Server
**Symptoms:**
- Database has no Media records
- Controller logs show MediaFiles count as 0

**Causes:**
1. Form not sending files (missing `enctype="multipart/form-data"`)
2. File input has no `name` attribute
3. Server request size limits exceeded

**Solutions:**
1. Verify form has `enctype="multipart/form-data"` ✓ (Already present)
2. Check `MediaFiles` input field in view ✓ (Already correct)
3. Check `appsettings.json` or `web.config` for request size limits

#### Issue B: Files Saved But Wrong Path in Database
**Symptoms:**
- Database has Media records
- URLs in database are wrong (e.g., missing leading slash, wrong path)

**Causes:**
1. FileStorageService returns incorrect relative path
2. Path construction logic error

**Solutions:**
1. Check `FileStorageService.cs` line 50: `var relativePath = $"/uploads/{folder.TrimStart('/')}/{fileName}";`
2. Verify database URLs start with `/uploads/`

#### Issue C: Files Saved But Not Accessible
**Symptoms:**
- Database has correct Media records
- Files exist on disk
- Images return 404 when accessed via browser

**Causes:**
1. Folder permissions (IIS/Kestrel user can't read)
2. Static files not configured properly
3. `.htaccess` or URL rewrite rules blocking access

**Solutions:**
1. Check folder permissions: `chmod 755 /wwwroot/uploads` (Linux) or set IIS_IUSRS permissions (Windows)
2. Verify `Program.cs` has `app.UseStaticFiles();`
3. Check server logs for 404 errors

#### Issue D: Development Works, Production Fails
**Symptoms:**
- Works locally
- Fails on deployed server

**Causes:**
1. Different file paths between local and server
2. Server uses different user account with different permissions
3. Production uses different storage provider (e.g., Azure Blob instead of local)

**Solutions:**
1. Check `_environment.WebRootPath` value on server vs local
2. Verify IIS application pool identity has write permissions
3. Check appsettings.json differences between environments

### 4. Server Configuration Checklist

For Windows/IIS:
- [ ] IIS_IUSRS has modify permissions on `/wwwroot/uploads/`
- [ ] Application pool identity is correct
- [ ] Request filtering allows large file uploads (>30MB default)
- [ ] web.config has correct maxRequestLength

For Linux/Nginx:
- [ ] www-data (or nginx user) has write permissions on `/wwwroot/uploads/`
- [ ] nginx.conf has `client_max_body_size` set appropriately
- [ ] SELinux not blocking file writes (if applicable)

### 5. Logging to Enable

Add these logs to identify the issue:

```csharp
// In PostController.Create (POST)
_logger.LogInformation("MediaFiles received: {Count}", model.MediaFiles?.Count ?? 0);
if (model.MediaFiles != null)
{
    foreach (var file in model.MediaFiles)
    {
        _logger.LogInformation("File: {Name}, Size: {Size}, Type: {Type}", 
            file.FileName, file.Length, file.ContentType);
    }
}

// In FileStorageService.SaveFileAsync
_logger.LogInformation("Saving file to: {Path}", fullPath);
_logger.LogInformation("WebRootPath: {Path}", _environment.WebRootPath);
```

### 6. Quick Database Query

```sql
-- Check if post has media
SELECT p.Title, m.Url, m.MediaType, m.FileSize, m.UploadedAt
FROM Posts p
LEFT JOIN Media m ON p.PostId = m.PostId
WHERE p.Slug = 'new-browser-of-chatgpt-everything-you-need-to-know';
```

### 7. URL Access Test

After identifying the database URLs, test direct access:
```
https://discussionspot.com/uploads/posts/images/[filename].jpg
```

If this returns 404, the issue is with static file serving, not upload.

## Expected Behavior

1. User selects images in create post form
2. Form submits with `multipart/form-data`
3. Controller receives `IFormFileCollection` in `model.MediaFiles`
4. `ProcessMediaFilesAsync` is called
5. `FileStorageService.SaveFileAsync` saves each file to `/wwwroot/uploads/posts/images/`
6. Returns relative URL like `/uploads/posts/images/abc123.jpg`
7. Media record created in database with this URL
8. When viewing post, image src points to this URL
9. Static file middleware serves the image

## Next Steps

1. Run the SQL diagnostic script
2. Check server logs for any errors during post creation
3. Verify the physical file system on the server
4. Test file upload with logging enabled
5. Report findings for further troubleshooting

