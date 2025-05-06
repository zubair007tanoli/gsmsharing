-- =============================================
-- DiscussionSpot Database Schema (Reddit-like)
-- =============================================

-- ASP.NET Core Identity tables are assumed to exist
-- We'll build on top of AspNetUsers table

-- =============================================
-- User Profile Extension
-- =============================================
CREATE TABLE UserProfile (
    UserId NVARCHAR(450) PRIMARY KEY,
    DisplayName NVARCHAR(100) NOT NULL,
    Bio NVARCHAR(MAX),
    AvatarUrl NVARCHAR(2048),
    BannerUrl NVARCHAR(2048),
    Website NVARCHAR(2048),
    Location NVARCHAR(100),
    JoinDate DATETIME2 DEFAULT GETDATE(),
    LastActive DATETIME2 DEFAULT GETDATE(),
    KarmaPoints INT DEFAULT 0,
    IsVerified BIT DEFAULT 0,
    CONSTRAINT FK_UserProfile_AspNetUsers FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

-- =============================================
-- Category Hierarchy
-- =============================================
CREATE TABLE Category (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Slug NVARCHAR(120) NOT NULL,
    Description NVARCHAR(MAX),
    ParentCategoryId INT NULL,
    DisplayOrder INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT UQ_Category_Slug UNIQUE (Slug),
    CONSTRAINT FK_Category_ParentCategory FOREIGN KEY (ParentCategoryId) REFERENCES Category(CategoryId)
);

-- =============================================
-- Subreddit/Community
-- =============================================
CREATE TABLE Community (
    CommunityId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Slug NVARCHAR(120) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    ShortDescription NVARCHAR(500),
    CategoryId INT,
    CreatorId NVARCHAR(450),
    CommunityType NVARCHAR(20) DEFAULT 'public', -- public, private, restricted
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    IconUrl NVARCHAR(2048),
    BannerUrl NVARCHAR(2048),
    ThemeColor NVARCHAR(20),
    MemberCount INT DEFAULT 0,
    PostCount INT DEFAULT 0,
    Rules NVARCHAR(MAX),
    IsNSFW BIT DEFAULT 0,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT UQ_Community_Slug UNIQUE (Slug),
    CONSTRAINT FK_Community_Category FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId) ON DELETE SET NULL,
    CONSTRAINT FK_Community_Creator FOREIGN KEY (CreatorId) REFERENCES AspNetUsers(Id) ON DELETE SET NULL,
    CONSTRAINT CK_Community_Type CHECK (CommunityType IN ('public', 'private', 'restricted'))
);

-- =============================================
-- Community Membership
-- =============================================
CREATE TABLE CommunityMember (
    UserId NVARCHAR(450) NOT NULL,
    CommunityId INT NOT NULL,
    Role NVARCHAR(20) DEFAULT 'member', -- member, moderator, admin
    JoinedAt DATETIME2 DEFAULT GETDATE(),
    NotificationPreference NVARCHAR(20) DEFAULT 'all',
    CONSTRAINT PK_CommunityMember PRIMARY KEY (UserId, CommunityId),
    CONSTRAINT FK_CommunityMember_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CommunityMember_Community FOREIGN KEY (CommunityId) REFERENCES Community(CommunityId) ON DELETE CASCADE,
    CONSTRAINT CK_CommunityMember_Role CHECK (Role IN ('member', 'moderator', 'admin')),
    CONSTRAINT CK_CommunityMember_NotificationPreference CHECK (NotificationPreference IN ('all', 'important', 'none'))
);

