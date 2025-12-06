-- Create Posts table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Posts')
BEGIN
    CREATE TABLE [dbo].[Posts](
        [PostID] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NULL,
        [Title] [nvarchar](max) NULL,
        [Slug] [nvarchar](max) NULL,
        [Tags] [nvarchar](max) NULL,
        [Content] [nvarchar](max) NULL,
        [FeaturedImage] [nvarchar](max) NULL,
        [MetaTitle] [nvarchar](max) NULL,
        [MetaDescription] [nvarchar](max) NULL,
        [OgTitle] [nvarchar](max) NULL,
        [OgDescription] [nvarchar](max) NULL,
        [OgImage] [nvarchar](max) NULL,
        [ViewCount] [int] NULL,
        [PostStatus] [nvarchar](50) NULL,
        [IsPromoted] [bit] NULL,
        [IsFeatured] [bit] NULL,
        [AllowComments] [bit] NULL,
        [CreatedAt] [datetime2](7) NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [PublishedAt] [datetime2](7) NULL,
        [CommunityID] [int] NULL,
        CONSTRAINT [PK_Posts] PRIMARY KEY CLUSTERED ([PostID] ASC)
    );
    PRINT 'Posts table created successfully';
END
ELSE
BEGIN
    PRINT 'Posts table already exists';
END
GO

-- Create Communities table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Communities')
BEGIN
    CREATE TABLE [dbo].[Communities](
        [CommunityID] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](max) NULL,
        [Slug] [nvarchar](max) NULL,
        [Description] [nvarchar](max) NULL,
        [Rules] [nvarchar](max) NULL,
        [CoverImage] [nvarchar](max) NULL,
        [IconImage] [nvarchar](max) NULL,
        [CreatorId] [nvarchar](450) NULL,
        [IsPrivate] [bit] NULL,
        [IsVerified] [bit] NULL,
        [MemberCount] [int] NULL,
        [CreatedAt] [datetime2](7) NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [CategoryID] [int] NULL,
        CONSTRAINT [PK_Communities] PRIMARY KEY CLUSTERED ([CommunityID] ASC)
    );
    PRINT 'Communities table created successfully';
END
GO

-- Create Comments table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Comments')
BEGIN
    CREATE TABLE [dbo].[Comments](
        [CommentID] [int] IDENTITY(1,1) NOT NULL,
        [PostID] [int] NULL,
        [UserId] [nvarchar](450) NULL,
        [ParentCommentID] [int] NULL,
        [Content] [nvarchar](max) NULL,
        [IsApproved] [bit] NULL,
        [CreatedAt] [datetime2](7) NULL,
        [UpdatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_Comments] PRIMARY KEY CLUSTERED ([CommentID] ASC)
    );
    PRINT 'Comments table created successfully';
END
GO

-- Create Categories table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Categories')
BEGIN
    CREATE TABLE [dbo].[Categories](
        [CategoryID] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](max) NULL,
        [Slug] [nvarchar](max) NULL,
        [ParentCategoryID] [int] NULL,
        [Description] [nvarchar](max) NULL,
        [MetaTitle] [nvarchar](max) NULL,
        [MetaDescription] [nvarchar](max) NULL,
        [OgTitle] [nvarchar](max) NULL,
        [OgDescription] [nvarchar](max) NULL,
        [OgImage] [nvarchar](max) NULL,
        [IconClass] [nvarchar](max) NULL,
        [DisplayOrder] [int] NULL,
        [IsActive] [bit] NULL,
        [CreatedAt] [datetime2](7) NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](max) NULL,
        [UpdatedBy] [nvarchar](max) NULL,
        [IsDisabledParent] [bit] NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([CategoryID] ASC)
    );
    PRINT 'Categories table created successfully';
END
GO

-- Create Tags table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Tags')
BEGIN
    CREATE TABLE [dbo].[Tags](
        [TagID] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](max) NULL,
        [Slug] [nvarchar](max) NULL,
        [Description] [nvarchar](max) NULL,
        [CreatedAt] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](max) NULL,
        CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED ([TagID] ASC)
    );
    PRINT 'Tags table created successfully';
