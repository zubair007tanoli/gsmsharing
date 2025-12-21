# Database Column Fix - Temporary Solution

## Issue
The application was trying to query database columns (`IsDeleted`, `MetaTitle`, `MetaDescription`) that don't exist in the database yet, causing errors on the landing page.

## Temporary Fix Applied
I've configured Entity Framework Core to **ignore** these new properties in `ApplicationDbContext.cs` until the database columns are added.

### Ignored Properties:

**Post Entity:**
- `IsDeleted`
- `CanonicalUrl`
- `FocusKeyword`
- `Excerpt`
- `Score`
- `CommentCount`
- `UpvoteCount`
- `DownvoteCount`
- `IsLocked`
- `IsPinned`
- `DeletedAt`
- `SchemaMarkup`

**Comment Entity:**
- `IsDeleted`
- `UpvoteCount`
- `DownvoteCount`
- `IsEdited`
- `EditedAt`
- `DeletedAt`

**Community Entity:**
- `IsDeleted`
- `MetaTitle`
- `MetaDescription`

## Next Steps

### Option 1: Run Database Migration Script (Recommended)
1. Run the `db_modernized_fixes.sql` script against your database to add the missing columns
2. After the columns are added, **remove the `.Ignore()` calls** from `ApplicationDbContext.cs` (around line 210-240)
3. Restart the application

### Option 2: Keep Ignoring (Temporary)
If you don't need these features yet, you can keep the ignores in place. The application will work, but the new features (voting, SEO fields, soft delete) won't be functional.

## Location of Changes
- **File**: `GsmsharingV2/Database/ApplicationDbContext.cs`
- **Lines**: ~210-240 (in `OnModelCreating` method)
- **Look for**: Comments with "Temporarily ignore new properties"

## Note
The `MetaTitle` and `MetaDescription` properties for the `Post` entity are currently **NOT ignored** because they should exist in the database. If you still get errors about these, uncomment the ignore lines for them as well.


