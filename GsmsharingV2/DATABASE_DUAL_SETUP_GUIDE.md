# Dual Database Setup Guide

This document explains the dual database architecture for GSMSharing V2.

## Architecture Overview

The application uses **two separate databases**:

1. **Old Database** (`GsmsharingConnection`) - `gsmsharingv3`
   - Contains existing data (GsmBlog, userforum, AffiliationProgram)
   - Used for **READ-ONLY** operations to display existing content
   - Uses `ApplicationDbContext`

2. **New Database** (`GsmsharingConnectionNew`) - `gsmsharingv4`
   - Modern schema with new features
   - Used for **WRITE** operations (creating new content)
   - Uses `NewApplicationDbContext`

## Database Schemas

### Old Database (gsmsharingv3)
- **Tables with data**:
  - `GsmBlog` - Blog posts
  - `userforum` (in `gsmsharing` schema) - Forum threads
  - `AffiliationProgram` - Affiliate products
  - `MobilePosts` - Legacy blog posts
  - Identity tables (string-based IDs)

### New Database (gsmsharingv4)
- **Main Tables**:
  - `ClassifiedAds` - OLX-style marketplace ads
  - `AdCategories` - Ad categories
  - `AdImages` - Ad images
  - `FileRepository` - Firmware/file repository
  - `FileCategories` - File categories
  - `AffiliateProducts` - Modern affiliate products
  - `AffiliatePartners` - Affiliate partners (Amazon, Daraz, etc.)
  - `ChatConversations` / `ChatMessages` - Messaging system
  - `SystemSettings` - System configuration
  - `AdminLogs` - Admin activity logs
  - Identity tables (BIGINT-based IDs)

## Connection Strings

In `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "GsmsharingConnection": "Data Source=...;Database=gsmsharingv3;...",
    "GsmsharingConnectionNew": "Data Source=...;Database=gsmsharingv4;..."
  }
}
```

## Database Contexts

### ApplicationDbContext (Old Database)
- Located in: `GsmsharingV2/Database/ApplicationDbContext.cs`
- Used for reading existing data
- Injected in controllers that display old content

### NewApplicationDbContext (New Database)
- Located in: `GsmsharingV2/Database/NewApplicationDbContext.cs`
- Used for creating/managing new content
- Injected in AdminController and other write operations

## Usage Examples

### Reading Old Data (Landing Page)
```csharp
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context; // Old DB
    
    public async Task<IActionResult> Index()
    {
        // Read from old database
        var blogs = await _context.GsmBlogs.Where(b => b.Publish == true).ToListAsync();
        var forums = await _context.UsersFourm.Where(f => f.Publish == 1).ToListAsync();
        var products = await _context.AffiliationProducts.ToListAsync();
        
        return View(model);
    }
}
```

### Writing New Data (Admin Dashboard)
```csharp
public class AdminController : Controller
{
    private readonly NewApplicationDbContext _context; // New DB
    
    public async Task<IActionResult> CreateAd(ClassifiedAd ad)
    {
        // Write to new database
        _context.ClassifiedAds.Add(ad);
        await _context.SaveChangesAsync();
        return RedirectToAction("Ads");
    }
}
```

## Admin Dashboard

The Admin Dashboard provides CRUD operations for all new database features:

### Access
- URL: `/Admin`
- Requires: `Admin` role

### Features
1. **Dashboard** (`/Admin`)
   - Statistics overview
   - Quick actions

2. **Classified Ads** (`/Admin/Ads`)
   - List all ads
   - Create/Edit/Delete ads
   - Manage ad categories

3. **File Repository** (`/Admin/Files`)
   - Manage firmware files
   - File categories
   - External links (Google Drive, Mega, etc.)

4. **Affiliate Products** (`/Admin/AffiliateProducts`)
   - Manage affiliate products
   - Partner management
   - Click tracking

5. **Settings** (`/Admin/Settings`)
   - System configuration
   - Key-value settings

6. **Logs** (`/Admin/Logs`)
   - Admin activity logs

## Migration Strategy

### Phase 1: Dual Database (Current)
- Old database: Read-only for existing content
- New database: All new content creation

### Phase 2: Data Migration (Future)
- Migrate old data to new schema
- Update references
- Consolidate to single database

### Phase 3: Single Database (Future)
- Deprecate old database
- Use only new database

## Important Notes

1. **User IDs**: 
   - Old DB uses `string` IDs
   - New DB uses `BIGINT` IDs
   - Admin logs store string ID in details field for reference

2. **Identity**:
   - Currently authenticated against old database
   - New database has separate Identity system (not yet integrated)

3. **Navigation Properties**:
   - Models don't have navigation properties to IdentityUser in new schema
   - Use UserID foreign keys and load users separately if needed

## Setup Instructions

1. **Run New Database Schema**:
   ```sql
   -- Execute db_modernized.sql on the new database
   -- This creates gsmsharingv4 database with all new tables
   ```

2. **Update Connection Strings**:
   - Ensure `GsmsharingConnectionNew` points to `gsmsharingv4`
   - Old connection remains pointing to `gsmsharingv3`

3. **Access Admin Dashboard**:
   - Login as Admin user
   - Navigate to `/Admin`

## Models Location

- Old Models: `GsmsharingV2/Models/`
- New Models: `GsmsharingV2/Models/NewSchema/`

## Troubleshooting

### Issue: "Invalid column name" errors
- **Solution**: Ensure database schema matches models
- Check that migrations are applied to new database

### Issue: Cannot access Admin dashboard
- **Solution**: Ensure user has "Admin" role in old database
- Check authorization attributes in AdminController

### Issue: User ID mismatches
- **Solution**: Admin logs use string ID from old DB
- Future: Implement user mapping table

