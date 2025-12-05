# 🔧 Link Preview Troubleshooting Guide

## Issue: URLs showing as plain text, no previews appearing

### Quick Diagnosis Steps

#### Step 1: Open Browser Console
1. Press `F12` to open Developer Tools
2. Go to the **Console** tab
3. Look for messages starting with `🔗`

#### Step 2: Check What You See

**If you see:**
```
🔗 Comment Link Preview: Initializing...
🔗 Document ready state: complete
🔗 Document already loaded, processing immediately...
🔗 Comment Link Preview: Processing all comments...
🔗 Found X comment(s) to process
```
✅ **Script is loading correctly**

**If you see:**
```
🔗 Found 0 comment(s) to process
🔗 No comments found! Checking selectors...
```
❌ **Problem: Comments not being detected**

**If you see nothing:**
❌ **Problem: Script not loading**

---

## Solution 1: Script Not Loading

### Check 1: Verify File Exists
Open in browser: `http://localhost:5099/js/CustomJs/comment-link-preview.js`

**Expected:** JavaScript file content
**If 404:** File is missing or path is wrong

### Check 2: Verify Script Tag in HTML
Open `Views/Post/DetailTestPage.cshtml` and look for:
```html
<script src="~/js/CustomJs/comment-link-preview.js"></script>
```

**Should be in the `@section Scripts` block**

### Check 3: Clear Browser Cache
1. Press `Ctrl + Shift + Delete`
2. Clear cached images and files
3. Refresh page with `Ctrl + F5`

---

## Solution 2: Comments Not Being Detected

### Check 1: Verify Comment HTML Structure
In browser console, run:
```javascript
document.querySelectorAll('.comment-item').length
```

**Expected:** Number > 0
**If 0:** Comment structure is different

### Check 2: Find Actual Comment Selectors
In browser console, run:
```javascript
// Check various selectors
console.log('comment-item:', document.querySelectorAll('.comment-item').length);
console.log('comment-text:', document.querySelectorAll('.comment-text').length);
console.log('comment-list:', document.querySelectorAll('.comment-list').length);
```

### Check 3: Manually Process Comments
In browser console, run:
```javascript
window.CommentLinkPreview.processAll()
```

**Expected:** URLs should be converted
**If error:** Check error message

---

## Solution 3: API Not Working

### Check 1: Test API Endpoint
In browser console, run:
```javascript
fetch('/api/LinkMetadata/GetMetadata', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ url: 'https://github.com' })
})
.then(r => r.json())
.then(data => console.log('✅ API Works:', data))
.catch(err => console.error('❌ API Failed:', err));
```

**Expected:** JSON response with metadata
**If 404:** API controller not registered
**If 500:** Server error (check backend logs)

### Check 2: Verify API Controller
File should exist: `Controllers/Api/LinkMetadataController.cs`

### Check 3: Check Backend Logs
Look for errors in the console where you ran `dotnet run`

---

## Solution 4: CSS Not Loading

### Check 1: Verify CSS File
Open in browser: `http://localhost:5099/css/comment-link-preview.css`

**Expected:** CSS file content
**If 404:** File is missing

### Check 2: Verify CSS Link in HTML
Open `Views/Post/DetailTestPage.cshtml` and look for:
```html
<link href="~/css/comment-link-preview.css" rel="stylesheet" />
```

**Should be in the `@section HeadScripts` block**

---

## Solution 5: Use Test Page

### Step 1: Open Test Page
Navigate to: `http://localhost:5099/test-link-preview.html`

### Step 2: Run Tests
1. Click "Test API with GitHub URL"
2. Click "Process Comments"
3. Check console output on the page

### Step 3: Diagnose
- If API test fails → Backend issue
- If comments don't process → JavaScript issue
- If previews don't appear → CSS issue

---

## Common Issues & Fixes

