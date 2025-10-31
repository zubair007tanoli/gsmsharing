# Database Column Fix - BackgroundAudioUrl and PostId

## Problem
SQL errors were occurring because the `Story` model had properties (`BackgroundAudioUrl` and `PostId`) that don't exist in the database table.

## Solution
Marked these properties as `[NotMapped]` so Entity Framework Core will ignore them when querying the database.

## Changes Made

### 1. Story Model (`Models/Domain/Story.cs`)
- Added `[NotMapped]` attribute to `PostId` property
- Added `[NotMapped]` attribute to `BackgroundAudioUrl` property  
- Added `[NotMapped]` attribute to `Post` navigation property
- Added `using System.ComponentModel.DataAnnotations.Schema;` for `[NotMapped]` attribute

### 2. ApplicationDbContext (`Data/DbContext/ApplicationDbContext.cs`)
- Commented out `PostId` index creation
- Commented out `Post` relationship mapping
- Added `entity.Ignore()` calls for `PostId`, `BackgroundAudioUrl`, and `Post` properties

### 3. StoriesController (`Controllers/StoriesController.cs`)
- Removed `.Include(s => s.Post)` from Viewer action
- Added `.Include(s => s.Community)` instead

## Testing
After these changes:
1. The SQL errors for `BackgroundAudioUrl` and `PostId` should be resolved
2. Stories should load without database errors
3. AMP stories pages should work correctly

## Future Enhancement
If you want to add these columns to the database in the future, you can:
1. Create a migration to add `PostId` column (INT, nullable, FK to Posts table)
2. Create a migration to add `BackgroundAudioUrl` column (NVARCHAR(2048), nullable)
3. Remove the `[NotMapped]` attributes
4. Re-enable the EF Core mappings in ApplicationDbContext

