# Share Button Testing & Troubleshooting Guide

## Quick Test Steps

### 1. Test Using the Test Page
1. Navigate to: `http://localhost:5099/test-share.html`
2. Click the "Share" button on Test Post 1
3. Verify:
   - Modal appears
   - URL display shows the correct post URL
   - Console logs appear in the log output section
   - Social media icons are clickable

### 2. Test on Actual Posts
1. Go to: `http://localhost:5099/r/gsmsharing`
2. Find a post with a share button
3. Click the share button
4. Verify the modal opens

### 3. Test Sharing to Social Media
1. In the share modal, click on **Facebook**
   - Should open a new popup window
   - Should show Facebook's share dialog
   - Should contain the post URL

2. Try **Twitter**
   - Should open Twitter's tweet composer
   - Should pre-fill with the post title and URL

3. Try **Copy Link**
   - Should show "Copied!" message
   - Paste somewhere to verify the URL

## Troubleshooting

### Issue: Modal opens but social media platforms don't receive the link

**Possible Causes:**

1. **Popup Blocker**
   - **Solution**: Allow popups for localhost:5099
   - Check browser's address bar for blocked popup icon
   - Add exception in browser settings

2. **URL Not Being Passed**
   - **Check**: Open browser console (F12)
   - Look for logs: "Opening share popup for:" and "showShareModal called with:"
   - Verify the `url` value is correct

3. **Data Attributes Not Set**
   - **Check**: Inspect the share button element
   - Right-click button > Inspect
   - Verify these attributes exist:
     ```html
     data-share-url="http://localhost:5099/r/gsmsharing/post/..."
     data-share-title="Post Title"
     data-content-type="post"
     data-content-id="123"
     ```

### Issue: Console shows "Share URL is missing"

**Solution:**
- The share button wrapper doesn't have the required data attributes
- Check if `_ShareButtonsUnified` partial is being called correctly
- Verify ViewData values are being passed:
  ```csharp
  @await Html.PartialAsync("_ShareButtonsUnified", new ViewDataDictionary(ViewData) {
      { "ShareTitle", Model.Title },
      { "ShareUrl", $"{Context.Request.Scheme}://{Context.Request.Host}{Model.PostUrl}" },
      { "ShareType", "post" },
      { "ContentId", Model.PostId.ToString() },
      { "ShareVariant", "inline" }
  })
  ```

### Issue: Console shows "Share wrapper not found"

**Solution:**
- The button isn't inside a `.share-inline` wrapper
- Check HTML structure:
  ```html
  <div class="share-inline" data-share-url="...">
      <button class="share-inline-trigger" onclick="openSharePopup(this)">
  ```

### Issue: Share modal doesn't open at all

**Possible Causes:**

1. **JavaScript Not Loaded**
   - **Check**: Open console and type: `typeof openSharePopup`
   - Should return: `"function"`
   - If `"undefined"`, the script didn't load

2. **Script Loading Order**
   - **Check**: View page source
   - Bootstrap should load before share-handler.js
   - Verify in `_Layout.cshtml`:
     ```html
     <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
     <script src="~/js/CustomJs/share-handler.js"></script>
     ```

3. **JavaScript Error**
   - **Check**: Browser console for errors
   - Common errors:
     - "bootstrap is not defined" - Bootstrap didn't load
     - "$ is not defined" - jQuery issue (not required for this)

## Browser Console Commands for Testing

Open console (F12) and try these:

```javascript
// Test if functions are available
console.log(typeof openSharePopup);  // Should be "function"
console.log(typeof showShareModal);  // Should be "function"

// Manually trigger share modal
showShareModal(
    'http://localhost:5099/r/gsmsharing/post/test',
    'Test Post Title',
    'post',
    '123'
);

// Check for share button elements
console.log('Inline share buttons:', document.querySelectorAll('.share-inline').length);
console.log('Share-btn buttons:', document.querySelectorAll('.share-btn').length);

// Inspect first share button
const shareWrapper = document.querySelector('.share-inline');
if (shareWrapper) {
    console.log('Share URL:', shareWrapper.dataset.shareUrl);
    console.log('Share Title:', shareWrapper.dataset.shareTitle);
}
```

## Expected Console Output (When Working)

When you click a share button, you should see:

```
Opening share popup for: {
    shareUrl: "http://localhost:5099/r/gsmsharing/post/my-post",
    shareTitle: "My Post Title",
    contentType: "post",
    contentId: "123"
}

showShareModal called with: {
    url: "http://localhost:5099/r/gsmsharing/post/my-post",
    title: "My Post Title",
    contentType: "post",
    contentId: "123"
}

Encoded values: {
    encodedUrl: "http%3A%2F%2Flocalhost%3A5099%2Fr%2Fgsmsharing%2Fpost%2Fmy-post",
    encodedTitle: "My%20Post%20Title"
}
```

## Testing Share URLs

After the modal opens, you can verify the share URLs are correct:

### Facebook
Expected URL pattern:
```
https://www.facebook.com/sharer/sharer.php?u=http%3A%2F%2Flocalhost%3A5099%2Fr%2Fgsmsharing%2Fpost%2Fmy-post
```

### Twitter
Expected URL pattern:
```
https://twitter.com/intent/tweet?url=http%3A%2F%2Flocalhost%3A5099%2Fr%2Fgsmsharing%2Fpost%2Fmy-post&text=My%20Post%20Title
```

### LinkedIn
Expected URL pattern:
```
https://www.linkedin.com/sharing/share-offsite/?url=http%3A%2F%2Flocalhost%3A5099%2Fr%2Fgsmsharing%2Fpost%2Fmy-post
```

## Common Issues with Solutions

### 1. "Share works on test page but not on actual posts"
- **Cause**: The actual post cards might be using a different share button implementation
- **Solution**: 
  - Find which partial is being used for post cards
  - Ensure it's using `_ShareButtonsUnified` with `ShareVariant: "inline"`
  - Or add the required data attributes to the share button

### 2. "URL shows 'undefined' in the modal"
- **Cause**: ShareUrl not being passed to the partial
- **Solution**: Check the ViewData dictionary when calling the partial
  ```csharp
  { "ShareUrl", $"{Context.Request.Scheme}://{Context.Request.Host}{Model.PostUrl}" }
  ```

### 3. "Social media opens but shows wrong URL"
- **Cause**: Model.PostUrl might not be a complete URL
- **Solution**: 
  - Check if PostUrl starts with "/"
  - Ensure scheme and host are prepended
  - Verify in browser console what URL is being encoded

### 4. "Popup window gets blocked"
- **Cause**: Browser popup blocker
- **Solutions**:
  - Allow popups for your domain
  - Click the popup blocker icon in address bar
  - Some browsers require user interaction to be very recent (within 1-2 seconds)

## Performance Testing

### Load Time
- Share modal should appear within 200ms of clicking
- No lag or delay

### Multiple Shares
- Click share, close modal, click share again
- Should work consistently
- Modal should be removed from DOM when closed

### Memory Leaks
- Open/close modal 10 times
- Check browser memory (Task Manager)
- Should not continuously increase

## Mobile Testing Checklist

- [ ] Share button is easily tappable (44x44px minimum)
- [ ] Modal is responsive and fits screen
- [ ] Native share API appears on mobile (if supported)
- [ ] Fallback modal works if native share is cancelled
- [ ] WhatsApp share works on mobile
- [ ] Links open in mobile browsers correctly

## Production Checklist

Before deploying to production:

- [ ] Remove console.log statements (or make them conditional)
- [ ] Test with actual domain (not localhost)
- [ ] Verify SSL certificate for https shares
- [ ] Test popup blockers on Chrome, Firefox, Safari
- [ ] Verify Open Graph meta tags are set for proper link previews
- [ ] Test share analytics/tracking
- [ ] Ensure error handling doesn't expose sensitive info
- [ ] Test across different post types (text, image, link, video)

## Quick Fixes

If share is completely broken, try this quick reset:

1. Clear browser cache (Ctrl+Shift+Delete)
2. Hard reload the page (Ctrl+Shift+R)
3. Check if share-handler.js loaded:
   ```javascript
   console.log(typeof showShareModal);
   ```
4. If still undefined, check:
   - Network tab for 404 errors
   - File path in _Layout.cshtml
   - File exists in wwwroot/js/CustomJs/

## Support & Further Help

If issues persist:

1. Capture full console output
2. Inspect share button HTML
3. Check Network tab for failed requests
4. Take screenshot of error
5. Note browser and version
6. Document exact steps to reproduce

---

**Last Updated**: 2025-10-25
**Applies To**: discussionspot9 share functionality

