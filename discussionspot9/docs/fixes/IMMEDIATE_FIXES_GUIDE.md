# Immediate Fixes Guide

## Quick Fix Instructions

### Issue 1: Visual Editor Not Loading Stories
**Location:** `discussionspot9/wwwroot/js/story-editor.js`
**Fix:** Add this after line 29 (after `this.init()`):

```javascript
// Load existing story data if available
const storyData = @Html.Raw(Json.Serialize(Model));
if (storyData && storyData.Slides && storyData.Slides.length > 0) {
    this.slides = storyData.Slides.map(slide => ({
        id: 'slide-' + slide.StorySlideId,
        backgroundColor: slide.BackgroundColor || '#667eea',
        backgroundType: 'color',
        duration: slide.Duration / 1000 || 5,
        transition: 'fade',
        elements: [] // Add element mapping here
    }));
    this.currentSlideIndex = 0;
    this.selectedSlide = this.slides[0];
    this.updateUI();
}
```

### Issue 2: Sticker Button Not Working
**Location:** `discussionspot9/wwwroot/js/story-editor-enhanced.js`
**Fix:** Ensure Bootstrap is loaded before this script runs:
- Move script tag to load after Bootstrap
- Or add: `window.addEventListener('load', initEnhancedFeatures);`

### Issue 3: Admin Link in Dropdown
**Location:** `discussionspot9/Views/Shared/_Header.cshtml`
**Fix:** Add after line 378 (before `<li><hr class="dropdown-divider"></li>`):

```csharp
@if (User.IsInRole("Admin"))
{
    <li><a class="dropdown-item" href="/admin"><i class="fas fa-cog me-2"></i>Admin</a></li>
    <li><hr class="dropdown-divider"></li>
}
```

### Issue 4: Mobile Navbar Fix
**Location:** `discussionspot9/Views/Shared/_Header.cshtml`
**Fix:** Add after line 213:

```css
@@media (max-width: 576px) {
    .navbar-right {
        flex-direction: column;
        width: 100%;
    }
    .nav-actions {
        display: flex;
        justify-content: space-around;
        width: 100%;
        margin-top: 1rem;
    }
    .action-icon {
        margin: 0;
    }
}
```

### Issue 5: Upload Dialog Fix
**Location:** `discussionspot9/wwwroot/js/story-editor-enhanced.js`
**Fix:** Ensure modal initialization happens after DOM ready and Bootstrap loaded.

## Application
These fixes address the most critical issues quickly.
Please apply them and test.

