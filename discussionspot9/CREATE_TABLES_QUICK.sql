-- Run this in SQL Server Management Studio (SSMS)
-- Connect to: srv749153.hstgr.cloud
-- Database: u749153_dsdb

-- Table 1: PostKeywords
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PostKeywords')
BEGIN
    CREATE TABLE PostKeywords (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PostId INT NOT NULL,
        Keyword NVARCHAR(200) NOT NULL,
        KeywordType NVARCHAR(20) NOT NULL DEFAULT 'Secondary',
        SearchVolume BIGINT NOT NULL DEFAULT 0,
        Competition NVARCHAR(20) NULL,
        SuggestedBidLow DECIMAL(10,2) NOT NULL DEFAULT 0,
        SuggestedBidHigh DECIMAL(10,2) NOT NULL DEFAULT 0,
        DifficultyScore INT NOT NULL DEFAULT 0,
        Priority INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    )
    PRINT 'PostKeywords created'
END
GO

-- Table 2: EnhancedSeoMetadata
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EnhancedSeoMetadata')
BEGIN
    CREATE TABLE EnhancedSeoMetadata (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PostId INT NOT NULL,
        OriginalMetaDescription NVARCHAR(500) NULL,
        OptimizedMetaDescription NVARCHAR(500) NULL,
        PredictedCtrImprovement DECIMAL(5,2) NOT NULL DEFAULT 0,
        EmotionalTriggers NVARCHAR(500) NULL,
        PowerWords NVARCHAR(500) NULL,
        PrimaryKeywords NVARCHAR(200) NULL,
        SecondaryKeywords NVARCHAR(500) NULL,
        LsiKeywords NVARCHAR(1000) NULL,
        TotalSearchVolume BIGINT NOT NULL DEFAULT 0,
        ReadabilityScore DECIMAL(5,2) NOT NULL DEFAULT 0,
        SeoScore INT NOT NULL DEFAULT 0,
        KeywordDensity DECIMAL(5,2) NOT NULL DEFAULT 0,
        CompetitorAnalysis NVARCHAR(MAX) NULL,
        SerpPreview NVARCHAR(MAX) NULL,
        IsApproved BIT NOT NULL DEFAULT 0,
        ApprovedBy NVARCHAR(450) NULL,
        ApprovedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    )
    PRINT 'EnhancedSeoMetadata created'
END
GO

-- Table 3: MultiSiteRevenues (NO FOREIGN KEY to avoid issues)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MultiSiteRevenues')
BEGIN
    CREATE TABLE MultiSiteRevenues (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SiteDomain NVARCHAR(100) NOT NULL,
        Date DATETIME2 NOT NULL,
        PostId INT NULL,
        PostUrl NVARCHAR(500) NULL,
        Earnings DECIMAL(18,2) NOT NULL DEFAULT 0,
        EstimatedEarnings DECIMAL(18,2) NOT NULL DEFAULT 0,
        PageViews INT NOT NULL DEFAULT 0,
        AdClicks INT NOT NULL DEFAULT 0,
        CTR DECIMAL(5,2) NOT NULL DEFAULT 0,
        CPC DECIMAL(10,2) NOT NULL DEFAULT 0,
        RPM DECIMAL(10,2) NOT NULL DEFAULT 0,
        AdImpressions INT NOT NULL DEFAULT 0,
        ActiveViewViewableImpressions INT NOT NULL DEFAULT 0,
        Coverage DECIMAL(5,2) NOT NULL DEFAULT 0,
        SyncedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        Source NVARCHAR(50) NOT NULL DEFAULT 'AdSense'
    )
    PRINT 'MultiSiteRevenues created'
END
GO

-- Verify
SELECT 'PostKeywords' AS TableName, COUNT(*) AS ColumnCount
FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PostKeywords'
UNION ALL
SELECT 'EnhancedSeoMetadata', COUNT(*) 
FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EnhancedSeoMetadata'
UNION ALL
SELECT 'MultiSiteRevenues', COUNT(*) 
FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MultiSiteRevenues'
GO

