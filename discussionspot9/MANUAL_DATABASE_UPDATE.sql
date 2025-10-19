-- ============================================
-- MANUAL DATABASE UPDATE SCRIPT
-- Admin & Moderation System
-- ============================================
-- Run this script on your database to fix the SQL errors

USE [DiscussionspotADO];
GO

-- ============================================
-- 1. Add missing columns to UserProfiles
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserProfiles') AND name = 'IsBanned')
BEGIN
    ALTER TABLE UserProfiles ADD IsBanned BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsBanned column to UserProfiles';
END
ELSE
    PRINT 'IsBanned column already exists';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserProfiles') AND name = 'BanExpiresAt')
BEGIN
    ALTER TABLE UserProfiles ADD BanExpiresAt DATETIME2 NULL;
    PRINT 'Added BanExpiresAt column to UserProfiles';
END
ELSE
    PRINT 'BanExpiresAt column already exists';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserProfiles') AND name = 'BanReason')
BEGIN
    ALTER TABLE UserProfiles ADD BanReason NVARCHAR(MAX) NULL;
    PRINT 'Added BanReason column to UserProfiles';
END
ELSE
    PRINT 'BanReason column already exists';

-- ============================================
-- 2. Create UserBans table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserBans')
BEGIN
    CREATE TABLE UserBans (
        BanId INT IDENTITY(1,1) PRIMARY KEY,
        UserId NVARCHAR(450) NOT NULL,
        CommunityId INT NULL,
        BannedByUserId NVARCHAR(450) NOT NULL,
        Reason NVARCHAR(MAX) NOT NULL,
        BannedAt DATETIME2 NOT NULL,
        ExpiresAt DATETIME2 NULL,
        IsPermanent BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        BanType NVARCHAR(50) NOT NULL DEFAULT 'site',
        ModeratorNotes NVARCHAR(MAX) NULL,
        LiftedAt DATETIME2 NULL,
        LiftedByUserId NVARCHAR(450) NULL,
        LiftReason NVARCHAR(MAX) NULL,
        
        CONSTRAINT FK_UserBans_BannedUser FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
        CONSTRAINT FK_UserBans_BannedByUser FOREIGN KEY (BannedByUserId) REFERENCES AspNetUsers(Id),
        CONSTRAINT FK_UserBans_LiftedByUser FOREIGN KEY (LiftedByUserId) REFERENCES AspNetUsers(Id),
        CONSTRAINT FK_UserBans_Community FOREIGN KEY (CommunityId) REFERENCES Communities(CommunityId)
    );
    
    CREATE INDEX IX_UserBans_UserId ON UserBans(UserId);
    CREATE INDEX IX_UserBans_CommunityId ON UserBans(CommunityId);
    CREATE INDEX IX_UserBans_IsActive ON UserBans(IsActive);
    
    PRINT 'Created UserBans table';
END
ELSE
    PRINT 'UserBans table already exists';

-- ============================================
-- 3. Create ModerationLogs table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ModerationLogs')
BEGIN
    CREATE TABLE ModerationLogs (
        LogId BIGINT IDENTITY(1,1) PRIMARY KEY,
        ModeratorId NVARCHAR(450) NOT NULL,
        TargetUserId NVARCHAR(450) NOT NULL,
        CommunityId INT NULL,
        ActionType NVARCHAR(100) NOT NULL,
        EntityType NVARCHAR(50) NOT NULL,
        EntityId NVARCHAR(100) NULL,
        Reason NVARCHAR(MAX) NOT NULL,
        PerformedAt DATETIME2 NOT NULL,
        OldValue NVARCHAR(500) NULL,
        NewValue NVARCHAR(500) NULL,
        ModeratorIp NVARCHAR(50) NULL,
        Metadata NVARCHAR(MAX) NULL,
        
        CONSTRAINT FK_ModerationLogs_Moderator FOREIGN KEY (ModeratorId) REFERENCES AspNetUsers(Id),
        CONSTRAINT FK_ModerationLogs_TargetUser FOREIGN KEY (TargetUserId) REFERENCES AspNetUsers(Id),
        CONSTRAINT FK_ModerationLogs_Community FOREIGN KEY (CommunityId) REFERENCES Communities(CommunityId)
    );
    
    CREATE INDEX IX_ModerationLogs_ModeratorId ON ModerationLogs(ModeratorId);
    CREATE INDEX IX_ModerationLogs_TargetUserId ON ModerationLogs(TargetUserId);
    CREATE INDEX IX_ModerationLogs_CommunityId ON ModerationLogs(CommunityId);
    CREATE INDEX IX_ModerationLogs_PerformedAt ON ModerationLogs(PerformedAt DESC);
    
    PRINT 'Created ModerationLogs table';
