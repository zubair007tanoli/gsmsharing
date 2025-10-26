# 🚀 Quick Migration Guide - Fix the SQL Errors

## Problem

You're getting these errors:
```
SqlException: Invalid column name 'BanExpiresAt'.
Invalid column name 'BanReason'.
Invalid column name 'IsBanned'.
```

**Cause**: The new columns haven't been added to the database yet.

**Solution**: Run the database migration.

---

## ✅ Quick Fix (2 Commands)

### **Step 1: Create Migration**

Open a **new** PowerShell terminal in the `discussionspot9` folder and run:

```powershell
dotnet ef migrations add AddAdminModerationSystem
```

Wait for it to complete (may take 30-60 seconds).

### **Step 2: Update Database**

```powershell
dotnet ef database update
```

This adds:
- 3 new columns to `UserProfiles` table: `IsBanned`, `BanExpiresAt`, `BanReason`
- 3 new tables: `UserBans`, `ModerationLogs`, `SiteRoles`

---

## 📋 After Migration

Your application will work without errors! Then:

### **Create First Admin User:**

```sql
-- 1. Get your user ID
SELECT Id, Email FROM AspNetUsers WHERE Email = 'your@email.com';

-- 2. Assign SiteAdmin role (replace YOUR_USER_ID_HERE)
DECLARE @UserId NVARCHAR(450) = 'YOUR_USER_ID_HERE';

INSERT INTO SiteRoles (UserId, RoleName, AssignedAt, IsActive)
VALUES (@UserId, 'SiteAdmin', GETUTCDATE(), 1);

-- 3. Add to ASP.NET Identity
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SiteAdmin')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'SiteAdmin', 'SITEADMIN', NEWID());

INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT @UserId, Id FROM AspNetRoles WHERE Name = 'SiteAdmin';
```

### **Then Access Admin Panel:**

```
http://localhost:5099/admin/manage/users
```

---

## 🎯 **What the Migration Does**

### **Adds to UserProfiles Table:**
- `IsBanned` (bit) - Is user currently banned?
- `BanExpiresAt` (datetime2, nullable) - When does ban expire?
- `BanReason` (nvarchar(max), nullable) - Why was user banned?

### **Creates New Tables:**

**UserBans:**
- Tracks all bans (site-wide and community-specific)
- Stores ban reason, duration, who banned
- Supports permanent and temporary bans

**ModerationLogs:**
- Complete audit trail of all admin actions
- Logs role changes, bans, unbans
- Stores moderator ID and timestamp

**SiteRoles:**
- Site-wide role assignments
- SiteAdmin, Moderator, Verified, VIP
- Tracks who assigned roles and when

---

## ⚠️ If Migration Fails

### Error: "Build failed"
**Solution**: Stop dotnet watch first (Ctrl+C), then run migration

### Error: "Unable to create migration"
**Solution**: Make sure you're in the `discussionspot9` folder:
```powershell
cd D:\LAPTOP BACKUP\CodingProject2025\Repos\gsmsharing\discussionspot9
dotnet ef migrations add AddAdminModerationSystem
```

### Error: "No DbContext was found"
**Solution**: Run with explicit project:
```powershell
dotnet ef migrations add AddAdminModerationSystem --project discussionspot9.csproj
```

---

## 🎊 **After Migration**

✅ No more SQL errors  
✅ Admin panel works  
✅ Community settings work  
✅ Role assignment works  
✅ Ban system works  

**Then you can use all the admin features we built!** 🚀

---

**Quick Summary**: Run `dotnet ef migrations add AddAdminModerationSystem` then `dotnet ef database update` in the discussionspot9 folder. That's it!

