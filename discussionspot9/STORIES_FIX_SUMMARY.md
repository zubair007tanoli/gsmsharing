# Stories Page - Bug Fixes & Improvements

## Date: October 25, 2025

## Issues Fixed

### 1. **Edit Button Not Working** ✅
**Problem:** The edit button on the stories index page was using `@story.StoryId` but the controller route expected a `storySlug`.

**Solution:**
- Updated the edit button href from `/stories/edit/@story.StoryId` to `/stories/@story.Slug/edit`
- This now correctly matches the controller route: `[Route("stories/{storySlug}/edit")]`

### 2. **Separate Visual Editor for Story Creation** ✅
**Problem:** Users needed a clear separation between creating stories visually vs. using AI-enhanced text-based creation.

**Solution:**
- Added a button group at the top of the stories page with two options:
  - **"Create Visually"** - Routes to `/stories/editor` for visual story creation
  - **"Create with AI"** - Routes to `/stories/create` for AI-enhanced text-based creation
- Updated empty state buttons to match this new pattern
- Visual Editor is now clearly separate for creating stories visually

### 3. **Missing Edit View** ✅
**Problem:** The Edit action in the controller existed but there was no Edit.cshtml view.

**Solution:**
- Created a comprehensive `Edit.cshtml` view for text-based story editing
- Features include:
  - Edit story title, description, and status
  - Add, delete, and reorder slides
  - Preview story functionality
  - Switch to Visual Editor option
  - Real-time slide count tracking
  - Responsive design with sticky sidebar

### 4. **Missing ViewModel Properties** ✅
**Problem:** `StoryEditViewModel` was missing `Slug` and `UpdatedAt` properties needed by the Edit view.

**Solution:**
- Added `Slug` and `UpdatedAt` properties to `StoryEditViewModel`
- Updated the controller's Edit action to populate these properties

## Files Modified

### Views
1. `discussionspot9/Views/Stories/Index.cshtml`
   - Fixed edit button URL (line 109)
   - Added button group for "Create Visually" and "Create with AI" (lines 25-32)
   - Updated empty state buttons (lines 180-185)

2. `discussionspot9/Views/Stories/Edit.cshtml` (NEW)
   - Comprehensive text-based editing interface
   - Slide management (add, delete, reorder)
   - Status management
   - Preview and navigation options

### Controllers
3. `discussionspot9/Controllers/StoriesController.cs`
   - Updated Edit action to populate Slug and UpdatedAt in the model (lines 149-153)

### Models
4. `discussionspot9/Models/ViewModels/CreativeViewModels/StoryViewModels.cs`
   - Added `Slug` property to `StoryEditViewModel` (line 39)
   - Added `UpdatedAt` property to `StoryEditViewModel` (line 42)

## How to Use

### Creating Stories
1. **Visual Creation:** Click "Create Visually" to use the drag-and-drop visual editor
2. **AI-Enhanced Creation:** Click "Create with AI" to use text-based creation with AI enhancement

### Editing Stories
1. **Visual Editing:** Click "Visual Editor" button on any story card
2. **Text-Based Editing:** Click "Edit" button on any story card
3. You can switch between visual and text-based editing at any time

## Testing Checklist
- [x] Edit button now works correctly
- [x] Visual editor accessible from "Create Visually" button
- [x] AI creation accessible from "Create with AI" button
- [x] Edit view displays correctly with existing stories
- [x] Slides can be added, deleted, and reordered
- [x] No linting errors in modified files

## Routes Available

- `GET /stories` - Stories index page
- `GET /stories/create` - AI-enhanced text-based story creation
- `GET /stories/editor` - Visual story editor (for new stories)
- `GET /stories/editor/{id}` - Visual story editor (for existing stories)
- `GET /stories/{storySlug}/edit` - Text-based story editing
- `POST /stories/{storySlug}/edit` - Save edited story
- `GET /stories/viewer/{slug}` - View published story
- `GET /stories/details/{slug}` - View story details
- `GET /stories/amp/{slug}` - AMP version of story
- `POST /stories/{storySlug}/delete` - Delete story

## Notes
- All changes are backward compatible
- Existing stories will work with the new edit functionality
- The visual editor and text-based edit views are now clearly separated
- Users have clear options for both creation methods

