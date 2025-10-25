-- =============================================
-- Script: Create SiteRoles Table
-- Description: Creates the SiteRoles table for site-wide role management
-- Author: System
-- Date: 2025-10-25
-- =============================================

-- Check if table already exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SiteRoles')
BEGIN
    PRINT 'Creating SiteRoles table...'
    
    CREATE TABLE SiteRoles (
        RoleId INT IDENTITY(1,1) NOT NULL,
        UserId NVARCHAR(450) NOT NULL,
        RoleName NVARCHAR(50) NOT NULL,
        AssignedAt DATETIME NOT NULL DEFAULT GETDATE(),
        AssignedByUserId NVARCHAR(450) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        Notes NVARCHAR(MAX) NULL,
        RemovedAt DATETIME NULL,
        RemovedByUserId NVARCHAR(450) NULL,
        
        CONSTRAINT PK_SiteRoles PRIMARY KEY (RoleId),
        
        CONSTRAINT FK_SiteRoles_User FOREIGN KEY (UserId) 
            REFERENCES AspNetUsers(Id) ON DELETE NO ACTION,
            
        CONSTRAINT FK_SiteRoles_AssignedBy FOREIGN KEY (AssignedByUserId) 
            REFERENCES AspNetUsers(Id) ON DELETE NO ACTION,
            
        CONSTRAINT FK_SiteRoles_RemovedBy FOREIGN KEY (RemovedByUserId) 
            REFERENCES AspNetUsers(Id) ON DELETE SET NULL,
            
        CONSTRAINT CK_SiteRole_RoleName CHECK (RoleName IN ('SiteAdmin', 'Moderator', 'Verified', 'Partner'))
    )
    
    PRINT 'SiteRoles table created successfully.'
END
ELSE
BEGIN
    PRINT 'SiteRoles table already exists. Skipping creation.'
END
GO

-- Create indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SiteRoles_UserId_RoleName_IsActive' AND object_id = OBJECT_ID('SiteRoles'))
BEGIN
    PRINT 'Creating composite index on UserId, RoleName, IsActive...'
    CREATE NONCLUSTERED INDEX IX_SiteRoles_UserId_RoleName_IsActive 
        ON SiteRoles (UserId, RoleName, IsActive)
    PRINT 'Composite index created.'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SiteRoles_RoleName' AND object_id = OBJECT_ID('SiteRoles'))
BEGIN
    PRINT 'Creating index on RoleName...'
    CREATE NONCLUSTERED INDEX IX_SiteRoles_RoleName 
        ON SiteRoles (RoleName)
    PRINT 'RoleName index created.'
END
GO

PRINT '============================================='
PRINT 'SiteRoles table setup complete!'
PRINT '============================================='
GO

