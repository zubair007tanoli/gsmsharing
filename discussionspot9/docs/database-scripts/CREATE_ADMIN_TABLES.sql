-- Create remaining admin tables
USE [DiscussionspotADO];
GO

-- Create UserBans table
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
    PRINT 'Created UserBans table';
END;

-- Create ModerationLogs table
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
    PRINT 'Created ModerationLogs table';
END;

-- Create SiteRoles table
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
    PRINT 'Created SiteRoles table';
END;

-- Create ASP.NET Identity roles
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SiteAdmin')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'SiteAdmin', 'SITEADMIN', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Moderator')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Moderator', 'MODERATOR', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Verified')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Verified', 'VERIFIED', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'VIP')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'VIP', 'VIP', NEWID());

PRINT 'Database update complete!';
GO

