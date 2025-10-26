# 🧪 Quick Test Guide - File Upload System

## ⚡ 5-Minute Testing Guide

---

## 🚀 **Step 1: Start the Application**

```bash
cd discussionspot9
dotnet run
```

Wait for: `Now listening on: https://localhost:XXXX`

---

## 🧪 **Step 2: Test Community Icon & Banner**

### **Test A: Create Community with Images**

1. Open browser: `https://localhost:XXXX/create-community`

2. **Fill the form:**
   - Name: `TestCommunity` ✍️
   - Title: `My Test Community` ✍️
   - Short Description: `Testing icon and banner uploads` ✍️
   - Category: Select any ✍️

3. **Upload Icon:**
   - Click "Upload Icon" area
   - Select any image (JPG, PNG, GIF)
   - ✅ Preview should appear

4. **Upload Banner:**
   - Click "Upload Banner" area
   - Select any image (landscape recommended)
   - ✅ Preview should appear

5. **Submit:**
   - Click "Create Community" button
   - ✅ Should redirect to community page

6. **Verify:**
   - ✅ Icon should display in community header
   - ✅ Banner should display as background
   - ✅ No broken images
   - ✅ No 404 errors in browser console

---

### **Test B: Verify Files on Disk**

**Check these folders:**
```
wwwroot/uploads/communities/icons/      → Should have your icon
wwwroot/uploads/communities/banners/    → Should have your banner
```

**Expected:**
- Files with GUID names like: `a1b2c3d4-e5f6-...abc123.jpg`
- File sizes reasonable (not 0 bytes)

---

## 🧪 **Step 3: Test Post Image Upload**

### **Test C: Create Post with Image**

1. Navigate to your test community: `/r/TestCommunity`

2. Click "Create Post"

3. **Fill the form:**
   - Title: `Test Post with Image` ✍️
   - Post Type: Select "Image Post" or "Text Post" ✍️
   - Content: Add some text ✍️

4. **Upload Image:**
   - Use media upload area
   - Select an image
   - ✅ Preview should show

5. **Submit:**
   - Click "Create Post"
   - ✅ Should redirect to post details

6. **Verify:**
   - ✅ Image should display in post
   - ✅ No broken image icon
   - ✅ No 404 errors

---

### **Test D: Verify Post Files**

**Check folder:**
```
wwwroot/uploads/posts/images/    → Should have your image
```

**Expected:**
- Image file with GUID name
- Proper image format

---

## ✅ **Success Criteria**

### **All Good If:**
- ✅ Community icon displays correctly
- ✅ Community banner displays correctly
- ✅ Post images display correctly
- ✅ Files exist in wwwroot/uploads folders
- ✅ No console errors
- ✅ No 404 errors
- ✅ Styling looks consistent
- ✅ Dark mode works

---

## 🐛 **Troubleshooting**

### **Problem: "Service not registered" Error**

**Error Message:**
```
Unable to resolve service for type 'discussionspot9.Interfaces.IFileStorageService'
```

**Fix:**
1. Check `Program.cs` line ~214
2. Should see: `builder.Services.AddScoped<IFileStorageService, FileStorageService>();`
3. If missing, add it
4. Rebuild: `dotnet build`
5. Run again: `dotnet run`

---

### **Problem: Files Upload but Don't Display**

**Symptoms:**
- File saved to disk (you can see it in folder)
- But shows 404 in browser

**Possible Causes:**
1. **URL format wrong**
   - Should be: `/uploads/...`
   - Not: `uploads/...` or `wwwroot/uploads/...`

2. **Static files not configured**
   - Check Program.cs has: `app.UseStaticFiles();` (line 255)

3. **File permissions**
   - Ensure IIS/Kestrel can read wwwroot/uploads folder

**Debug Steps:**
1. Check database - what's the URL stored?
2. Try accessing URL directly in browser
3. Check browser console for errors
4. Check application logs

---

### **Problem: "File type not allowed" Error**

**Cause:**
Trying to upload unsupported file type

**Supported Types:**
- Images: .jpg, .jpeg, .png, .gif, .webp, .svg
- Videos: .mp4, .webm, .mov, .avi
- Docs: .pdf, .doc, .docx, .txt

**To Add More:**
Edit `Services/FileStorageService.cs` - add to allowed arrays

---

### **Problem: CSS Not Loading**

**Symptoms:**
- Page loads but looks unstyled
- Community pages look broken

**Fix:**
1. Check if file exists: `wwwroot/css/community-pages.css`
2. Clear browser cache (Ctrl+F5)
3. Check browser network tab - is CSS loading?
4. Check for CSS errors in console

---

## 📊 **Expected Results**

### **File Sizes:**
- Community icons: ~5-50 KB (256x256)
- Community banners: ~50-200 KB (1920x384)
- Post images: varies (original size, up to 10MB)

### **Response Times:**
- Upload 1MB image: ~500ms
- Upload 5MB video: ~2-3 seconds
- Page load with images: ~1-2 seconds

### **Database:**
After creating community with images:
```sql
SELECT IconUrl, BannerUrl FROM Communities WHERE Name = 'TestCommunity'

-- Should return:
-- IconUrl: /uploads/communities/icons/abc123-...xyz.jpg
-- BannerUrl: /uploads/communities/banners/def456-...xyz.jpg
```

---

## 🎯 **Quick Smoke Test (2 Minutes)**

**Fastest way to verify everything works:**

1. Run app
2. Create community with icon & banner
3. Check if images display on community page
4. ✅ If yes → ALL GOOD!
5. ❌ If no → Check troubleshooting section

---

## 💡 **Tips**

### **For Best Results:**
- Use JPG or PNG for icons
- Use landscape images for banners (16:9 or wider)
- Keep files under 5MB for fast uploads
- Use descriptive names (will be sanitized anyway)

### **For Testing:**
- Use small test images first (100-200 KB)
- Test on different browsers (Chrome, Firefox, Edge)
- Test on mobile device
- Test with slow internet (throttle in browser)

---

## 📞 **Need Help?**

### **Check Logs:**

**Application logs:**
- Look for: `"File saved successfully"`
- Look for: `"Community icon saved"`
- Look for errors in red

**Browser console:**
- F12 → Console tab
- Look for 404 errors
- Look for network errors

### **Common Log Messages:**

**Success:**
```
File saved successfully: /uploads/communities/icons/abc123.jpg (45678 bytes)
Community icon saved: /uploads/communities/icons/abc123.jpg
Community created successfully: TestCommunity (Slug: testcommunity)
```

**Errors:**
```
Error saving file: File size exceeds maximum limit
Error uploading community images: Invalid file type
```

---

## ✅ **All Done!**

If all tests pass:
- ✅ File upload system is working!
- ✅ Communities can have custom branding!
- ✅ Posts can have images/videos!
- ✅ Styling is clean and maintainable!

**Your platform is now feature-complete for file uploads!** 🎉

---

*Quick Test Guide*  
*Last Updated: October 19, 2025*  
*Estimated Test Time: 5-10 minutes*
