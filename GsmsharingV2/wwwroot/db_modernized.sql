/* GSMSharing.com - Master Database Schema
   Version: 4.0 (Final Production Ready)
   Tech: ASP.NET Core Identity + Python Backend
*/

-- =============================================
-- SECTION 1: ASP.NET CORE IDENTITY (Authentication)
-- =============================================
-- Standard Identity tables with custom columns for your users.
create database gsmsharingv4
go
use gsmsharingv4
go
CREATE TABLE AspNetUsers (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    UserName NVARCHAR(256),
    NormalizedUserName NVARCHAR(256),
    Email NVARCHAR(256),
    NormalizedEmail NVARCHAR(256),
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    PasswordHash NVARCHAR(MAX),
    SecurityStamp NVARCHAR(MAX),
    ConcurrencyStamp NVARCHAR(MAX),
    PhoneNumber NVARCHAR(MAX),
    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    LockoutEnd DATETIMEOFFSET,
    LockoutEnabled BIT NOT NULL DEFAULT 0,
    AccessFailedCount INT NOT NULL DEFAULT 0,
    
    -- [CUSTOM FIELDS]
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    AvatarPath NVARCHAR(500),       -- Local path to image
    RegistrationIP NVARCHAR(45),
    City NVARCHAR(100),             -- For Local Ads
    Country NVARCHAR(100),
    IsSellerVerified BIT DEFAULT 0, -- Verified badge for OLX sellers
    CreditsBalance DECIMAL(18,2) DEFAULT 0, -- Wallet for premium actions
    CreatedDate DATETIME2 DEFAULT GETDATE(),

    INDEX IX_AspNetUsers_NormalizedUserName (NormalizedUserName),
    INDEX IX_AspNetUsers_NormalizedEmail (NormalizedEmail)
);

CREATE TABLE AspNetRoles (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(256),
    NormalizedName NVARCHAR(256),
    ConcurrencyStamp NVARCHAR(MAX),
    Description NVARCHAR(255),
    INDEX IX_AspNetRoles_NormalizedName (NormalizedName)
);

