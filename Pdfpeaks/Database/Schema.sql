-- PdfPeaks Database Schema for MSSQL Server
-- Database: Pdfpeaks
-- Target: MSSQL Server on Ubuntu

-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Pdfpeaks')
BEGIN
    CREATE DATABASE Pdfpeaks;
END
GO

USE Pdfpeaks;
GO

-- ============================================
-- ASP.NET Core Identity Tables
-- ============================================

-- Users table (extended)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers')
BEGIN
    CREATE TABLE AspNetUsers (
        Id NVARCHAR(450) NOT NULL PRIMARY KEY,
        FirstName NVARCHAR(100) NULL,
        LastName NVARCHAR(100) NULL,
        SubscriptionTier INT NOT NULL DEFAULT 0,
        StripeCustomerId NVARCHAR(100) NULL,
        SubscriptionEndDate DATETIME2 NULL,
        DownloadsRemaining INT NOT NULL DEFAULT 1,
        TotalFilesProcessed INT NOT NULL DEFAULT 0,
        TotalStorageUsed BIGINT NOT NULL DEFAULT 0,
        MaxStorageAllowed BIGINT NOT NULL DEFAULT 104857600,
        EmailConfirmed BIT NOT NULL DEFAULT 0,
        AvatarUrl NVARCHAR(500) NULL,
        Timezone NVARCHAR(50) NULL DEFAULT 'UTC',
        PreferredLanguage NVARCHAR(10) NULL DEFAULT 'en',
        LastLoginDate DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsActive BIT NOT NULL DEFAULT 1,
        UserName NVARCHAR(256) NULL,
        NormalizedUserName NVARCHAR(256) NULL,
        Email NVARCHAR(256) NULL,
        NormalizedEmail NVARCHAR(256) NULL,
        EmailConfirmed BIT NOT NULL DEFAULT 0,
        PasswordHash NVARCHAR(MAX) NULL,
        SecurityStamp NVARCHAR(MAX) NULL,
        ConcurrencyStamp NVARCHAR(MAX) NULL,
        PhoneNumber NVARCHAR(MAX) NULL,
        PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
        TwoFactorEnabled BIT NOT NULL DEFAULT 0,
        LockoutEnd DATETIME2 NULL,
        LockoutEnabled BIT NOT NULL DEFAULT 0,
        AccessFailedCount INT NOT NULL DEFAULT 0
    );
END
GO

-- Roles table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoles')
BEGIN
    CREATE TABLE AspNetRoles (
        Id NVARCHAR(450) NOT NULL PRIMARY KEY,
        Name NVARCHAR(256) NULL,
        NormalizedName NVARCHAR(256) NULL,
        ConcurrencyStamp NVARCHAR(MAX) NULL
    );
END
GO

-- User Claims
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserClaims')
BEGIN
    CREATE TABLE AspNetUserClaims (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId NVARCHAR(450) NOT NULL,
        ClaimType NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT FK_AspNetUserClaims_AspNetUsers FOREIGN KEY (UserId) 
            REFERENCES AspNetUsers(Id) ON DELETE CASCADE
    );
END
GO

-- User Logins
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserLogins')
BEGIN
    CREATE TABLE AspNetUserLogins (
        LoginProvider NVARCHAR(450) NOT NULL,
        ProviderKey NVARCHAR(450) NOT NULL,
        ProviderDisplayName NVARCHAR(MAX) NULL,
        UserId NVARCHAR(450) NOT NULL,
        PRIMARY KEY (LoginProvider, ProviderKey),
        CONSTRAINT FK_AspNetUserLogins_AspNetUsers FOREIGN KEY (UserId) 
            REFERENCES AspNetUsers(Id) ON DELETE CASCADE
    );
END
GO

-- User Tokens
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserTokens')
BEGIN
    CREATE TABLE AspNetUserTokens (
        UserId NVARCHAR(450) NOT NULL,
        LoginProvider NVARCHAR(450) NOT NULL,
        Name NVARCHAR(450) NOT NULL,
        Value NVARCHAR(MAX) NULL,
        PRIMARY KEY (UserId, LoginProvider, Name),
        CONSTRAINT FK_AspNetUserTokens_AspNetUsers FOREIGN KEY (UserId) 
            REFERENCES AspNetUsers(Id) ON DELETE CASCADE
    );
END
GO

-- Role Claims
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoleClaims')
BEGIN
    CREATE TABLE AspNetRoleClaims (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RoleId NVARCHAR(450) NOT NULL,
        ClaimType NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT FK_AspNetRoleClaims_AspNetRoles FOREIGN KEY (RoleId) 
            REFERENCES AspNetRoles(Id) ON DELETE CASCADE
    );
END
GO

-- User Roles
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserRoles')
BEGIN
    CREATE TABLE AspNetUserRoles (
        UserId NVARCHAR(450) NOT NULL,
        RoleId NVARCHAR(450) NOT NULL,
        PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_AspNetUserRoles_AspNetUsers FOREIGN KEY (UserId) 
            REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AspNetUserRoles_AspNetRoles FOREIGN KEY (RoleId) 
            REFERENCES AspNetRoles(Id) ON DELETE CASCADE
    );
END
GO

-- ============================================
-- Application Tables
-- ============================================

