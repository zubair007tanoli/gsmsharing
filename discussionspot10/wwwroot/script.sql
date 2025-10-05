USE [master]
GO
/****** Object:  Database [DiscussionspotADO]    Script Date: 10/4/2025 3:27:22 PM ******/
CREATE DATABASE [DiscussionspotADO]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'DiscussionspotADO', FILENAME = N'/var/opt/mssql/data/DiscussionspotADO.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'DiscussionspotADO_log', FILENAME = N'/var/opt/mssql/data/DiscussionspotADO_log.ldf' , SIZE = 335872KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [DiscussionspotADO] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DiscussionspotADO].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DiscussionspotADO] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET ARITHABORT OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [DiscussionspotADO] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DiscussionspotADO] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DiscussionspotADO] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET  ENABLE_BROKER 
GO
ALTER DATABASE [DiscussionspotADO] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DiscussionspotADO] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [DiscussionspotADO] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET RECOVERY FULL 
GO
ALTER DATABASE [DiscussionspotADO] SET  MULTI_USER 
GO
ALTER DATABASE [DiscussionspotADO] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DiscussionspotADO] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DiscussionspotADO] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DiscussionspotADO] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [DiscussionspotADO] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [DiscussionspotADO] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [DiscussionspotADO] SET QUERY_STORE = ON
GO
ALTER DATABASE [DiscussionspotADO] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [DiscussionspotADO]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 10/4/2025 3:27:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Awards]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Awards](
	[AwardId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[IconUrl] [nvarchar](2048) NOT NULL,
	[CoinCost] [int] NOT NULL,
	[GiveKarma] [int] NOT NULL,
	[ReceiveKarma] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Awards] PRIMARY KEY CLUSTERED 
(
	[AwardId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categories](
	[CategoryId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Slug] [nvarchar](120) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ParentCategoryId] [int] NULL,
	[DisplayOrder] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CommentAwards]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommentAwards](
	[CommentAwardId] [int] IDENTITY(1,1) NOT NULL,
	[CommentId] [int] NOT NULL,
	[AwardId] [int] NOT NULL,
	[AwardedByUserId] [nvarchar](450) NULL,
	[AwardedAt] [datetime2](7) NOT NULL,
	[Message] [nvarchar](500) NULL,
	[IsAnonymous] [bit] NOT NULL,
 CONSTRAINT [PK_CommentAwards] PRIMARY KEY CLUSTERED 
(
	[CommentAwardId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Comments]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Comments](
	[CommentId] [int] IDENTITY(1,1) NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[PostId] [int] NOT NULL,
	[ParentCommentId] [int] NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[UpvoteCount] [int] NOT NULL,
	[DownvoteCount] [int] NOT NULL,
	[Score] [int] NOT NULL,
	[IsEdited] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[TreeLevel] [int] NOT NULL,
 CONSTRAINT [PK_Comments] PRIMARY KEY CLUSTERED 
(
	[CommentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CommentVotes]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommentVotes](
	[UserId] [nvarchar](450) NOT NULL,
	[CommentId] [int] NOT NULL,
	[VoteType] [int] NOT NULL,
	[VotedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_CommentVotes] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[CommentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Communities]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Communities](
	[CommunityId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Slug] [nvarchar](120) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ShortDescription] [nvarchar](500) NULL,
	[CategoryId] [int] NULL,
	[CreatorId] [nvarchar](450) NULL,
	[CommunityType] [nvarchar](20) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[IconUrl] [nvarchar](2048) NULL,
	[BannerUrl] [nvarchar](2048) NULL,
	[ThemeColor] [nvarchar](20) NULL,
	[MemberCount] [int] NOT NULL,
	[PostCount] [int] NOT NULL,
	[Rules] [nvarchar](max) NULL,
	[IsNSFW] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Communities] PRIMARY KEY CLUSTERED 
(
	[CommunityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CommunityMembers]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommunityMembers](
	[UserId] [nvarchar](450) NOT NULL,
	[CommunityId] [int] NOT NULL,
	[Role] [nvarchar](20) NOT NULL,
	[JoinedAt] [datetime2](7) NOT NULL,
	[NotificationPreference] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_CommunityMembers] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[CommunityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Media]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Media](
	[MediaId] [int] IDENTITY(1,1) NOT NULL,
	[Url] [nvarchar](2048) NOT NULL,
	[ThumbnailUrl] [nvarchar](2048) NULL,
	[UserId] [nvarchar](450) NULL,
	[PostId] [int] NULL,
	[MediaType] [nvarchar](20) NOT NULL,
	[ContentType] [nvarchar](100) NULL,
	[FileName] [nvarchar](255) NULL,
	[FileSize] [bigint] NULL,
	[Width] [int] NULL,
	[Height] [int] NULL,
	[Duration] [int] NULL,
	[Caption] [nvarchar](500) NULL,
	[AltText] [nvarchar](500) NULL,
	[UploadedAt] [datetime2](7) NOT NULL,
	[StorageProvider] [nvarchar](50) NOT NULL,
	[StoragePath] [nvarchar](500) NULL,
	[IsProcessed] [bit] NOT NULL,
 CONSTRAINT [PK_Media] PRIMARY KEY CLUSTERED 
(
	[MediaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notifications]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notifications](
	[NotificationId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Message] [nvarchar](500) NULL,
	[EntityType] [nvarchar](50) NULL,
	[EntityId] [nvarchar](450) NULL,
	[IsRead] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED 
(
	[NotificationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PollConfigurations]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PollConfigurations](
	[PostId] [int] NOT NULL,
	[AllowMultipleChoices] [bit] NOT NULL,
	[EndDate] [datetime2](7) NULL,
	[ShowResultsBeforeVoting] [bit] NOT NULL,
	[ShowResultsBeforeEnd] [bit] NOT NULL,
	[AllowAddingOptions] [bit] NOT NULL,
	[MinOptions] [int] NOT NULL,
	[MaxOptions] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[PollQuestion] [nvarchar](500) NULL,
	[PollDescription] [nvarchar](1000) NULL,
	[ClosedByUserId] [nvarchar](450) NULL,
	[ClosedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_PollConfigurations] PRIMARY KEY CLUSTERED 
(
	[PostId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PollOptions]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PollOptions](
	[PollOptionId] [int] IDENTITY(1,1) NOT NULL,
	[PostId] [int] NOT NULL,
	[OptionText] [nvarchar](255) NOT NULL,
	[DisplayOrder] [int] NOT NULL,
	[VoteCount] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_PollOptions] PRIMARY KEY CLUSTERED 
(
	[PollOptionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PollVotes]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PollVotes](
	[UserId] [nvarchar](450) NOT NULL,
	[PollOptionId] [int] NOT NULL,
	[VotedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_PollVotes] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[PollOptionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PostAwards]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PostAwards](
	[PostAwardId] [int] IDENTITY(1,1) NOT NULL,
	[PostId] [int] NOT NULL,
	[AwardId] [int] NOT NULL,
	[AwardedByUserId] [nvarchar](450) NULL,
	[AwardedAt] [datetime2](7) NOT NULL,
	[Message] [nvarchar](500) NULL,
	[IsAnonymous] [bit] NOT NULL,
 CONSTRAINT [PK_PostAwards] PRIMARY KEY CLUSTERED 
(
	[PostAwardId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Posts]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Posts](
	[PostId] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](300) NOT NULL,
	[Slug] [nvarchar](320) NOT NULL,
	[Content] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NULL,
	[CommunityId] [int] NOT NULL,
	[PostType] [nvarchar](20) NOT NULL,
	[Url] [nvarchar](2048) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[UpvoteCount] [int] NOT NULL,
	[DownvoteCount] [int] NOT NULL,
	[CommentCount] [int] NOT NULL,
	[Score] [int] NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[IsPinned] [bit] NOT NULL,
	[IsLocked] [bit] NOT NULL,
	[IsNSFW] [bit] NOT NULL,
	[IsSpoiler] [bit] NOT NULL,
	[ViewCount] [int] NOT NULL,
	[HasPoll] [bit] NOT NULL,
	[PollOptionCount] [int] NOT NULL,
	[PollVoteCount] [int] NOT NULL,
	[PollExpiresAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Posts] PRIMARY KEY CLUSTERED 
(
	[PostId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PostTags]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PostTags](
	[PostId] [int] NOT NULL,
	[TagId] [int] NOT NULL,
 CONSTRAINT [PK_PostTags] PRIMARY KEY CLUSTERED 
(
	[PostId] ASC,
	[TagId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PostVotes]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PostVotes](
	[UserId] [nvarchar](450) NOT NULL,
	[PostId] [int] NOT NULL,
	[VoteType] [int] NOT NULL,
	[VotedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_PostVotes] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[PostId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SavedPosts]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SavedPosts](
	[SavedPostId] [int] IDENTITY(1,1) NOT NULL,
	[PostId] [int] NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[SavedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_SavedPosts] PRIMARY KEY CLUSTERED 
(
	[SavedPostId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SeoMetadata]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SeoMetadata](
	[EntityType] [nvarchar](20) NOT NULL,
	[EntityId] [int] NOT NULL,
	[MetaTitle] [nvarchar](200) NULL,
	[MetaDescription] [nvarchar](500) NULL,
	[CanonicalUrl] [nvarchar](2048) NULL,
	[OgTitle] [nvarchar](200) NULL,
	[OgDescription] [nvarchar](500) NULL,
	[OgImageUrl] [nvarchar](2048) NULL,
	[TwitterCard] [nvarchar](20) NOT NULL,
	[TwitterTitle] [nvarchar](200) NULL,
	[TwitterDescription] [nvarchar](500) NULL,
	[TwitterImageUrl] [nvarchar](2048) NULL,
	[Keywords] [nvarchar](500) NULL,
	[StructuredData] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_SeoMetadata] PRIMARY KEY CLUSTERED 
(
	[EntityType] ASC,
	[EntityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tags](
	[TagId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Slug] [nvarchar](120) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[PostCount] [int] NOT NULL,
 CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED 
(
	[TagId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserProfiles]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserProfiles](
	[UserId] [nvarchar](450) NOT NULL,
	[DisplayName] [nvarchar](100) NOT NULL,
	[Bio] [nvarchar](max) NULL,
	[AvatarUrl] [nvarchar](2048) NULL,
	[BannerUrl] [nvarchar](2048) NULL,
	[Website] [nvarchar](2048) NULL,
	[Location] [nvarchar](100) NULL,
	[JoinDate] [datetime2](7) NOT NULL,
	[LastActive] [datetime2](7) NOT NULL,
	[KarmaPoints] [int] NOT NULL,
	[IsVerified] [bit] NOT NULL,
 CONSTRAINT [PK_UserProfiles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetRoleClaims_RoleId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[NormalizedName] ASC
)
WHERE ([NormalizedName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserClaims_UserId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserLogins_UserId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserRoles_RoleId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [EmailIndex]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UserNameIndex]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedUserName] ASC
)
WHERE ([NormalizedUserName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Categories_ParentCategoryId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Categories_ParentCategoryId] ON [dbo].[Categories]
(
	[ParentCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Categories_Slug]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Categories_Slug] ON [dbo].[Categories]
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_CommentAwards_AwardedByUserId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_CommentAwards_AwardedByUserId] ON [dbo].[CommentAwards]
(
	[AwardedByUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CommentAwards_AwardId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_CommentAwards_AwardId] ON [dbo].[CommentAwards]
(
	[AwardId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CommentAwards_CommentId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_CommentAwards_CommentId] ON [dbo].[CommentAwards]
(
	[CommentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Comments_ParentCommentId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Comments_ParentCommentId] ON [dbo].[Comments]
(
	[ParentCommentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Comments_PostId_CreatedAt]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Comments_PostId_CreatedAt] ON [dbo].[Comments]
(
	[PostId] ASC,
	[CreatedAt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Comments_UserId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Comments_UserId] ON [dbo].[Comments]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CommentVotes_CommentId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_CommentVotes_CommentId] ON [dbo].[CommentVotes]
(
	[CommentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Communities_CategoryId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Communities_CategoryId] ON [dbo].[Communities]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Communities_CreatorId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Communities_CreatorId] ON [dbo].[Communities]
(
	[CreatorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Communities_Slug]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Communities_Slug] ON [dbo].[Communities]
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CommunityMembers_CommunityId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_CommunityMembers_CommunityId] ON [dbo].[CommunityMembers]
(
	[CommunityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Media_PostId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Media_PostId] ON [dbo].[Media]
(
	[PostId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Media_UserId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Media_UserId] ON [dbo].[Media]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Notifications_UserId_IsRead]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Notifications_UserId_IsRead] ON [dbo].[Notifications]
(
	[UserId] ASC,
	[IsRead] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PollConfigurations_ClosedAt]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_PollConfigurations_ClosedAt] ON [dbo].[PollConfigurations]
(
	[ClosedAt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PollConfigurations_EndDate]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_PollConfigurations_EndDate] ON [dbo].[PollConfigurations]
(
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PollOptions_PostId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_PollOptions_PostId] ON [dbo].[PollOptions]
(
	[PostId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PollVotes_PollOptionId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_PollVotes_PollOptionId] ON [dbo].[PollVotes]
(
	[PollOptionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PollVotes_UserId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_PollVotes_UserId] ON [dbo].[PollVotes]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PostAwards_AwardedByUserId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_PostAwards_AwardedByUserId] ON [dbo].[PostAwards]
(
	[AwardedByUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PostAwards_AwardId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_PostAwards_AwardId] ON [dbo].[PostAwards]
(
	[AwardId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PostAwards_PostId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_PostAwards_PostId] ON [dbo].[PostAwards]
(
	[PostId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Posts_CommunityId_CreatedAt]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Posts_CommunityId_CreatedAt] ON [dbo].[Posts]
(
	[CommunityId] ASC,
	[CreatedAt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Posts_PollExpiresAt]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Posts_PollExpiresAt] ON [dbo].[Posts]
(
	[PollExpiresAt] ASC
)
WHERE ([PollExpiresAt] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Posts_PostType_HasPoll]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Posts_PostType_HasPoll] ON [dbo].[Posts]
(
	[PostType] ASC,
	[HasPoll] ASC
)
WHERE ([PostType]='poll' AND [HasPoll]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Posts_Slug]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Posts_Slug] ON [dbo].[Posts]
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Posts_Slug_CommunityId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Posts_Slug_CommunityId] ON [dbo].[Posts]
(
	[Slug] ASC,
	[CommunityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Posts_UserId_CreatedAt]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_Posts_UserId_CreatedAt] ON [dbo].[Posts]
(
	[UserId] ASC,
	[CreatedAt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PostTags_TagId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_PostTags_TagId] ON [dbo].[PostTags]
(
	[TagId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PostVotes_PostId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_PostVotes_PostId] ON [dbo].[PostVotes]
(
	[PostId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_SavedPosts_PostId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_SavedPosts_PostId] ON [dbo].[SavedPosts]
(
	[PostId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_SavedPosts_UserId]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE NONCLUSTERED INDEX [IX_SavedPosts_UserId] ON [dbo].[SavedPosts]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Tags_Slug]    Script Date: 10/4/2025 3:27:30 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Tags_Slug] ON [dbo].[Tags]
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Awards] ADD  DEFAULT ((0)) FOR [GiveKarma]
GO
ALTER TABLE [dbo].[Awards] ADD  DEFAULT ((0)) FOR [ReceiveKarma]
GO
ALTER TABLE [dbo].[Awards] ADD  DEFAULT (CONVERT([bit],(1))) FOR [IsActive]
GO
ALTER TABLE [dbo].[Awards] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Categories] ADD  DEFAULT ((0)) FOR [DisplayOrder]
GO
ALTER TABLE [dbo].[Categories] ADD  DEFAULT (CONVERT([bit],(1))) FOR [IsActive]
GO
ALTER TABLE [dbo].[Categories] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Categories] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[CommentAwards] ADD  DEFAULT (getdate()) FOR [AwardedAt]
GO
ALTER TABLE [dbo].[CommentAwards] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsAnonymous]
GO
ALTER TABLE [dbo].[Comments] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Comments] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Comments] ADD  DEFAULT ((0)) FOR [UpvoteCount]
GO
ALTER TABLE [dbo].[Comments] ADD  DEFAULT ((0)) FOR [DownvoteCount]
GO
ALTER TABLE [dbo].[Comments] ADD  DEFAULT ((0)) FOR [Score]
GO
ALTER TABLE [dbo].[Comments] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsEdited]
GO
ALTER TABLE [dbo].[Comments] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Comments] ADD  DEFAULT ((0)) FOR [TreeLevel]
GO
ALTER TABLE [dbo].[CommentVotes] ADD  DEFAULT (getdate()) FOR [VotedAt]
GO
ALTER TABLE [dbo].[Communities] ADD  DEFAULT (N'public') FOR [CommunityType]
GO
ALTER TABLE [dbo].[Communities] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Communities] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Communities] ADD  DEFAULT ((0)) FOR [MemberCount]
GO
ALTER TABLE [dbo].[Communities] ADD  DEFAULT ((0)) FOR [PostCount]
GO
ALTER TABLE [dbo].[Communities] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsNSFW]
GO
ALTER TABLE [dbo].[Communities] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[CommunityMembers] ADD  DEFAULT (N'member') FOR [Role]
GO
ALTER TABLE [dbo].[CommunityMembers] ADD  DEFAULT (getdate()) FOR [JoinedAt]
GO
ALTER TABLE [dbo].[CommunityMembers] ADD  DEFAULT (N'all') FOR [NotificationPreference]
GO
ALTER TABLE [dbo].[Media] ADD  DEFAULT (getdate()) FOR [UploadedAt]
GO
ALTER TABLE [dbo].[Media] ADD  DEFAULT (N'local') FOR [StorageProvider]
GO
ALTER TABLE [dbo].[Media] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsProcessed]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsRead]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[PollConfigurations] ADD  DEFAULT (CONVERT([bit],(0))) FOR [AllowMultipleChoices]
GO
ALTER TABLE [dbo].[PollConfigurations] ADD  DEFAULT (CONVERT([bit],(1))) FOR [ShowResultsBeforeVoting]
GO
ALTER TABLE [dbo].[PollConfigurations] ADD  DEFAULT (CONVERT([bit],(1))) FOR [ShowResultsBeforeEnd]
GO
ALTER TABLE [dbo].[PollConfigurations] ADD  DEFAULT (CONVERT([bit],(0))) FOR [AllowAddingOptions]
GO
ALTER TABLE [dbo].[PollConfigurations] ADD  DEFAULT ((2)) FOR [MinOptions]
GO
ALTER TABLE [dbo].[PollConfigurations] ADD  DEFAULT ((10)) FOR [MaxOptions]
GO
ALTER TABLE [dbo].[PollConfigurations] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[PollConfigurations] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[PollOptions] ADD  DEFAULT ((0)) FOR [DisplayOrder]
GO
ALTER TABLE [dbo].[PollOptions] ADD  DEFAULT ((0)) FOR [VoteCount]
GO
ALTER TABLE [dbo].[PollOptions] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[PollVotes] ADD  DEFAULT (getdate()) FOR [VotedAt]
GO
ALTER TABLE [dbo].[PostAwards] ADD  DEFAULT (getdate()) FOR [AwardedAt]
GO
ALTER TABLE [dbo].[PostAwards] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsAnonymous]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT (N'text') FOR [PostType]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT ((0)) FOR [UpvoteCount]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT ((0)) FOR [DownvoteCount]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT ((0)) FOR [CommentCount]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT ((0)) FOR [Score]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT (N'published') FOR [Status]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsPinned]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsLocked]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsNSFW]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsSpoiler]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT ((0)) FOR [ViewCount]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT (CONVERT([bit],(0))) FOR [HasPoll]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT ((0)) FOR [PollOptionCount]
GO
ALTER TABLE [dbo].[Posts] ADD  DEFAULT ((0)) FOR [PollVoteCount]
GO
ALTER TABLE [dbo].[PostVotes] ADD  DEFAULT (getdate()) FOR [VotedAt]
GO
ALTER TABLE [dbo].[SeoMetadata] ADD  DEFAULT (N'summary') FOR [TwitterCard]
GO
ALTER TABLE [dbo].[SeoMetadata] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[SeoMetadata] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Tags] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Tags] ADD  DEFAULT ((0)) FOR [PostCount]
GO
ALTER TABLE [dbo].[UserProfiles] ADD  DEFAULT (getdate()) FOR [JoinDate]
GO
ALTER TABLE [dbo].[UserProfiles] ADD  DEFAULT (getdate()) FOR [LastActive]
GO
ALTER TABLE [dbo].[UserProfiles] ADD  DEFAULT ((0)) FOR [KarmaPoints]
GO
ALTER TABLE [dbo].[UserProfiles] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsVerified]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[Categories]  WITH CHECK ADD  CONSTRAINT [FK_Categories_Categories_ParentCategoryId] FOREIGN KEY([ParentCategoryId])
REFERENCES [dbo].[Categories] ([CategoryId])
GO
ALTER TABLE [dbo].[Categories] CHECK CONSTRAINT [FK_Categories_Categories_ParentCategoryId]
GO
ALTER TABLE [dbo].[CommentAwards]  WITH CHECK ADD  CONSTRAINT [FK_CommentAwards_AspNetUsers_AwardedByUserId] FOREIGN KEY([AwardedByUserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[CommentAwards] CHECK CONSTRAINT [FK_CommentAwards_AspNetUsers_AwardedByUserId]
GO
ALTER TABLE [dbo].[CommentAwards]  WITH CHECK ADD  CONSTRAINT [FK_CommentAwards_Awards_AwardId] FOREIGN KEY([AwardId])
REFERENCES [dbo].[Awards] ([AwardId])
GO
ALTER TABLE [dbo].[CommentAwards] CHECK CONSTRAINT [FK_CommentAwards_Awards_AwardId]
GO
ALTER TABLE [dbo].[CommentAwards]  WITH CHECK ADD  CONSTRAINT [FK_CommentAwards_Comments_CommentId] FOREIGN KEY([CommentId])
REFERENCES [dbo].[Comments] ([CommentId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommentAwards] CHECK CONSTRAINT [FK_CommentAwards_Comments_CommentId]
GO
ALTER TABLE [dbo].[Comments]  WITH CHECK ADD  CONSTRAINT [FK_Comments_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Comments] CHECK CONSTRAINT [FK_Comments_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[Comments]  WITH CHECK ADD  CONSTRAINT [FK_Comments_Comments_ParentCommentId] FOREIGN KEY([ParentCommentId])
REFERENCES [dbo].[Comments] ([CommentId])
GO
ALTER TABLE [dbo].[Comments] CHECK CONSTRAINT [FK_Comments_Comments_ParentCommentId]
GO
ALTER TABLE [dbo].[Comments]  WITH CHECK ADD  CONSTRAINT [FK_Comments_Posts_PostId] FOREIGN KEY([PostId])
REFERENCES [dbo].[Posts] ([PostId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Comments] CHECK CONSTRAINT [FK_Comments_Posts_PostId]
GO
ALTER TABLE [dbo].[CommentVotes]  WITH CHECK ADD  CONSTRAINT [FK_CommentVotes_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommentVotes] CHECK CONSTRAINT [FK_CommentVotes_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[CommentVotes]  WITH CHECK ADD  CONSTRAINT [FK_CommentVotes_Comments_CommentId] FOREIGN KEY([CommentId])
REFERENCES [dbo].[Comments] ([CommentId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommentVotes] CHECK CONSTRAINT [FK_CommentVotes_Comments_CommentId]
GO
ALTER TABLE [dbo].[Communities]  WITH CHECK ADD  CONSTRAINT [FK_Communities_AspNetUsers_CreatorId] FOREIGN KEY([CreatorId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Communities] CHECK CONSTRAINT [FK_Communities_AspNetUsers_CreatorId]
GO
ALTER TABLE [dbo].[Communities]  WITH CHECK ADD  CONSTRAINT [FK_Communities_Categories_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Categories] ([CategoryId])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Communities] CHECK CONSTRAINT [FK_Communities_Categories_CategoryId]
GO
ALTER TABLE [dbo].[CommunityMembers]  WITH CHECK ADD  CONSTRAINT [FK_CommunityMembers_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommunityMembers] CHECK CONSTRAINT [FK_CommunityMembers_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[CommunityMembers]  WITH CHECK ADD  CONSTRAINT [FK_CommunityMembers_Communities_CommunityId] FOREIGN KEY([CommunityId])
REFERENCES [dbo].[Communities] ([CommunityId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CommunityMembers] CHECK CONSTRAINT [FK_CommunityMembers_Communities_CommunityId]
GO
ALTER TABLE [dbo].[Media]  WITH CHECK ADD  CONSTRAINT [FK_Media_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Media] CHECK CONSTRAINT [FK_Media_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[Media]  WITH CHECK ADD  CONSTRAINT [FK_Media_Posts_PostId] FOREIGN KEY([PostId])
REFERENCES [dbo].[Posts] ([PostId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Media] CHECK CONSTRAINT [FK_Media_Posts_PostId]
GO
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[PollConfigurations]  WITH CHECK ADD  CONSTRAINT [FK_PollConfigurations_ClosedByUser] FOREIGN KEY([ClosedByUserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[PollConfigurations] CHECK CONSTRAINT [FK_PollConfigurations_ClosedByUser]
GO
ALTER TABLE [dbo].[PollConfigurations]  WITH CHECK ADD  CONSTRAINT [FK_PollConfigurations_Posts_PostId] FOREIGN KEY([PostId])
REFERENCES [dbo].[Posts] ([PostId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PollConfigurations] CHECK CONSTRAINT [FK_PollConfigurations_Posts_PostId]
GO
ALTER TABLE [dbo].[PollOptions]  WITH CHECK ADD  CONSTRAINT [FK_PollOptions_Posts_PostId] FOREIGN KEY([PostId])
REFERENCES [dbo].[Posts] ([PostId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PollOptions] CHECK CONSTRAINT [FK_PollOptions_Posts_PostId]
GO
ALTER TABLE [dbo].[PollVotes]  WITH CHECK ADD  CONSTRAINT [FK_PollVotes_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PollVotes] CHECK CONSTRAINT [FK_PollVotes_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[PollVotes]  WITH CHECK ADD  CONSTRAINT [FK_PollVotes_PollOptions_PollOptionId] FOREIGN KEY([PollOptionId])
REFERENCES [dbo].[PollOptions] ([PollOptionId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PollVotes] CHECK CONSTRAINT [FK_PollVotes_PollOptions_PollOptionId]
GO
ALTER TABLE [dbo].[PostAwards]  WITH CHECK ADD  CONSTRAINT [FK_PostAwards_AspNetUsers_AwardedByUserId] FOREIGN KEY([AwardedByUserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[PostAwards] CHECK CONSTRAINT [FK_PostAwards_AspNetUsers_AwardedByUserId]
GO
ALTER TABLE [dbo].[PostAwards]  WITH CHECK ADD  CONSTRAINT [FK_PostAwards_Awards_AwardId] FOREIGN KEY([AwardId])
REFERENCES [dbo].[Awards] ([AwardId])
GO
ALTER TABLE [dbo].[PostAwards] CHECK CONSTRAINT [FK_PostAwards_Awards_AwardId]
GO
ALTER TABLE [dbo].[PostAwards]  WITH CHECK ADD  CONSTRAINT [FK_PostAwards_Posts_PostId] FOREIGN KEY([PostId])
REFERENCES [dbo].[Posts] ([PostId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PostAwards] CHECK CONSTRAINT [FK_PostAwards_Posts_PostId]
GO
ALTER TABLE [dbo].[Posts]  WITH CHECK ADD  CONSTRAINT [FK_Posts_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Posts] CHECK CONSTRAINT [FK_Posts_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[Posts]  WITH CHECK ADD  CONSTRAINT [FK_Posts_Communities_CommunityId] FOREIGN KEY([CommunityId])
REFERENCES [dbo].[Communities] ([CommunityId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Posts] CHECK CONSTRAINT [FK_Posts_Communities_CommunityId]
GO
ALTER TABLE [dbo].[PostTags]  WITH CHECK ADD  CONSTRAINT [FK_PostTags_Posts_PostId] FOREIGN KEY([PostId])
REFERENCES [dbo].[Posts] ([PostId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PostTags] CHECK CONSTRAINT [FK_PostTags_Posts_PostId]
GO
ALTER TABLE [dbo].[PostTags]  WITH CHECK ADD  CONSTRAINT [FK_PostTags_Tags_TagId] FOREIGN KEY([TagId])
REFERENCES [dbo].[Tags] ([TagId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PostTags] CHECK CONSTRAINT [FK_PostTags_Tags_TagId]
GO
ALTER TABLE [dbo].[PostVotes]  WITH CHECK ADD  CONSTRAINT [FK_PostVotes_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PostVotes] CHECK CONSTRAINT [FK_PostVotes_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[PostVotes]  WITH CHECK ADD  CONSTRAINT [FK_PostVotes_Posts_PostId] FOREIGN KEY([PostId])
REFERENCES [dbo].[Posts] ([PostId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PostVotes] CHECK CONSTRAINT [FK_PostVotes_Posts_PostId]
GO
ALTER TABLE [dbo].[SavedPosts]  WITH CHECK ADD  CONSTRAINT [FK_SavedPosts_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SavedPosts] CHECK CONSTRAINT [FK_SavedPosts_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[SavedPosts]  WITH CHECK ADD  CONSTRAINT [FK_SavedPosts_Posts_PostId] FOREIGN KEY([PostId])
REFERENCES [dbo].[Posts] ([PostId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SavedPosts] CHECK CONSTRAINT [FK_SavedPosts_Posts_PostId]
GO
ALTER TABLE [dbo].[UserProfiles]  WITH CHECK ADD  CONSTRAINT [FK_UserProfiles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserProfiles] CHECK CONSTRAINT [FK_UserProfiles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[CommentVotes]  WITH CHECK ADD  CONSTRAINT [CK_CommentVote_Type] CHECK  (([VoteType]=(1) OR [VoteType]=(-1)))
GO
ALTER TABLE [dbo].[CommentVotes] CHECK CONSTRAINT [CK_CommentVote_Type]
GO
ALTER TABLE [dbo].[Communities]  WITH CHECK ADD  CONSTRAINT [CK_Community_Type] CHECK  (([CommunityType]='restricted' OR [CommunityType]='private' OR [CommunityType]='public'))
GO
ALTER TABLE [dbo].[Communities] CHECK CONSTRAINT [CK_Community_Type]
GO
ALTER TABLE [dbo].[CommunityMembers]  WITH CHECK ADD  CONSTRAINT [CK_CommunityMember_NotificationPreference] CHECK  (([NotificationPreference]='none' OR [NotificationPreference]='important' OR [NotificationPreference]='all'))
GO
ALTER TABLE [dbo].[CommunityMembers] CHECK CONSTRAINT [CK_CommunityMember_NotificationPreference]
GO
ALTER TABLE [dbo].[CommunityMembers]  WITH CHECK ADD  CONSTRAINT [CK_CommunityMember_Role] CHECK  (([Role]='admin' OR [Role]='moderator' OR [Role]='member'))
GO
ALTER TABLE [dbo].[CommunityMembers] CHECK CONSTRAINT [CK_CommunityMember_Role]
GO
ALTER TABLE [dbo].[Media]  WITH CHECK ADD  CONSTRAINT [CK_Media_Type] CHECK  (([MediaType]='audio' OR [MediaType]='document' OR [MediaType]='video' OR [MediaType]='image'))
GO
ALTER TABLE [dbo].[Media] CHECK CONSTRAINT [CK_Media_Type]
GO
ALTER TABLE [dbo].[PollConfigurations]  WITH CHECK ADD  CONSTRAINT [CK_PollConfiguration_MaxOptions] CHECK  (([MaxOptions]>=[MinOptions]))
GO
ALTER TABLE [dbo].[PollConfigurations] CHECK CONSTRAINT [CK_PollConfiguration_MaxOptions]
GO
ALTER TABLE [dbo].[PollConfigurations]  WITH CHECK ADD  CONSTRAINT [CK_PollConfiguration_MinOptions] CHECK  (([MinOptions]>=(2)))
GO
ALTER TABLE [dbo].[PollConfigurations] CHECK CONSTRAINT [CK_PollConfiguration_MinOptions]
GO
ALTER TABLE [dbo].[Posts]  WITH CHECK ADD  CONSTRAINT [CK_Post_Status] CHECK  (([Status]='archived' OR [Status]='deleted' OR [Status]='removed' OR [Status]='published'))
GO
ALTER TABLE [dbo].[Posts] CHECK CONSTRAINT [CK_Post_Status]
GO
ALTER TABLE [dbo].[Posts]  WITH CHECK ADD  CONSTRAINT [CK_Post_Type] CHECK  (([PostType]='poll' OR [PostType]='video' OR [PostType]='image' OR [PostType]='link' OR [PostType]='text'))
GO
ALTER TABLE [dbo].[Posts] CHECK CONSTRAINT [CK_Post_Type]
GO
ALTER TABLE [dbo].[PostVotes]  WITH CHECK ADD  CONSTRAINT [CK_PostVote_Type] CHECK  (([VoteType]=(1) OR [VoteType]=(-1)))
GO
ALTER TABLE [dbo].[PostVotes] CHECK CONSTRAINT [CK_PostVote_Type]
GO
ALTER TABLE [dbo].[SeoMetadata]  WITH CHECK ADD  CONSTRAINT [CK_SeoMetadata_EntityType] CHECK  (([EntityType]='post' OR [EntityType]='community'))
GO
ALTER TABLE [dbo].[SeoMetadata] CHECK CONSTRAINT [CK_SeoMetadata_EntityType]
GO
/****** Object:  StoredProcedure [dbo].[sp_CreatePoll]    Script Date: 10/4/2025 3:27:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

    CREATE PROCEDURE [dbo].[sp_CreatePoll]
        @PostId INT,
        @PollQuestion NVARCHAR(500),
        @PollDescription NVARCHAR(1000) = NULL,
        @AllowMultipleChoices BIT = 0,
        @MaxSelections INT = 1,
        @EndDate DATETIME2 = NULL,
        @VoteVisibility NVARCHAR(20) = 'public',
        @AllowChangingVote BIT = 1
    AS
    BEGIN
        INSERT INTO PollConfiguration (
            PostId, PollQuestion, PollDescription, AllowMultipleChoices, 
            MaxSelections, EndDate, VoteVisibility, AllowChangingVote
        )
        VALUES (
            @PostId, @PollQuestion, @PollDescription, @AllowMultipleChoices,
            @MaxSelections, @EndDate, @VoteVisibility, @AllowChangingVote
        )
    END
    
GO
USE [master]
GO
ALTER DATABASE [DiscussionspotADO] SET  READ_WRITE 
GO