### Issue: "CommentLinkPreview is not defined"
**Cause:** Script not loaded
**Fix:** 
1. Check script tag in DetailTestPage.cshtml
2. Clear browser cache
3. Verify file path

### Issue: "Failed to fetch metadata"
**Cause:** API endpoint not working
**Fix:**
1. Check if API controller is registered
2. Check backend logs for errors
3. Verify URL is accessible from server

### Issue: Icons showing but no preview cards
**Cause:** API working but cards not rendering
**Fix:**
1. Check browser console for errors
2. Verify CSS is loaded
3. Check if preview container is created

### Issue: Preview cards showing but ugly/broken
**Cause:** CSS not loaded or conflicting styles
**Fix:**
1. Verify CSS file is loaded
2. Check for CSS conflicts
3. Inspect element in DevTools

---

## Manual Testing Commands

### In Browser Console:

```javascript
// 1. Check if script loaded
console.log('Script loaded:', typeof window.CommentLinkPreview !== 'undefined');

// 2. Check comment elements
console.log('Comments found:', document.querySelectorAll('.comment-item').length);

// 3. Process all comments manually
window.CommentLinkPreview.processAll();

// 4. Test API
fetch('/api/LinkMetadata/GetMetadata', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ url: 'https://github.com' })
})
.then(r => r.json())
.then(console.log);

// 5. Check for processed comments
console.log('Processed:', document.querySelectorAll('[data-links-processed="true"]').length);

// 6. Check for link icons
console.log('Icons:', document.querySelectorAll('.comment-inline-link-icon').length);

// 7. Check for preview cards
console.log('Previews:', document.querySelectorAll('.comment-link-preview-card').length);
```

---

## Step-by-Step Debug Process

### 1. Verify Files Exist
- [ ] `wwwroot/js/CustomJs/comment-link-preview.js` exists
- [ ] `wwwroot/css/comment-link-preview.css` exists
- [ ] `Controllers/Api/LinkMetadataController.cs` exists

### 2. Verify HTML References
- [ ] Script tag in DetailTestPage.cshtml
- [ ] CSS link in DetailTestPage.cshtml
- [ ] Files load in browser (no 404)

### 3. Verify JavaScript Execution
- [ ] Console shows initialization messages
- [ ] `window.CommentLinkPreview` is defined
- [ ] Comments are detected

### 4. Verify API
- [ ] API endpoint responds
- [ ] Returns valid JSON
- [ ] No CORS errors

### 5. Verify Rendering
- [ ] URLs replaced with icons
- [ ] Preview cards created
- [ ] CSS styles applied

---

## Emergency Fix: Force Reload

If nothing works, try this in browser console:

```javascript
// 1. Remove all processed flags
document.querySelectorAll('[data-links-processed]').forEach(el => {
    el.removeAttribute('data-links-processed');
    el.removeAttribute('data-links-processing');
});

// 2. Remove all existing previews
document.querySelectorAll('.comment-link-previews').forEach(el => el.remove());

// 3. Remove all link icons
document.querySelectorAll('.comment-inline-link-icon').forEach(el => {
    el.replaceWith(el.getAttribute('title') || el.href);
});

// 4. Reprocess everything
window.CommentLinkPreview.processAll();
```

---

## Contact Information

If none of these solutions work:

1. **Check browser console** for specific error messages
2. **Check backend logs** for API errors
3. **Use test page** at `/test-link-preview.html`
4. **Share error messages** for further help

---

## Quick Reference

**Test Page:** `http://localhost:5099/test-link-preview.html`
**API Endpoint:** `POST /api/LinkMetadata/GetMetadata`
**Main Script:** `/js/CustomJs/comment-link-preview.js`
**CSS File:** `/css/comment-link-preview.css`

**Console Commands:**
- `window.CommentLinkPreview.processAll()` - Process all comments
- `window.CommentLinkPreview.processComment(element)` - Process one comment

---

**Last Updated:** December 5, 2024
**Status:** Troubleshooting Guide