CREATE TABLE AspNetUserRoles (
    UserId BIGINT NOT NULL,
    RoleId BIGINT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetUserClaims (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId BIGINT NOT NULL,
    ClaimType NVARCHAR(MAX),
    ClaimValue NVARCHAR(MAX),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetRoleClaims (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RoleId BIGINT NOT NULL,
    ClaimType NVARCHAR(MAX),
    ClaimValue NVARCHAR(MAX),
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetUserLogins (
    LoginProvider NVARCHAR(450) NOT NULL,
    ProviderKey NVARCHAR(450) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX),
    UserId BIGINT NOT NULL,
    PRIMARY KEY (LoginProvider, ProviderKey),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetUserTokens (
    UserId BIGINT NOT NULL,
    LoginProvider NVARCHAR(450) NOT NULL,
    Name NVARCHAR(450) NOT NULL,
    Value NVARCHAR(MAX),
    PRIMARY KEY (UserId, LoginProvider, Name),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

-- =============================================
-- SECTION 2: CLASSIFIED ADS (OLX-Style Marketplace)
-- =============================================
-- Users selling Phones and Laptops to each other.

CREATE TABLE AdCategories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(50) NOT NULL, -- e.g., 'Mobile Phones', 'Laptops', 'Spare Parts'
    Slug NVARCHAR(100) UNIQUE,
    IconClass NVARCHAR(50) -- e.g., 'fa-mobile'
);

CREATE TABLE ClassifiedAds (
    AdID BIGINT PRIMARY KEY IDENTITY(1,1),
    UserID BIGINT FOREIGN KEY REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CategoryID INT FOREIGN KEY REFERENCES AdCategories(CategoryID),
    
    -- Ad Details
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(10) DEFAULT 'PKR',
    
    -- Product Specs
    Brand NVARCHAR(100),       -- Samsung, Apple
    Model NVARCHAR(100),       -- iPhone 15 Pro
    Condition NVARCHAR(50),    -- 'New', 'Used', 'Refurbished'
    
    -- Location
    City NVARCHAR(100),
    Area NVARCHAR(100),
    
    -- Meta
    Status NVARCHAR(50) DEFAULT 'Active', -- Active, Sold, Expired
    ViewCount INT DEFAULT 0,
    PhoneViewCount INT DEFAULT 0, -- How many people clicked "Show Phone Number"
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ExpirationDate DATETIME2,
    
    INDEX IX_Ads_Category (CategoryID),
    INDEX IX_Ads_User (UserID),
    INDEX IX_Ads_Status (Status)
);

CREATE TABLE AdImages (
    ImageID BIGINT PRIMARY KEY IDENTITY(1,1),
    AdID BIGINT FOREIGN KEY REFERENCES ClassifiedAds(AdID) ON DELETE CASCADE,
    ImagePath NVARCHAR(500) NOT NULL, -- Stored locally on server
    IsPrimary BIT DEFAULT 0,
    INDEX IX_AdImages_AdID (AdID)
);

CREATE TABLE SavedAds (
    SavedID BIGINT PRIMARY KEY IDENTITY(1,1),
    UserID BIGINT FOREIGN KEY REFERENCES AspNetUsers(Id),
    AdID BIGINT FOREIGN KEY REFERENCES ClassifiedAds(AdID) ON DELETE CASCADE,
    SavedDate DATETIME2 DEFAULT GETDATE()
);

-- =============================================
-- SECTION 3: MESSAGING SYSTEM (Buyer <-> Seller)
-- =============================================

CREATE TABLE ChatConversations (
    ConversationID BIGINT PRIMARY KEY IDENTITY(1,1),
    AdID BIGINT NULL FOREIGN KEY REFERENCES ClassifiedAds(AdID), -- Linked to an Ad
    BuyerID BIGINT FOREIGN KEY REFERENCES AspNetUsers(Id),
    SellerID BIGINT FOREIGN KEY REFERENCES AspNetUsers(Id),
    LastMessageDate DATETIME2 DEFAULT GETDATE(),
    INDEX IX_Chat_Users (BuyerID, SellerID)
);

CREATE TABLE ChatMessages (
    MessageID BIGINT PRIMARY KEY IDENTITY(1,1),
    ConversationID BIGINT FOREIGN KEY REFERENCES ChatConversations(ConversationID) ON DELETE CASCADE,
    SenderID BIGINT FOREIGN KEY REFERENCES AspNetUsers(Id),
    MessageContent NVARCHAR(MAX),
    IsRead BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    INDEX IX_Messages_Conversation (ConversationID)
);

-- =============================================
-- SECTION 4: FILE REPOSITORY (Firmware/Google Drive)
-- =============================================
-- For GSM Firmware links. Images are local, Files are External.

CREATE TABLE FileCategories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100), -- Samsung, Xiaomi, Schematics
    ParentCategoryID INT NULL FOREIGN KEY REFERENCES FileCategories(CategoryID),
    Slug NVARCHAR(150) UNIQUE
);

CREATE TABLE FileRepository (
    FileID BIGINT PRIMARY KEY IDENTITY(1,1),
    UserID BIGINT FOREIGN KEY REFERENCES AspNetUsers(Id), -- Uploader (Admin or User)
    CategoryID INT FOREIGN KEY REFERENCES FileCategories(CategoryID),
    
    FileName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    
    -- Storage Logic
    StorageProvider NVARCHAR(50) DEFAULT 'GoogleDrive', -- GoogleDrive, Mega, MediaFire
    ExternalUrl NVARCHAR(2000), -- The actual link
    FilePassword NVARCHAR(50),  -- If the RAR/ZIP is locked
    
    -- Technical Data (Filled by Python Script or Manual)
    FileSize NVARCHAR(50),
    MD5Checksum NVARCHAR(32),
    AndroidVersion NVARCHAR(50),
    SecurityPatchLevel DATE,
    
    -- Access
    IsPremium BIT DEFAULT 0,    -- True = Deduct credits
    CreditCost INT DEFAULT 0,
    
    DownloadCount INT DEFAULT 0,
    IsActive BIT DEFAULT 1,     -- False if broken link
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    
    INDEX IX_Files_Category (CategoryID),
    INDEX IX_Files_MD5 (MD5Checksum)
);

-- =============================================
-- SECTION 5: AFFILIATE MARKETING (Amazon/AliExpress)
-- =============================================
-- You post products, users click, you get paid by Amazon.

CREATE TABLE AffiliatePartners (
    PartnerID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100),         -- Amazon, Daraz
    AffiliateTag NVARCHAR(100), -- e.g. "gsmshare-20"
    BaseUrl NVARCHAR(500)       -- https://amazon.com/dp/
);