-- =============================================
-- Post
-- =============================================
-- =============================================
-- Modified Post Table with Poll Support
-- =============================================
CREATE TABLE Post (
    PostId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(300) NOT NULL,
    Slug NVARCHAR(320) NOT NULL,
    Content NVARCHAR(MAX),
    UserId NVARCHAR(450),
    CommunityId INT NOT NULL,
    PostType NVARCHAR(20) DEFAULT 'text', -- text, link, image, video, poll
    Url NVARCHAR(2048), -- For link posts
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    UpvoteCount INT DEFAULT 0,
    DownvoteCount INT DEFAULT 0,
    CommentCount INT DEFAULT 0,
    Score INT DEFAULT 0, -- Simplified reddit-like score
    Status NVARCHAR(20) DEFAULT 'published', -- published, removed, deleted, archived
    IsPinned BIT DEFAULT 0,
    IsLocked BIT DEFAULT 0,
    IsNSFW BIT DEFAULT 0,
    IsSpoiler BIT DEFAULT 0,
    ViewCount INT DEFAULT 0,
    HasPoll BIT DEFAULT 0, -- Flag to indicate if post has an active poll
    PollOptionCount INT DEFAULT 0, -- Number of options in the poll
    PollVoteCount INT DEFAULT 0, -- Total number of votes on the poll
    PollExpiresAt DATETIME2, -- When the poll expires (if applicable)
    CONSTRAINT FK_Post_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Post_Community FOREIGN KEY (CommunityId) REFERENCES Community(CommunityId) ON DELETE CASCADE,
    CONSTRAINT UQ_Post_Slug_CommunityId UNIQUE (Slug, CommunityId),
    CONSTRAINT CK_Post_Type CHECK (PostType IN ('text', 'link', 'image', 'video', 'poll')),
    CONSTRAINT CK_Post_Status CHECK (Status IN ('published', 'removed', 'deleted', 'archived'))
);

-- Add index for poll-related queries
CREATE INDEX IX_Post_PostType_HasPoll ON Post(PostType, HasPoll) 
    WHERE PostType = 'poll' AND HasPoll = 1;
    
-- Add index for expiring polls
CREATE INDEX IX_Post_PollExpiresAt ON Post(PollExpiresAt)
    WHERE PollExpiresAt IS NOT NULL;

-- =============================================
-- Media
-- =============================================
CREATE TABLE Media (
    MediaId INT IDENTITY(1,1) PRIMARY KEY,
    Url NVARCHAR(2048) NOT NULL,
    ThumbnailUrl NVARCHAR(2048),
    UserId NVARCHAR(450),
    PostId INT,
    MediaType NVARCHAR(20) NOT NULL, -- image, video, document, audio
    ContentType NVARCHAR(100), -- MIME type
    FileName NVARCHAR(255),
    FileSize BIGINT,
    Width INT,
    Height INT,
    Duration INT, -- For video/audio in seconds
    Caption NVARCHAR(500),
    AltText NVARCHAR(500), -- For accessibility
    UploadedAt DATETIME2 DEFAULT GETDATE(),
    StorageProvider NVARCHAR(50) DEFAULT 'local', -- local, s3, cloudinary, etc.
    StoragePath NVARCHAR(500),
    IsProcessed BIT DEFAULT 0,
    CONSTRAINT FK_Media_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Media_Post FOREIGN KEY (PostId) REFERENCES Post(PostId) ON DELETE CASCADE,
    CONSTRAINT CK_Media_Type CHECK (MediaType IN ('image', 'video', 'document', 'audio'))
);

-- =============================================
-- Comments
-- =============================================
CREATE TABLE Comment (
    CommentId INT IDENTITY(1,1) PRIMARY KEY,
    Content NVARCHAR(MAX) NOT NULL,
    UserId NVARCHAR(450),
    PostId INT NOT NULL,
    ParentCommentId INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    UpvoteCount INT DEFAULT 0,
    DownvoteCount INT DEFAULT 0,
    Score INT DEFAULT 0,
    IsEdited BIT DEFAULT 0,
    IsDeleted BIT DEFAULT 0,
    TreeLevel INT DEFAULT 0, -- Comment hierarchy level
    CONSTRAINT FK_Comment_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Comment_Post FOREIGN KEY (PostId) REFERENCES Post(PostId) ON DELETE CASCADE,
    CONSTRAINT FK_Comment_ParentComment FOREIGN KEY (ParentCommentId) REFERENCES Comment(CommentId)
);

