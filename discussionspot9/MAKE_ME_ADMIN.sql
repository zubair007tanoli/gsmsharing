-- ============================================
-- MAKE YOURSELF ADMIN - Quick Setup Script
-- ============================================

USE [DiscussionspotADO];
GO

-- ============================================
-- STEP 1: Find your user ID
-- ============================================
PRINT '================================================';
PRINT 'YOUR USER IDS:';
PRINT '================================================';
SELECT Id, Email, UserName FROM AspNetUsers;
GO

-- ============================================
-- STEP 2: Assign SiteAdmin Role
-- ============================================
-- IMPORTANT: Replace 'YOUR_USER_ID_HERE' with your actual user ID from above

DECLARE @UserId NVARCHAR(450) = 'YOUR_USER_ID_HERE'; -- ⬅️ CHANGE THIS!

-- Check if user exists
IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = @UserId)
BEGIN
    PRINT '';
    PRINT '================================================';
    PRINT 'ASSIGNING SITEADMIN ROLE...';
    PRINT '================================================';
    
    -- Add to SiteRoles table
    IF NOT EXISTS (SELECT 1 FROM SiteRoles WHERE UserId = @UserId AND RoleName = 'SiteAdmin' AND IsActive = 1)
    BEGIN
        INSERT INTO SiteRoles (UserId, RoleName, AssignedAt, IsActive)
        VALUES (@UserId, 'SiteAdmin', GETUTCDATE(), 1);
        PRINT '✅ Added to SiteRoles table';
    END
    ELSE
        PRINT '⚠️ Already has SiteAdmin in SiteRoles';
    
    -- Add to ASP.NET Identity
    IF NOT EXISTS (
        SELECT 1 FROM AspNetUserRoles ur
        INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
        WHERE ur.UserId = @UserId AND r.Name = 'SiteAdmin'
    )
    BEGIN
        DECLARE @RoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'SiteAdmin');
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        VALUES (@UserId, @RoleId);
        PRINT '✅ Added to AspNetUserRoles';
    END
    ELSE
        PRINT '⚠️ Already has SiteAdmin in AspNetUserRoles';
    
    PRINT '';
    PRINT '================================================';
    PRINT '✅ SUCCESS! YOU ARE NOW A SITEADMIN!';
    PRINT '================================================';
    PRINT 'Next steps:';
    PRINT '  1. Restart your application';
    PRINT '  2. Navigate to: http://localhost:5099/admin/manage/users';
    PRINT '  3. You can now manage all users!';
    PRINT '================================================';
END
ELSE
BEGIN
    PRINT '❌ ERROR: User ID not found!';
    PRINT 'Please replace YOUR_USER_ID_HERE with your actual user ID from the list above.';
END
GO