-- File Processing Log
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FileProcessingLogs')
BEGIN
    CREATE TABLE FileProcessingLogs (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId NVARCHAR(450) NULL,
        OperationType INT NOT NULL,
        SourceFormat NVARCHAR(50) NULL,
        TargetFormat NVARCHAR(50) NULL,
        OriginalFileName NVARCHAR(500) NULL,
        OriginalFileSize BIGINT NOT NULL DEFAULT 0,
        ProcessedFileSize BIGINT NOT NULL DEFAULT 0,
        CompressionRatio DECIMAL(18,4) NULL,
        PageCount INT NULL,
        Status INT NOT NULL DEFAULT 0,
        ErrorMessage NVARCHAR(MAX) NULL,
        ProcessingTimeMs BIGINT NULL,
        IpAddress NVARCHAR(50) NULL,
        UserAgent NVARCHAR(500) NULL,
        WasDownloaded BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NULL,
        CONSTRAINT FK_FileProcessingLogs_AspNetUsers FOREIGN KEY (UserId) 
            REFERENCES AspNetUsers(Id) ON DELETE SET NULL
    );
END
GO

-- Create index for faster queries
CREATE INDEX IX_FileProcessingLogs_UserId ON FileProcessingLogs(UserId);
CREATE INDEX IX_FileProcessingLogs_CreatedAt ON FileProcessingLogs(CreatedAt);
CREATE INDEX IX_FileProcessingLogs_OperationType ON FileProcessingLogs(OperationType);
GO

-- Download Log
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DownloadLogs')
BEGIN
    CREATE TABLE DownloadLogs (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId NVARCHAR(450) NULL,
        FileProcessingLogId BIGINT NULL,
        FileName NVARCHAR(500) NOT NULL,
        FileSize BIGINT NOT NULL DEFAULT 0,
        FileType NVARCHAR(50) NOT NULL,
        DownloadType INT NOT NULL,
        IsPaidUser BIT NOT NULL DEFAULT 0,
        IpAddress NVARCHAR(50) NULL,
        UserAgent NVARCHAR(500) NULL,
        DownloadedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_DownloadLogs_AspNetUsers FOREIGN KEY (UserId) 
            REFERENCES AspNetUsers(Id) ON DELETE SET NULL,
        CONSTRAINT FK_DownloadLogs_FileProcessingLogs FOREIGN KEY (FileProcessingLogId) 
            REFERENCES FileProcessingLogs(Id) ON DELETE SET NULL
    );
END
GO

-- Create index for faster queries
CREATE INDEX IX_DownloadLogs_UserId ON DownloadLogs(UserId);
CREATE INDEX IX_DownloadLogs_DownloadedAt ON DownloadLogs(DownloadedAt);
GO

-- Subscription History
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SubscriptionHistories')
BEGIN
    CREATE TABLE SubscriptionHistories (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId NVARCHAR(450) NOT NULL,
        PreviousTier INT NOT NULL,
        NewTier INT NOT NULL,
        StripeSubscriptionId NVARCHAR(100) NULL,
        StripeInvoiceId NVARCHAR(100) NULL,
        Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Currency NVARCHAR(10) NOT NULL DEFAULT 'USD',
        Action INT NOT NULL,
        Reason NVARCHAR(500) NULL,
        StartDate DATETIME2 NOT NULL,
        EndDate DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_SubscriptionHistories_AspNetUsers FOREIGN KEY (UserId) 
            REFERENCES AspNetUsers(Id) ON DELETE CASCADE
    );
END
GO

-- Create index for faster queries
CREATE INDEX IX_SubscriptionHistories_UserId ON SubscriptionHistories(UserId);
CREATE INDEX IX_SubscriptionHistories_CreatedAt ON SubscriptionHistories(CreatedAt);
GO

-- ============================================
-- SEO Tables
-- ============================================

-- Sitemap URLs (for auto sitemap generation)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SitemapUrls')
BEGIN
    CREATE TABLE SitemapUrls (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Url NVARCHAR(500) NOT NULL,
        LastMod DATETIME2 NULL,
        ChangeFreq NVARCHAR(20) NULL,
        Priority DECIMAL(3,2) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IncludeInSitemap BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Create unique index
CREATE UNIQUE INDEX IX_SitemapUrls_Url ON SitemapUrls(Url);
CREATE INDEX IX_SitemapUrls_IsActive ON SitemapUrls(IsActive);
GO

-- ============================================
-- Utility Stored Procedures
-- ============================================

-- Reset user's daily download count (call this daily via cron job)
IF OBJECT_ID('ResetDailyDownloads', 'P') IS NOT NULL
    DROP PROCEDURE ResetDailyDownloads;
GO

CREATE PROCEDURE ResetDailyDownloads
AS
BEGIN
    UPDATE AspNetUsers 
    SET DownloadsRemaining = 1 
    WHERE SubscriptionTier = 0; -- Free tier
END
GO

-- Cleanup expired file processing logs (call this weekly)
IF OBJECT_ID('CleanupExpiredFiles', 'P') IS NOT NULL
    DROP PROCEDURE CleanupExpiredFiles;
GO

CREATE PROCEDURE CleanupExpiredFiles
AS
BEGIN
    DELETE FROM FileProcessingLogs 
    WHERE ExpiresAt IS NOT NULL 
    AND ExpiresAt < GETUTCDATE();
END
GO

-- Generate sitemap XML
IF OBJECT_ID('GenerateSitemapXml', 'P') IS NOT NULL
    DROP PROCEDURE GenerateSitemapXml;
GO

CREATE PROCEDURE GenerateSitemapXml
AS
BEGIN
    SELECT 
        Url,
        LastMod,
        ChangeFreq,
        Priority
    FROM SitemapUrls
    WHERE IsActive = 1 AND IncludeInSitemap = 1
    ORDER BY Priority DESC, CreatedAt DESC;
END
GO

PRINT 'Database schema created successfully!';
GO