-- =============================================
-- Votes
-- =============================================
CREATE TABLE PostVote (
    UserId NVARCHAR(450) NOT NULL,
    PostId INT NOT NULL,
    VoteType INT NOT NULL, -- 1 for upvote, -1 for downvote
    VotedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT PK_PostVote PRIMARY KEY (UserId, PostId),
    CONSTRAINT FK_PostVote_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PostVote_Post FOREIGN KEY (PostId) REFERENCES Post(PostId) ON DELETE CASCADE,
    CONSTRAINT CK_PostVote_Type CHECK (VoteType IN (-1, 1))
);

CREATE TABLE CommentVote (
    UserId NVARCHAR(450) NOT NULL,
    CommentId INT NOT NULL,
    VoteType INT NOT NULL, -- 1 for upvote, -1 for downvote
    VotedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT PK_CommentVote PRIMARY KEY (UserId, CommentId),
    CONSTRAINT FK_CommentVote_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CommentVote_Comment FOREIGN KEY (CommentId) REFERENCES Comment(CommentId) ON DELETE CASCADE,
    CONSTRAINT CK_CommentVote_Type CHECK (VoteType IN (-1, 1))
);

-- =============================================
-- SEO Optimization
-- =============================================
CREATE TABLE SeoMetadata (
    EntityType NVARCHAR(20) NOT NULL, -- community, post
    EntityId INT NOT NULL,
    MetaTitle NVARCHAR(200),
    MetaDescription NVARCHAR(500),
    CanonicalUrl NVARCHAR(2048),
    OgTitle NVARCHAR(200),
    OgDescription NVARCHAR(500),
    OgImageUrl NVARCHAR(2048),
    TwitterCard NVARCHAR(20) DEFAULT 'summary',
    TwitterTitle NVARCHAR(200),
    TwitterDescription NVARCHAR(500),
    TwitterImageUrl NVARCHAR(2048),
    Keywords NVARCHAR(500),
    StructuredData NVARCHAR(MAX), -- JSON-LD format
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT PK_SeoMetadata PRIMARY KEY (EntityType, EntityId),
    CONSTRAINT CK_SeoMetadata_EntityType CHECK (EntityType IN ('community', 'post'))
);

-- =============================================
-- Tags
-- =============================================
CREATE TABLE Tag (
    TagId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Slug NVARCHAR(120) NOT NULL,
    Description NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    PostCount INT DEFAULT 0,
    CONSTRAINT UQ_Tag_Slug UNIQUE (Slug)
);

CREATE TABLE PostTag (
    PostId INT NOT NULL,
    TagId INT NOT NULL,
    CONSTRAINT PK_PostTag PRIMARY KEY (PostId, TagId),
    CONSTRAINT FK_PostTag_Post FOREIGN KEY (PostId) REFERENCES Post(PostId) ON DELETE CASCADE,
    CONSTRAINT FK_PostTag_Tag FOREIGN KEY (TagId) REFERENCES Tag(TagId) ON DELETE CASCADE
);