CREATE TABLE AffiliateProducts (
    ProductID BIGINT PRIMARY KEY IDENTITY(1,1),
    PartnerID INT FOREIGN KEY REFERENCES AffiliatePartners(PartnerID),
    
    Title NVARCHAR(255) NOT NULL,
    Category NVARCHAR(50),      -- 'Repair Tools', 'Accessories'
    
    OriginalLink NVARCHAR(2000), -- The raw product link
    AffiliateLink NVARCHAR(2000),-- The link with your ?tag=
    
    ImageUrl NVARCHAR(500),      -- Local cached image
    PriceDisplay DECIMAL(18,2),
    
    Clicks INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);

-- Analytics: Who clicked what?
CREATE TABLE AffiliateClicks (
    ClickID BIGINT PRIMARY KEY IDENTITY(1,1),
    ProductID BIGINT FOREIGN KEY REFERENCES AffiliateProducts(ProductID),
    UserID BIGINT NULL, -- Nullable (Guests can click)
    IPAddress NVARCHAR(45),
    UserAgent NVARCHAR(500),
    ClickDate DATETIME2 DEFAULT GETDATE(),
    INDEX IX_AffiliateClicks_Date (ClickDate)
);

-- =============================================
-- SECTION 6: SYSTEM LOGS & SETTINGS
-- =============================================

CREATE TABLE SystemSettings (
    SettingKey NVARCHAR(100) PRIMARY KEY, -- e.g., 'SiteName', 'BannerText'
    SettingValue NVARCHAR(MAX),
    LastUpdated DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE AdminLogs (
    LogID BIGINT PRIMARY KEY IDENTITY(1,1),
    AdminUserID BIGINT FOREIGN KEY REFERENCES AspNetUsers(Id),
    Action NVARCHAR(255),
    Details NVARCHAR(MAX),
    LogDate DATETIME2 DEFAULT GETDATE()
);

/* FIX SCRIPT: Resize Identity Keys to fit SQL Server Index Limits 
   Run this to fix the 900 byte warning.
*/

-- 1. Drop the problematic tables if they exist
IF OBJECT_ID('AspNetUserTokens', 'U') IS NOT NULL DROP TABLE AspNetUserTokens;
IF OBJECT_ID('AspNetUserLogins', 'U') IS NOT NULL DROP TABLE AspNetUserLogins;

-- 2. Recreate AspNetUserLogins with NVARCHAR(128)
-- 128 chars * 2 bytes = 256 bytes. 
-- 256 + 256 = 512 bytes (Safe, well under the 900 limit)
CREATE TABLE AspNetUserLogins (
    LoginProvider NVARCHAR(128) NOT NULL, -- WAS 450
    ProviderKey NVARCHAR(128) NOT NULL,   -- WAS 450
    ProviderDisplayName NVARCHAR(MAX),
    UserId BIGINT NOT NULL,
    PRIMARY KEY (LoginProvider, ProviderKey),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

-- 3. Recreate AspNetUserTokens with NVARCHAR(128)
-- 8 (BigInt) + 256 + 256 = 520 bytes (Safe)
CREATE TABLE AspNetUserTokens (
    UserId BIGINT NOT NULL,
    LoginProvider NVARCHAR(128) NOT NULL, -- WAS 450
    Name NVARCHAR(128) NOT NULL,          -- WAS 450
    Value NVARCHAR(MAX),
    PRIMARY KEY (UserId, LoginProvider, Name),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);