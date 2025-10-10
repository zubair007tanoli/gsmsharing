# Comment System Testing Checklist

## Quick Testing Guide

### 1. Authentication & Login Flow

#### Test 1: Comment When Not Logged In
- [ ] Navigate to any post detail page while logged out
- [ ] Try to type in the comment editor
- [ ] Click "Comment" button
- **Expected:** Login prompt modal should appear
- **Expected:** Modal should show "Login" and "Create Account" buttons

#### Test 2: Login via Modal with ReturnUrl
- [ ] From the login modal, click "Login" button
- [ ] Enter valid credentials and submit
- **Expected:** Should redirect back to the same post page
- **Expected:** Should be able to comment immediately without another login

#### Test 3: Register via Modal with ReturnUrl
- [ ] Log out, then try to comment again
- [ ] From the login modal, click "Create Account" button
- [ ] Fill in registration form and submit
- **Expected:** Should redirect back to the same post page after registration
- **Expected:** Should be able to comment immediately

---

### 2. Rich Text Editor - Main Comment Box

#### Test 4: Basic Formatting
- [ ] Log in to the site
- [ ] Navigate to a post detail page
- [ ] Type some text in the comment editor
- [ ] Select text and apply **bold** formatting
- [ ] Select text and apply *italic* formatting
- [ ] Select text and apply underline formatting
- **Expected:** Text should be formatted as selected
- **Expected:** Toolbar buttons should highlight when active