END
ELSE
    PRINT 'ModerationLogs table already exists';

-- ============================================
-- 4. Create SiteRoles table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SiteRoles')
BEGIN
    CREATE TABLE SiteRoles (
        SiteRoleId INT IDENTITY(1,1) PRIMARY KEY,
        UserId NVARCHAR(450) NOT NULL,
        RoleName NVARCHAR(100) NOT NULL,
        AssignedAt DATETIME2 NOT NULL,
        AssignedByUserId NVARCHAR(450) NULL,
        ExpiresAt DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        Permissions NVARCHAR(MAX) NULL,
        
        CONSTRAINT FK_SiteRoles_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
        CONSTRAINT FK_SiteRoles_AssignedByUser FOREIGN KEY (AssignedByUserId) REFERENCES AspNetUsers(Id)
    );
    
    CREATE INDEX IX_SiteRoles_UserId ON SiteRoles(UserId);
    CREATE INDEX IX_SiteRoles_RoleName ON SiteRoles(RoleName);
    CREATE INDEX IX_SiteRoles_IsActive ON SiteRoles(IsActive);
    
    PRINT 'Created SiteRoles table';
END
ELSE
    PRINT 'SiteRoles table already exists';

-- ============================================
-- 5. Create default roles in ASP.NET Identity
-- ============================================
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SiteAdmin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'SiteAdmin', 'SITEADMIN', NEWID());
    PRINT 'Created SiteAdmin role';
END;

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Moderator')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Moderator', 'MODERATOR', NEWID());
    PRINT 'Created Moderator role';
END;

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Verified')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Verified', 'VERIFIED', NEWID());
    PRINT 'Created Verified role';
END;

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'VIP')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'VIP', 'VIP', NEWID());
    PRINT 'Created VIP role';
END;

-- ============================================
-- 6. OPTIONAL: Make yourself admin
-- ============================================
-- First, find your user ID:
-- SELECT Id, Email FROM AspNetUsers WHERE Email = 'your@email.com';

-- Then uncomment and run this, replacing YOUR_USER_ID_HERE:
/*
DECLARE @UserId NVARCHAR(450) = 'YOUR_USER_ID_HERE';

-- Add to SiteRoles
IF NOT EXISTS (SELECT 1 FROM SiteRoles WHERE UserId = @UserId AND RoleName = 'SiteAdmin')
BEGIN
    INSERT INTO SiteRoles (UserId, RoleName, AssignedAt, IsActive)
    VALUES (@UserId, 'SiteAdmin', GETUTCDATE(), 1);
    PRINT 'Assigned SiteAdmin role in SiteRoles table';
END;

-- Add to ASP.NET Identity
IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = (SELECT Id FROM AspNetRoles WHERE Name = 'SiteAdmin'))
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    SELECT @UserId, Id FROM AspNetRoles WHERE Name = 'SiteAdmin';
    PRINT 'Assigned SiteAdmin role in AspNetUserRoles';
END;

PRINT 'User is now SiteAdmin!';
*/

GO

PRINT '';
PRINT '================================================';
PRINT 'DATABASE UPDATE COMPLETE!';
PRINT '================================================';
PRINT 'New tables created:';
PRINT '  - UserBans';
PRINT '  - ModerationLogs';
PRINT '  - SiteRoles';
PRINT '';
PRINT 'New columns added to UserProfiles:';
PRINT '  - IsBanned';
PRINT '  - BanExpiresAt';
PRINT '  - BanReason';
PRINT '';
PRINT 'Next steps:';
PRINT '  1. Run section 6 to make yourself admin';
PRINT '  2. Restart your application';
PRINT '  3. Navigate to /admin/manage/users';
PRINT '================================================';