-- =============================================
-- Awards & Badges (Premium Features)
-- =============================================
CREATE TABLE Award (
    AwardId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    IconUrl NVARCHAR(2048) NOT NULL,
    CoinCost INT NOT NULL,
    GiveKarma INT DEFAULT 0,
    ReceiveKarma INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE PostAward (
    PostAwardId INT IDENTITY(1,1) PRIMARY KEY,
    PostId INT NOT NULL,
    AwardId INT NOT NULL,
    AwardedByUserId NVARCHAR(450),
    AwardedAt DATETIME2 DEFAULT GETDATE(),
    Message NVARCHAR(500),
    IsAnonymous BIT DEFAULT 0,
    CONSTRAINT FK_PostAward_Post FOREIGN KEY (PostId) REFERENCES Post(PostId) ON DELETE CASCADE,
    CONSTRAINT FK_PostAward_Award FOREIGN KEY (AwardId) REFERENCES Award(AwardId),
    CONSTRAINT FK_PostAward_User FOREIGN KEY (AwardedByUserId) REFERENCES AspNetUsers(Id) ON DELETE SET NULL
);

CREATE TABLE CommentAward (
    CommentAwardId INT IDENTITY(1,1) PRIMARY KEY,
    CommentId INT NOT NULL,
    AwardId INT NOT NULL,
    AwardedByUserId NVARCHAR(450),
    AwardedAt DATETIME2 DEFAULT GETDATE(),
    Message NVARCHAR(500),
    IsAnonymous BIT DEFAULT 0,
    CONSTRAINT FK_CommentAward_Comment FOREIGN KEY (CommentId) REFERENCES Comment(CommentId) ON DELETE CASCADE,
    CONSTRAINT FK_CommentAward_Award FOREIGN KEY (AwardId) REFERENCES Award(AwardId),
    CONSTRAINT FK_CommentAward_User FOREIGN KEY (AwardedByUserId) REFERENCES AspNetUsers(Id) ON DELETE SET NULL
);

-- =============================================
-- User Account Handling for Test User
-- =============================================
-- Set variables for the test user
DECLARE @UserId NVARCHAR(450) = NEWID(); -- Generate a new GUID
DECLARE @UserName NVARCHAR(256) = 'testuser@discussionspot.com';
DECLARE @NormalizedUserName NVARCHAR(256) = 'TESTUSER@DISCUSSIONSPOT.COM';
DECLARE @Email NVARCHAR(256) = 'testuser@discussionspot.com';
DECLARE @NormalizedEmail NVARCHAR(256) = 'TESTUSER@DISCUSSIONSPOT.COM';
DECLARE @PasswordHash NVARCHAR(MAX) = 'AQAAAAEAACcQAAAAEHxTyIJBGYlvyNvzOvQIQ9Q3irzYhq8kGw/8MkAR1QMJlXrNL61DIyv74aEyFMSHvw=='; -- Hash for 'Password123!'

-- Insert into AspNetUsers table
INSERT INTO [dbo].[AspNetUsers] (
    [Id],
    [UserName],
    [NormalizedUserName],
    [Email],
    [NormalizedEmail],
    [EmailConfirmed],
    [PasswordHash],
    [SecurityStamp],
    [ConcurrencyStamp],
    [PhoneNumber],
    [PhoneNumberConfirmed],
    [TwoFactorEnabled],
    [LockoutEnd],
    [LockoutEnabled],
    [AccessFailedCount]
)
VALUES (
    @UserId,
    @UserName,
    @NormalizedUserName,
    @Email,
    @NormalizedEmail,
    1, -- EmailConfirmed = true
    @PasswordHash,
    NEWID(), -- SecurityStamp
    NEWID(), -- ConcurrencyStamp
    NULL, -- PhoneNumber
    0, -- PhoneNumberConfirmed = false
    0, -- TwoFactorEnabled = false
    NULL, -- LockoutEnd
    1, -- LockoutEnabled = true
    0 -- AccessFailedCount
);

-- Add user to a role (assuming you have the 'User' role created)
DECLARE @RoleId NVARCHAR(450);
SELECT @RoleId = [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'User';

-- Then add the user to that role
INSERT INTO [dbo].[AspNetUserRoles] (
    [UserId],
    [RoleId]
)
VALUES (
    @UserId,
    @RoleId
);

-- Create user profile
INSERT INTO UserProfile (
    UserId,
    DisplayName,
    Bio,
    JoinDate
)
VALUES (
    @UserId,
    'TestUser',
    'This is a test account for DiscussionSpot.',
    GETDATE()
);

-- =============================================
-- Poll System Tables
-- =============================================

-- Poll Options Table
CREATE TABLE PollOption (
    PollOptionId INT IDENTITY(1,1) PRIMARY KEY,
    PostId INT NOT NULL,
    OptionText NVARCHAR(255) NOT NULL,
    DisplayOrder INT DEFAULT 0,
    VoteCount INT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_PollOption_Post FOREIGN KEY (PostId) REFERENCES Post(PostId) ON DELETE CASCADE
);

-- Poll Votes Table - Tracks user votes
CREATE TABLE PollVote (
    UserId NVARCHAR(450) NOT NULL,
    PollOptionId INT NOT NULL,
    VotedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT PK_PollVote PRIMARY KEY (UserId, PollOptionId),
    CONSTRAINT FK_PollVote_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PollVote_PollOption FOREIGN KEY (PollOptionId) REFERENCES PollOption(PollOptionId) ON DELETE CASCADE
);

-- Poll Configuration Table - Store poll settings
CREATE TABLE PollConfiguration (
    PostId INT PRIMARY KEY,
    AllowMultipleChoices BIT DEFAULT 0,
    EndDate DATETIME2 NULL, -- NULL means poll never expires
    ShowResultsBeforeVoting BIT DEFAULT 1,
    ShowResultsBeforeEnd BIT DEFAULT 1,
    AllowAddingOptions BIT DEFAULT 0, -- Allow users to add new options?
    MinOptions INT DEFAULT 2,
    MaxOptions INT DEFAULT 10,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_PollConfiguration_Post FOREIGN KEY (PostId) REFERENCES Post(PostId) ON DELETE CASCADE,
    CONSTRAINT CK_PollConfiguration_MinOptions CHECK (MinOptions >= 2),
    CONSTRAINT CK_PollConfiguration_MaxOptions CHECK (MaxOptions >= MinOptions)
);

-- =============================================
-- Post Table Modification - Add Poll Fields
-- =============================================

-- Note: You should add this to the Post table definition
-- in the main schema. It's shown here separately.

-- Add to 'Post' table (update the existing constraint):
-- PostType NVARCHAR(20) DEFAULT 'text', -- text, link, image, video, poll
-- CONSTRAINT CK_Post_Type CHECK (PostType IN ('text', 'link', 'image', 'video', 'poll'))

-- =============================================
-- Indexes for Poll Performance
-- =============================================
CREATE INDEX IX_PollOption_PostId ON PollOption(PostId);
CREATE INDEX IX_PollVote_PollOptionId ON PollVote(PollOptionId);
CREATE INDEX IX_PollVote_UserId ON PollVote(UserId);
CREATE INDEX IX_PollConfiguration_EndDate ON PollConfiguration(EndDate);

-- =============================================
-- Indexes for Better Performance
-- =============================================
CREATE INDEX IX_Category_ParentCategoryId ON Category(ParentCategoryId);
CREATE INDEX IX_Community_CategoryId ON Community(CategoryId);
CREATE INDEX IX_Community_Slug ON Community(Slug);
CREATE INDEX IX_Post_CommunityId_CreatedAt ON Post(CommunityId, CreatedAt DESC);
CREATE INDEX IX_Post_UserId_CreatedAt ON Post(UserId, CreatedAt DESC);
CREATE INDEX IX_Post_Slug ON Post(Slug);
CREATE INDEX IX_Comment_PostId_CreatedAt ON Comment(PostId, CreatedAt DESC);
CREATE INDEX IX_Comment_UserId ON Comment(UserId);
CREATE INDEX IX_Comment_ParentCommentId ON Comment(ParentCommentId);
CREATE INDEX IX_Media_PostId ON Media(PostId);
CREATE INDEX IX_Media_UserId ON Media(UserId);
CREATE INDEX IX_UserProfile_DisplayName ON UserProfile(DisplayName);
CREATE INDEX IX_PostTag_TagId ON PostTag(TagId);
CREATE INDEX IX_Tag_Slug ON Tag(Slug);