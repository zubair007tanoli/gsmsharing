# Admin Setup Guide

## Issue Fixed
The `SiteRoles` table was missing from the database, which was causing the error when accessing admin pages.

## What Was Done
1. ✅ Created `SiteRole` domain model
2. ✅ Added entity configuration in `ApplicationDbContext`
3. ✅ Created and executed `CREATE_SITE_ROLES_TABLE.sql` migration
4. ✅ Table successfully created in database

## Assign Your First Site Admin

To access admin pages (like `/admin/manage/reports`), you need to assign the SiteAdmin role to your user account.

### Option 1: Using SQL Script (Recommended)

1. **Edit the script:**
   - Open `discussionspot9/ASSIGN_SITE_ADMIN.sql`
   - Replace `your-email@example.com` on line 7 with your actual email address

2. **Run the script:**
   ```powershell
   sqlcmd -S 167.88.42.56 -d DiscussionspotADO -U sa -P "1nsp1r0N@321" -i "D:\LAPTOP BACKUP\CodingProject2025\Repos\gsmsharing\discussionspot9\ASSIGN_SITE_ADMIN.sql"
   ```

### Option 2: Manual SQL Query

Run this query in your database, replacing `your-email@example.com`:

```sql
DECLARE @UserEmail NVARCHAR(256) = 'your-email@example.com';
DECLARE @UserId NVARCHAR(450);

SELECT @UserId = Id FROM AspNetUsers WHERE Email = @UserEmail;

-- Assign SiteAdmin role
INSERT INTO SiteRoles (UserId, RoleName, AssignedAt, AssignedByUserId, IsActive)
VALUES (@UserId, 'SiteAdmin', GETDATE(), @UserId, 1);

-- Add to AspNetRoles if not exists
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SiteAdmin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'SiteAdmin', 'SITEADMIN', NEWID());
END

-- Add to AspNetUserRoles
DECLARE @RoleId NVARCHAR(450);
SELECT @RoleId = Id FROM AspNetRoles WHERE Name = 'SiteAdmin';

INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (@UserId, @RoleId);
```

### Option 3: Find Your User ID First

If you don't know your email, list all users:

```sql
SELECT TOP 10 Id, UserName, Email, EmailConfirmed 
FROM AspNetUsers 
ORDER BY Id DESC;
```

## After Assignment

1. **Restart the application:**
   ```powershell
   # Stop the current dotnet watch
   # Then restart:
   dotnet watch
   ```

2. **Clear browser cookies/cache** for the application

3. **Re-login** to your account

4. **Access admin pages:**
   - Navigate to `http://localhost:5099/admin/manage/reports`
   - You should now see the Reports page

## Verify Admin Status

To check if a user has admin rights:

```sql
SELECT 
    u.Email,
    sr.RoleName,
    sr.IsActive,
    sr.AssignedAt
FROM SiteRoles sr
INNER JOIN AspNetUsers u ON sr.UserId = u.Id
WHERE sr.RoleName = 'SiteAdmin' AND sr.IsActive = 1;
```

## Available Roles

The system supports these roles:
- **SiteAdmin**: Full access to all admin features
- **Moderator**: Can moderate content
- **Verified**: Verified user badge
- **Partner**: Partner status

## Troubleshooting

### "User not found" Error
- Double-check the email address
- Ensure the user has registered/created an account
- Use the "Find Your User ID" query above

### Still Can't Access Admin Pages
1. Verify the role assignment with the SQL query above
2. Restart the application completely
3. Clear browser cache and re-login
4. Check browser console for any JavaScript errors

### Database Connection Issues
- Verify the connection string in `appsettings.json`
- Ensure the SQL Server is accessible
- Check firewall settings if remote database

## Security Notes

⚠️ **Important:**
- Only assign SiteAdmin to trusted users
- The first admin must assign themselves (bootstrap process)
- All role assignments are logged in `ModerationLogs` table
- Roles can be revoked using `AdminService.RemoveSiteRoleAsync()`