#### Test 5: Links in Comments
- [ ] In the comment editor, type some text
- [ ] Select text and click the link button in toolbar
- [ ] Enter a URL (e.g., https://github.com)
- [ ] Submit the comment
- **Expected:** Link should be clickable in the posted comment
- **Expected:** Link preview should appear below the comment (after a moment)

#### Test 6: Lists and Blockquotes
- [ ] Create a new comment
- [ ] Use the ordered list button to create numbered items
- [ ] Use the unordered list button to create bullet points
- [ ] Use blockquote button on some text
- [ ] Submit comment
- **Expected:** Lists should render properly
- **Expected:** Blockquote should have distinct styling

#### Test 7: Code Blocks
- [ ] Create a new comment
- [ ] Type some code snippet
- [ ] Select the code and click "code block" button
- [ ] Submit comment
- **Expected:** Code should appear in monospace font with background
- **Expected:** Code formatting should be preserved

#### Test 8: Clear Button
- [ ] Type some content in the comment editor
- [ ] Apply various formatting
- [ ] Click the "Clear" button
- **Expected:** All content should be removed
- **Expected:** Editor should return to empty state

#### Test 9: Empty Comment Validation
- [ ] Open comment editor but don't type anything
- [ ] Click "Comment" button
- **Expected:** Warning notification should appear
- **Expected:** Comment should NOT be submitted

#### Test 10: Headers
- [ ] Type some text in the editor
- [ ] Select text and apply Header 1 from dropdown
- [ ] Try Header 2 and Header 3
- **Expected:** Text should become larger based on header level
- **Expected:** Headers should render properly in submitted comment

---

### 3. Reply Editor

#### Test 11: Opening Reply Editor
- [ ] Find an existing comment on a post
- [ ] Click the "Reply" button
- **Expected:** Rich text editor should appear below the comment
- **Expected:** Editor should have simplified toolbar (bold, italic, link, lists)
- **Expected:** Editor should be focused and ready to type

#### Test 12: Formatting in Replies
- [ ] Open reply editor
- [ ] Type some text with **bold** and *italic* formatting
- [ ] Add a link
- [ ] Create a list
- [ ] Submit the reply
- **Expected:** Reply should post with all formatting preserved
- **Expected:** Reply should appear nested under parent comment

#### Test 13: Cancel Reply
- [ ] Open reply editor
- [ ] Type some content
- [ ] Click "Cancel" button
- **Expected:** Reply editor should close
- **Expected:** Content should be cleared
- **Expected:** Memory should be cleaned up (no console errors)

#### Test 14: Multiple Reply Forms
- [ ] Open reply editor on Comment A
- [ ] Without submitting, click Reply on Comment B
- **Expected:** First reply editor should close
- **Expected:** Second reply editor should open
- **Expected:** No memory leaks (check console)

#### Test 15: Reply Validation
- [ ] Open reply editor but don't type anything
- [ ] Click "Reply" button
- **Expected:** Warning notification should appear
- **Expected:** Reply should NOT be submitted

---

### 4. Link Previews

#### Test 16: Single Link Preview
- [ ] Create comment with this text: "Check out https://github.com"
- [ ] Submit the comment
- [ ] Wait 1-2 seconds
- **Expected:** Link preview card should appear below comment
- **Expected:** Preview should show GitHub logo, title, description
- **Expected:** Clicking preview should open link in new tab

#### Test 17: Multiple Link Previews
- [ ] Create comment with multiple URLs:
  ```
  Here are some links:
  https://github.com
  https://stackoverflow.com
  ```
- [ ] Submit the comment
- **Expected:** Should show preview for each link
- **Expected:** Previews should be stacked vertically

#### Test 18: Link Preview Caching
- [ ] Post comment with URL: https://github.com
- [ ] Wait for preview to load
- [ ] Post another comment with same URL: https://github.com
- **Expected:** Second preview should load instantly (from cache)
- **Expected:** Both previews should look identical

#### Test 19: Invalid URLs
- [ ] Post comment with invalid URL: "http://this-does-not-exist-12345.com"
- [ ] Wait 2-3 seconds
- **Expected:** Should show fallback preview or no preview
- **Expected:** Comment should still post successfully
- **Expected:** No errors in console

---

### 5. Real-time Updates (SignalR)

#### Test 20: Real-time Comment Posting
- [ ] Open same post in two browser windows/tabs
- [ ] Post a comment in Window 1
- **Expected:** Comment should appear in Window 2 immediately
- **Expected:** No page refresh needed

#### Test 21: Real-time Replies
- [ ] Open same post in two browser windows
- [ ] Post a reply in Window 1
- **Expected:** Reply should appear nested in Window 2 immediately

#### Test 22: Vote Synchronization
- [ ] Open same post in two browser windows
- [ ] Upvote a comment in Window 1
- **Expected:** Vote count should update in Window 2 immediately
- **Expected:** Vote button should highlight in both windows

---

### 6. Edge Cases & Error Handling

#### Test 23: Long Comments
- [ ] Create a very long comment (500+ words)
- [ ] Include formatting throughout
- [ ] Submit comment
- **Expected:** Comment should post successfully
- **Expected:** Formatting should be preserved
- **Expected:** No truncation or errors

#### Test 24: Special Characters
- [ ] Create comment with special characters: `& < > " ' / \ `
- [ ] Submit comment
- **Expected:** Characters should be properly escaped
- **Expected:** No XSS vulnerabilities
- **Expected:** Comment displays correctly

#### Test 25: Network Failure
- [ ] Open developer tools → Network tab
- [ ] Start typing a comment
- [ ] Enable "Offline" mode in network tab
- [ ] Try to submit comment
- **Expected:** Error notification should appear
- **Expected:** Comment should remain in editor (not lost)
- **Expected:** User can retry after reconnecting

#### Test 26: Session Timeout
- [ ] Log in and open a post
- [ ] Wait for session to expire (or clear cookies)
- [ ] Try to post a comment
- **Expected:** Login modal should appear
- **Expected:** After re-login, should return to same post
- **Expected:** Draft comment should be preserved (if possible)

#### Test 27: Concurrent Editing
- [ ] Open same post in two browsers (logged in as different users)
- [ ] Both users post comments at same time
- **Expected:** Both comments should appear
- **Expected:** No conflicts or lost comments
- **Expected:** Correct author attribution

---

### 7. Mobile & Responsive Testing

#### Test 28: Mobile Comment Box
- [ ] Open post on mobile device (or responsive mode)
- [ ] Open comment editor
- **Expected:** Editor should be responsive
- **Expected:** Toolbar should be accessible
- **Expected:** Virtual keyboard shouldn't obscure editor

#### Test 29: Mobile Reply Forms
- [ ] On mobile, tap Reply on a comment
- **Expected:** Reply editor should open smoothly
- **Expected:** Page should scroll to editor if needed
- **Expected:** Easy to type and format

#### Test 30: Mobile Link Previews
- [ ] Post comment with links on mobile
- **Expected:** Link previews should be responsive
- **Expected:** Touch-friendly click targets
- **Expected:** Images should load appropriately sized

---

### 8. Performance Testing

#### Test 31: Many Comments
- [ ] Navigate to post with 50+ comments
- [ ] Scroll through all comments
- [ ] Try to reply to various comments
- **Expected:** Page should remain responsive
- **Expected:** No lag when opening editors
- **Expected:** Smooth scrolling

#### Test 32: Large Comment with Many Links
- [ ] Create comment with 10+ URLs
- [ ] Submit comment
- **Expected:** Preview processing should complete in < 5 seconds
- **Expected:** Page should remain usable during processing
- **Expected:** Previews should load progressively

---

## Console Checks

After each test, check browser console for:
- ❌ No JavaScript errors
- ❌ No failed network requests
- ❌ No memory leaks
- ✅ Proper authentication logging messages
- ✅ SignalR connection success messages

---

## Acceptance Criteria Summary

### Must Pass
- ✅ Users can log in via modal and return to same page
- ✅ Users can register via modal and return to same page
- ✅ Authenticated users can post comments with formatting
- ✅ Rich text editor works in main comment box
- ✅ Rich text editor works in reply forms
- ✅ Link previews appear for URLs in comments
- ✅ Real-time updates work via SignalR
- ✅ No authentication errors when logged in
- ✅ ReturnUrl properly preserved through entire flow

### Should Pass
- ✅ Mobile responsive design works correctly
- ✅ Link preview caching works (7-day cache)
- ✅ Error handling is graceful
- ✅ No console errors during normal operation
- ✅ Performance is acceptable with many comments

---

## Browser Testing Matrix

| Feature | Chrome | Firefox | Safari | Edge | Mobile Safari | Mobile Chrome |
|---------|--------|---------|--------|------|---------------|---------------|
| Auth Flow | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| Rich Editor | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| Replies | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| Link Previews | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| SignalR | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |

---

## Known Issues (If Any)

Document any issues discovered during testing here:

1. 
2. 
3. 

---

## Test Sign-off

- [ ] All critical tests passed
- [ ] All browser compatibility checks passed
- [ ] No console errors during testing
- [ ] Performance is acceptable
- [ ] Mobile experience is good
- [ ] Documentation is complete

**Tested By:** _________________  
**Date:** _________________  
**Sign-off:** _________________

---

## Notes

- For production deployment, ensure link preview service rate limits are configured
- Monitor SignalR connection stability in production
- Consider adding rate limiting for comment submissions
- Review XSS sanitization rules for rich text content