END
GO

-- Create PostTags table (composite key)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PostTags')
BEGIN
    CREATE TABLE [dbo].[PostTags](
        [PostID] [int] NOT NULL,
        [TagID] [int] NOT NULL,
        CONSTRAINT [PK_PostTags] PRIMARY KEY CLUSTERED ([PostID] ASC, [TagID] ASC)
    );
    PRINT 'PostTags table created successfully';
END
GO

-- Create Reactions table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Reactions')
BEGIN
    CREATE TABLE [dbo].[Reactions](
        [ReactionID] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NULL,
        [PostID] [int] NULL,
        [CommentID] [int] NULL,
        [ReactionType] [nvarchar](50) NULL,
        [CreatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_Reactions] PRIMARY KEY CLUSTERED ([ReactionID] ASC)
    );
    PRINT 'Reactions table created successfully';
END
GO

-- Create UserProfiles table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserProfiles')
BEGIN
    CREATE TABLE [dbo].[UserProfiles](
        [UserProfileID] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NULL,
        [Bio] [nvarchar](max) NULL,
        [ProfileImage] [nvarchar](max) NULL,
        [CoverImage] [nvarchar](max) NULL,
        [Location] [nvarchar](max) NULL,
        [Website] [nvarchar](max) NULL,
        [TwitterHandle] [nvarchar](max) NULL,
        [FacebookUrl] [nvarchar](max) NULL,
        [LinkedInUrl] [nvarchar](max) NULL,
        [DisplayName] [nvarchar](max) NULL,
        [LastLoginAt] [datetime2](7) NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NOT NULL,
        [IsActive] [bit] NOT NULL,
        CONSTRAINT [PK_UserProfiles] PRIMARY KEY CLUSTERED ([UserProfileID] ASC)
    );
    PRINT 'UserProfiles table created successfully';
END
GO

-- Create CommunityMembers table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CommunityMembers')
BEGIN
    CREATE TABLE [dbo].[CommunityMembers](
        [CommunityMemberID] [int] IDENTITY(1,1) NOT NULL,
        [CommunityID] [int] NOT NULL,
        [UserId] [nvarchar](450) NULL,
        [Role] [nvarchar](50) NULL,
        [JoinedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_CommunityMembers] PRIMARY KEY CLUSTERED ([CommunityMemberID] ASC)
    );
    PRINT 'CommunityMembers table created successfully';
END
GO

-- Create ChatRooms table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ChatRooms')
BEGIN
    CREATE TABLE [dbo].[ChatRooms](
        [RoomID] [int] IDENTITY(1,1) NOT NULL,
        [RoomType] [nvarchar](50) NULL,
        [CommunityID] [int] NULL,
        [Name] [nvarchar](max) NULL,
        [CreatedAt] [datetime2](7) NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](450) NULL,
        CONSTRAINT [PK_ChatRooms] PRIMARY KEY CLUSTERED ([RoomID] ASC)
    );
    PRINT 'ChatRooms table created successfully';
END
GO

-- Create ChatRoomMembers table (composite key)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ChatRoomMembers')
BEGIN
    CREATE TABLE [dbo].[ChatRoomMembers](
        [RoomID] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [JoinedAt] [datetime2](7) NULL,
        [LastReadAt] [datetime2](7) NULL,
        CONSTRAINT [PK_ChatRoomMembers] PRIMARY KEY CLUSTERED ([RoomID] ASC, [UserId] ASC)
    );
    PRINT 'ChatRoomMembers table created successfully';
END
GO

-- Create Notifications table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Notifications')
BEGIN
    CREATE TABLE [dbo].[Notifications](
        [NotificationID] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NULL,
        [Title] [nvarchar](max) NULL,
        [Content] [nvarchar](max) NULL,
        [Type] [nvarchar](50) NULL,
        [ReferenceID] [int] NULL,
        [ReferenceType] [nvarchar](50) NULL,
        [IsRead] [bit] NULL,
        [CreatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED ([NotificationID] ASC)
    );
    PRINT 'Notifications table created successfully';
END
GO

