USE [gsmsharingv2]
GO
/****** Object:  Schema [gsmsharing]    Script Date: 06/12/2025 5:27:20 pm ******/
CREATE SCHEMA [gsmsharing]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 06/12/2025 5:27:20 pm ******/
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
/****** Object:  Table [dbo].[MobilePosts]    Script Date: 06/12/2025 5:27:20 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MobilePosts](
	[BlogId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[FileId] [varchar](255) NULL,
	[Title] [nvarchar](550) NULL,
	[Content] [nvarchar](max) NULL,
	[Tags] [nvarchar](max) NULL,
	[MetaDis] [nvarchar](500) NULL,
	[FileSize] [nvarchar](255) NULL,
	[FileName] [nvarchar](255) NULL,
	[WebLinks] [nvarchar](355) NULL,
	[views] [int] NULL,
	[likes] [int] NULL,
	[dislikes] [int] NULL,
	[publish] [tinyint] NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK__MobilePo__54379E302C94E4A7] PRIMARY KEY CLUSTERED 
(
	[BlogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[GetAllPosts]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[GetAllPosts]
AS
SELECT dbo.AspNetUsers.LoginName, dbo.MobilePosts.BlogId, dbo.MobilePosts.UserId, dbo.MobilePosts.FileId, dbo.MobilePosts.Title, dbo.MobilePosts.[Content], dbo.MobilePosts.Tags, dbo.MobilePosts.MetaDis, dbo.MobilePosts.FileSize, 
                  dbo.MobilePosts.FileName, dbo.MobilePosts.WebLinks, dbo.MobilePosts.views, dbo.MobilePosts.likes, dbo.MobilePosts.dislikes, dbo.MobilePosts.publish, dbo.MobilePosts.CreationDate, dbo.MobilePosts.BlogId AS Expr1, 
                  dbo.MobilePosts.UserId AS Expr2, dbo.MobilePosts.Title AS Expr3, dbo.MobilePosts.views AS Expr4, dbo.MobilePosts.likes AS Expr5, dbo.MobilePosts.dislikes AS Expr6, dbo.MobilePosts.CreationDate AS Expr7
FROM     dbo.MobilePosts INNER JOIN
                  dbo.AspNetUsers ON dbo.MobilePosts.UserId = dbo.AspNetUsers.Id
WHERE  (dbo.MobilePosts.publish = 1)
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 06/12/2025 5:27:21 pm ******/
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
/****** Object:  Table [dbo].[AdCategory]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CategoryName] [varchar](255) NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK_AdCategory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AdPostCat]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdPostCat](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AdsId] [int] NULL,
	[CatId] [int] NULL,
 CONSTRAINT [PK_AdPostCat] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AdsImage]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdsImage](
	[SalePicId] [int] IDENTITY(1,1) NOT NULL,
	[AdsId] [int] NULL,
	[Pics] [varbinary](max) NULL,
	[ImageDate] [datetime] NULL,
 CONSTRAINT [PK_AdsImage] PRIMARY KEY CLUSTERED 
(
	[SalePicId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AdSubCat]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdSubCat](
	[SubId] [int] IDENTITY(1,1) NOT NULL,
	[MainCatId] [int] NULL,
	[CategoryName] [varchar](255) NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK_AdSubCat] PRIMARY KEY CLUSTERED 
(
	[SubId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AffiliationProgram]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AffiliationProgram](
	[ProductId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[Title] [nvarchar](max) NULL,
	[ProductDiscription] [nvarchar](max) NULL,
	[Keywords] [nvarchar](max) NULL,
	[Content] [nvarchar](max) NULL,
	[Views] [int] NULL,
	[Likes] [int] NULL,
	[DisLikes] [int] NULL,
	[CreationDate] [datetime] NULL,
	[Price] [int] NULL,
	[ImageLink] [nvarchar](max) NULL,
	[BuyLink] [nvarchar](255) NULL,
 CONSTRAINT [PK_AffiliationProgram] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AmazonProducts]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AmazonProducts](
	[product_id] [int] IDENTITY(1,1) NOT NULL,
	[asin] [varchar](20) NULL,
	[title] [varchar](255) NULL,
	[current_price] [decimal](10, 2) NULL,
	[original_price] [decimal](10, 2) NULL,
	[max_price] [decimal](10, 2) NULL,
	[currency] [varchar](5) NULL,
	[country] [varchar](5) NULL,
	[star_rating] [decimal](2, 1) NULL,
	[num_ratings] [int] NULL,
	[url] [varchar](255) NULL,
	[main_photo_url] [varchar](255) NULL,
	[num_offers] [int] NULL,
	[availability] [varchar](50) NULL,
	[is_best_seller] [bit] NULL,
	[is_amazon_choice] [bit] NULL,
	[climate_pledge_friendly] [bit] NULL,
	[sales_volume] [varchar](50) NULL,
	[is_prime] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[product_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[asin] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 06/12/2025 5:27:21 pm ******/
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
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 06/12/2025 5:27:21 pm ******/
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
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 06/12/2025 5:27:21 pm ******/
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
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 06/12/2025 5:27:21 pm ******/
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
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 06/12/2025 5:27:21 pm ******/
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
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 06/12/2025 5:27:21 pm ******/
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
/****** Object:  Table [dbo].[BlogCatContainer]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BlogCatContainer](
	[CatBlogId] [int] IDENTITY(1,1) NOT NULL,
	[CatId] [int] NULL,
	[SubCatId] [int] NULL,
	[BlogId] [int] NULL,
 CONSTRAINT [PK_BlogCatContainer] PRIMARY KEY CLUSTERED 
(
	[CatBlogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BlogComments]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BlogComments](
	[Commentid] [int] IDENTITY(1,1) NOT NULL,
	[BlogId] [int] NULL,
	[UserId] [nvarchar](450) NULL,
	[Comment] [varchar](max) NULL,
	[CreationDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Commentid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BlogFolder]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BlogFolder](
	[FID] [int] IDENTITY(0,1) NOT NULL,
	[BlogId] [int] NULL,
	[Folderid] [varchar](255) NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK__BlogFold__C1BEA5A29E5AE303] PRIMARY KEY CLUSTERED 
(
	[FID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BlogSEO]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BlogSEO](
	[SEOID] [int] IDENTITY(1,1) NOT NULL,
	[BlogId] [int] NULL,
	[BlogDiscription] [nvarchar](max) NULL,
	[BlogKeywords] [nvarchar](max) NULL,
	[canonical] [varchar](50) NULL,
 CONSTRAINT [PK_BlogSEO] PRIMARY KEY CLUSTERED 
(
	[SEOID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BlogSpecContainer]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BlogSpecContainer](
	[ContainerId] [int] IDENTITY(1,1) NOT NULL,
	[BlogId] [int] NULL,
	[SpecId] [int] NULL,
 CONSTRAINT [PK_BlogSpecContainer] PRIMARY KEY CLUSTERED 
(
	[ContainerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FileMenu]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FileMenu](
	[MenuId] [bigint] IDENTITY(1,1) NOT NULL,
	[MenuName] [varchar](50) NULL,
	[DriveId] [varchar](150) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ForumCategory]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ForumCategory](
	[CategoryId] [int] IDENTITY(0,1) NOT NULL,
	[UserFourmID] [int] NULL,
	[CategoryName] [varchar](255) NULL,
	[Parantid] [int] NULL,
	[creationDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ForumReplys]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ForumReplys](
	[Id] [int] NOT NULL,
	[ThreadId] [int] NULL,
	[UserId] [nvarchar](450) NULL,
	[ForumContent] [nvarchar](max) NULL,
	[Like] [int] NULL,
	[DisLike] [int] NULL,
	[Views] [int] NULL,
	[PublishDate] [datetime] NULL,
 CONSTRAINT [PK_ForumReplys] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FourmComments]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FourmComments](
	[Commentid] [int] IDENTITY(0,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[Comment] [varchar](max) NULL,
	[CreationDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Commentid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GsmBlog]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GsmBlog](
	[BlogId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[BlogTitle] [nvarchar](500) NULL,
	[BlogDiscription] [nvarchar](max) NULL,
	[BlogKeywords] [nvarchar](max) NULL,
	[BlogContent] [nvarchar](max) NULL,
	[BlogViews] [int] NULL,
	[BlogLikes] [int] NULL,
	[BlogDisLikes] [int] NULL,
	[Publish] [bit] NULL,
	[PublishDate] [datetime] NULL,
	[CategoryId] [int] NULL,
	[ThumbNailLink] [nvarchar](255) NULL,
 CONSTRAINT [PK_GsmBlog] PRIMARY KEY CLUSTERED 
(
	[BlogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GsmBlogCategory]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GsmBlogCategory](
	[CategoryId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ParantId] [int] NULL,
 CONSTRAINT [PK_GsmBlogCategory] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GsmBlogComments]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GsmBlogComments](
	[CommentId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[BlogId] [int] NULL,
	[Comment] [nvarchar](max) NULL,
	[publishDate] [datetime] NULL,
 CONSTRAINT [PK_GsmBlogComments] PRIMARY KEY CLUSTERED 
(
	[CommentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[keywords]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[keywords](
	[KeyId] [int] IDENTITY(0,1) NOT NULL,
	[KeywordStudy] [varchar](355) NULL,
	[KeywordLawyers] [varchar](355) NULL,
	[keywordInsurance] [varchar](355) NULL,
	[KeywordProperty] [varchar](355) NULL,
	[keywordMobile] [varchar](355) NULL,
	[KeywordMedical] [varchar](355) NULL,
PRIMARY KEY CLUSTERED 
(
	[KeyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MobileAds]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MobileAds](
	[AdsId] [int] IDENTITY(0,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[Title] [varchar](550) NULL,
	[Discription] [varchar](max) NULL,
	[price] [int] NULL,
	[Tags] [varchar](max) NULL,
	[views] [int] NULL,
	[likes] [int] NULL,
	[dislikes] [int] NULL,
	[publish] [tinyint] NULL,
	[CreationDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[AdsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MobileCategory]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MobileCategory](
	[CategoryId] [int] IDENTITY(0,1) NOT NULL,
	[CategoryName] [varchar](255) NULL,
	[Parantid] [int] NULL,
	[creationDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MobileFiles]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MobileFiles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FileId] [nvarchar](255) NULL,
	[FolderName] [nvarchar](255) NULL,
 CONSTRAINT [PK_MobileFiles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MobilePartAds]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MobilePartAds](
	[MobileAdsId] [int] IDENTITY(0,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[Title] [varchar](550) NULL,
	[Discription] [varchar](max) NULL,
	[Tags] [varchar](max) NULL,
	[price] [int] NULL,
	[views] [int] NULL,
	[likes] [int] NULL,
	[dislikes] [int] NULL,
	[publish] [tinyint] NULL,
	[CreationDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[MobileAdsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MobileSale]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MobileSale](
	[Mobileid] [int] NULL,
	[UserId] [nvarchar](450) NULL,
	[Mobile_Model] [nvarchar](455) NULL,
	[Ram] [nvarchar](50) NULL,
	[Dispaly] [nvarchar](50) NULL,
	[Processor] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MobileSpecs]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MobileSpecs](
	[Specid] [int] IDENTITY(0,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[ModelName] [varchar](255) NULL,
	[NetworkInfo] [varchar](255) NULL,
	[Launched] [datetime] NULL,
	[Body] [varchar](255) NULL,
	[Display] [varchar](255) NULL,
	[OS] [varchar](255) NULL,
	[Processor] [varchar](255) NULL,
	[Memory] [varchar](255) NULL,
	[MainCamera] [varchar](255) NULL,
	[SelfiCam] [varchar](255) NULL,
	[Sounds] [varchar](255) NULL,
	[Common] [varchar](255) NULL,
	[Sensors] [varchar](255) NULL,
	[Battery] [varchar](255) NULL,
	[Price] [varchar](55) NULL,
	[MetaInfo] [nvarchar](max) NULL,
	[Tags] [nvarchar](max) NULL,
 CONSTRAINT [PK__MobileSp__8804B653D8243E75] PRIMARY KEY CLUSTERED 
(
	[Specid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MobileSubCategory]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MobileSubCategory](
	[SubCategoryId] [int] IDENTITY(0,1) NOT NULL,
	[CategoryId] [int] NULL,
	[CategoryName] [varchar](255) NULL,
	[Parantid] [int] NULL,
	[creationDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[SubCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductImage]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductImage](
	[PicId] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NULL,
	[ProductPic] [varbinary](max) NULL,
	[UploadDate] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductReview]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductReview](
	[RId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[BlogId] [int] NULL,
	[Review] [nvarchar](max) NULL,
	[ReviewDate] [datetime] NULL,
 CONSTRAINT [PK_ProductReview] PRIMARY KEY CLUSTERED 
(
	[RId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[profile]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[profile](
	[ProfileId] [bigint] IDENTITY(0,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[ProfilePhoto] [varbinary](max) NULL,
	[uploadDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProfilePhoto]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfilePhoto](
	[PhotoId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[Photo] [varbinary](max) NULL,
	[PhotoDate] [datetime] NULL,
 CONSTRAINT [PK_ProfilePhoto] PRIMARY KEY CLUSTERED 
(
	[PhotoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[rating_distribution]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[rating_distribution](
	[rating_id] [int] IDENTITY(1,1) NOT NULL,
	[product_id] [int] NULL,
	[star_rating] [tinyint] NULL,
	[num_ratings] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[rating_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Review]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Review](
	[ReviewId] [varchar](20) NOT NULL,
	[ReviewTitle] [varchar](255) NULL,
	[ReviewComment] [text] NULL,
	[ReviewStarRating] [int] NULL,
	[ReviewLink] [varchar](255) NULL,
	[ReviewAuthor] [varchar](255) NULL,
	[ReviewAuthorAvatar] [varchar](255) NULL,
	[ReviewDate] [datetime] NULL,
	[IsVerifiedPurchase] [bit] NULL,
	[HelpfulVoteStatement] [varchar](255) NULL,
	[ProductAsin] [varchar](10) NULL,
PRIMARY KEY CLUSTERED 
(
	[ReviewId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[review_aspects]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[review_aspects](
	[aspect_id] [int] IDENTITY(1,1) NOT NULL,
	[product_id] [int] NULL,
	[aspect_name] [varchar](100) NULL,
	[sentiment] [varchar](10) NULL,
PRIMARY KEY CLUSTERED 
(
	[aspect_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReviewImage]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReviewImage](
	[ReviewImageId] [int] IDENTITY(1,1) NOT NULL,
	[ReviewId] [varchar](20) NULL,
	[ImageUrl] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ReviewImageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReviewProduct]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReviewProduct](
	[Asin] [varchar](10) NOT NULL,
	[TotalReviews] [int] NULL,
	[TotalRatings] [int] NULL,
	[Country] [varchar](2) NULL,
	[Domain] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Asin] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SocialCategories]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SocialCategories](
	[CategoryId] [int] IDENTITY(1,1) NOT NULL,
	[CategoryName] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[IconClass] [nvarchar](100) NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SocialCommunities]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SocialCommunities](
	[CommunityId] [int] IDENTITY(1,1) NOT NULL,
	[CommunityName] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CategoryId] [int] NULL,
	[CreatedBy] [nvarchar](450) NOT NULL,
	[IconClass] [nvarchar](100) NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[CommunityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users_Communities]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users_Communities](
	[CommunityId] [int] NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[JoinedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[CommunityId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UsersFourm]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UsersFourm](
	[UserFourmID] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NULL,
	[Title] [varchar](550) NULL,
	[Content] [nvarchar](max) NULL,
	[Tags] [varchar](max) NULL,
	[MetaDiscription] [varchar](600) NULL,
	[views] [int] NULL,
	[likes] [int] NULL,
	[dislikes] [int] NULL,
	[ParantId] [int] NULL,
	[publish] [tinyint] NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK__UsersFou__938B20B1D6BDE15F] PRIMARY KEY CLUSTERED 
(
	[UserFourmID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[blog_files]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[blog_files](
	[filesId] [bigint] IDENTITY(1,1) NOT NULL,
	[Blog_ID] [bigint] NULL,
	[FileName] [nvarchar](255) NULL,
	[FileSize] [nvarchar](45) NULL,
	[Link] [nvarchar](255) NULL,
	[UploadDate] [datetime2](0) NULL,
 CONSTRAINT [PK_blog_files_filesId] PRIMARY KEY CLUSTERED 
(
	[filesId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[blogcat]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[blogcat](
	[containerid] [bigint] IDENTITY(132,1) NOT NULL,
	[Blog_ID] [int] NULL,
	[catId] [bigint] NULL,
	[SubCat] [bigint] NULL,
	[catDate] [datetime2](0) NULL,
 CONSTRAINT [PK_blogcat_containerid] PRIMARY KEY CLUSTERED 
(
	[containerid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[blogfolder]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[blogfolder](
	[fid] [int] IDENTITY(19,1) NOT NULL,
	[Blog_ID] [int] NULL,
	[folderid] [nvarchar](255) NULL,
	[date] [datetime2](0) NULL,
 CONSTRAINT [PK_blogfolder_fid] PRIMARY KEY CLUSTERED 
(
	[fid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[blogposts]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[blogposts](
	[Blog_ID] [int] IDENTITY(128,1) NOT NULL,
	[UserID] [int] NULL,
	[FileId] [nvarchar](45) NULL,
	[Title] [nvarchar](max) NULL,
	[Content] [nvarchar](max) NULL,
	[FileName] [nvarchar](255) NULL,
	[views] [int] NULL,
	[likes] [int] NULL,
	[dislike] [int] NULL,
	[size] [nvarchar](255) NULL,
	[weblink] [nvarchar](255) NULL,
	[tags] [nvarchar](max) NULL,
	[creationDate] [datetime2](0) NULL,
	[publish] [int] NULL,
 CONSTRAINT [PK_blogposts_Blog_ID] PRIMARY KEY CLUSTERED 
(
	[Blog_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[category]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[category](
	[catid] [bigint] IDENTITY(40,1) NOT NULL,
	[catName] [nvarchar](255) NOT NULL,
	[parantid] [bigint] NULL,
	[creationDate] [datetime2](0) NULL,
 CONSTRAINT [PK_category_catid] PRIMARY KEY CLUSTERED 
(
	[catid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[categoryforum]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[categoryforum](
	[catid] [bigint] NOT NULL,
	[forumid] [bigint] NULL,
	[catName] [nvarchar](255) NULL,
	[parantID] [bigint] NULL,
	[creationDate] [datetime2](0) NULL,
	[categoryforumcol] [nvarchar](45) NULL,
 CONSTRAINT [PK_categoryforum_catid] PRIMARY KEY CLUSTERED 
(
	[catid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[code]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[code](
	[idcode] [int] IDENTITY(9,1) NOT NULL,
	[UserId] [int] NULL,
	[title] [nvarchar](500) NULL,
	[content] [nvarchar](max) NULL,
	[Tags] [nvarchar](max) NULL,
	[likes] [int] NULL,
	[dislike] [int] NULL,
	[views] [int] NULL,
	[published] [int] NULL,
	[publishdate] [datetime2](0) NULL,
 CONSTRAINT [PK_code_idcode] PRIMARY KEY CLUSTERED 
(
	[idcode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[codecategory]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[codecategory](
	[catid] [int] IDENTITY(20,1) NOT NULL,
	[CatName] [nvarchar](255) NULL,
	[PublishDate] [datetime] NULL,
 CONSTRAINT [PK_codecategory_catid] PRIMARY KEY CLUSTERED 
(
	[catid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[codecombine]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[codecombine](
	[id] [int] IDENTITY(7,1) NOT NULL,
	[codeId] [int] NULL,
	[catid] [int] NULL,
	[SubId] [int] NULL,
 CONSTRAINT [PK_codecombine_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[codecomments]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[codecomments](
	[commentid] [int] IDENTITY(3,1) NOT NULL,
	[Userid] [int] NULL,
	[codeid] [int] NULL,
	[comment] [nvarchar](max) NULL,
	[parantid] [int] NULL,
	[publishDate] [datetime] NULL,
 CONSTRAINT [PK_codecomments_commentid] PRIMARY KEY CLUSTERED 
(
	[commentid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[codesubcat]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[codesubcat](
	[SubId] [int] IDENTITY(5,1) NOT NULL,
	[Main_Id] [int] NOT NULL,
	[SubName] [nvarchar](255) NOT NULL,
	[PublishDate] [datetime] NULL,
 CONSTRAINT [PK_codesubcat_SubId] PRIMARY KEY CLUSTERED 
(
	[SubId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[comments]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[comments](
	[commentid] [bigint] IDENTITY(5,1) NOT NULL,
	[UserID] [int] NULL,
	[BlogID] [bigint] NOT NULL,
	[comment] [nvarchar](max) NULL,
	[parantId] [int] NULL,
	[commentDate] [datetime2](0) NULL,
 CONSTRAINT [PK_comments_commentid] PRIMARY KEY CLUSTERED 
(
	[commentid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[gsmlog]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[gsmlog](
	[logid] [bigint] IDENTITY(86841,1) NOT NULL,
	[ipAddress] [nvarchar](45) NULL,
	[hostName] [nvarchar](45) NULL,
	[visitTime] [datetime2](0) NULL,
 CONSTRAINT [PK_gsmlog_logid] PRIMARY KEY CLUSTERED 
(
	[logid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[news]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[news](
	[newsid] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[title] [nvarchar](255) NULL,
	[detail] [nvarchar](max) NULL,
	[newsdate] [datetime2](0) NULL,
 CONSTRAINT [PK_news_newsid] PRIMARY KEY CLUSTERED 
(
	[newsid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[subcat]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[subcat](
	[subid] [bigint] IDENTITY(46,1) NOT NULL,
	[catid] [bigint] NULL,
	[subName] [nvarchar](255) NULL,
	[subDate] [datetime2](0) NULL,
 CONSTRAINT [PK_subcat_subid] PRIMARY KEY CLUSTERED 
(
	[subid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[user]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[user](
	[UserID] [int] IDENTITY(161,1) NOT NULL,
	[UserName] [nvarchar](55) NOT NULL,
	[Email] [nvarchar](45) NOT NULL,
	[Password] [nvarchar](255) NOT NULL,
	[admin] [int] NULL,
	[CreationDate] [datetime2](0) NULL,
 CONSTRAINT [PK_user_UserID] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [user$Email] UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [gsmsharing].[userforum]    Script Date: 06/12/2025 5:27:21 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [gsmsharing].[userforum](
	[forumid] [bigint] IDENTITY(20,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[forumTitle] [nvarchar](255) NULL,
	[forumContent] [nvarchar](max) NULL,
	[tags] [nvarchar](max) NULL,
	[parantid] [bigint] NULL,
	[voteup] [int] NULL,
	[votedown] [int] NULL,
	[forumDate] [datetime2](0) NULL,
 CONSTRAINT [PK_userforum_forumid] PRIMARY KEY CLUSTERED 
(
	[forumid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[AdCategory] ADD  CONSTRAINT [DF_AdCategory_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[AdSubCat] ADD  CONSTRAINT [DF_AdSubCat_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[AffiliationProgram] ADD  CONSTRAINT [DF_AffiliationProgram_Views]  DEFAULT ((0)) FOR [Views]
GO
ALTER TABLE [dbo].[AffiliationProgram] ADD  CONSTRAINT [DF_AffiliationProgram_Likes]  DEFAULT ((0)) FOR [Likes]
GO
ALTER TABLE [dbo].[AffiliationProgram] ADD  CONSTRAINT [DF_AffiliationProgram_DisLikes]  DEFAULT ((0)) FOR [DisLikes]
GO
ALTER TABLE [dbo].[AffiliationProgram] ADD  CONSTRAINT [DF_AffiliationProgram_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[BlogComments] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[BlogFolder] ADD  CONSTRAINT [DF__BlogFolde__Creat__6C190EBB]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[ForumCategory] ADD  DEFAULT ((0)) FOR [Parantid]
GO
ALTER TABLE [dbo].[ForumCategory] ADD  DEFAULT (getdate()) FOR [creationDate]
GO
ALTER TABLE [dbo].[ForumReplys] ADD  CONSTRAINT [DF_ForumReplys_PublishDate]  DEFAULT (getdate()) FOR [PublishDate]
GO
ALTER TABLE [dbo].[FourmComments] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[GsmBlog] ADD  CONSTRAINT [DF_GsmBlog_BlogViews]  DEFAULT ((0)) FOR [BlogViews]
GO
ALTER TABLE [dbo].[GsmBlog] ADD  CONSTRAINT [DF_GsmBlog_BlogLikes]  DEFAULT ((0)) FOR [BlogLikes]
GO
ALTER TABLE [dbo].[GsmBlog] ADD  CONSTRAINT [DF_GsmBlog_BlogDisLikes]  DEFAULT ((0)) FOR [BlogDisLikes]
GO
ALTER TABLE [dbo].[GsmBlog] ADD  CONSTRAINT [DF_GsmBlog_Publish]  DEFAULT ((0)) FOR [Publish]
GO
ALTER TABLE [dbo].[GsmBlog] ADD  CONSTRAINT [DF_GsmBlog_PublishDate]  DEFAULT (getdate()) FOR [PublishDate]
GO
ALTER TABLE [dbo].[GsmBlog] ADD  CONSTRAINT [DF_GsmBlog_ThumbNailLink]  DEFAULT (N'Loading......') FOR [ThumbNailLink]
GO
ALTER TABLE [dbo].[GsmBlogCategory] ADD  CONSTRAINT [DF_GsmBlogCategory_ParantId]  DEFAULT ((0)) FOR [ParantId]
GO
ALTER TABLE [dbo].[GsmBlogComments] ADD  CONSTRAINT [DF_GsmBlogComments_publishDate]  DEFAULT (getdate()) FOR [publishDate]
GO
ALTER TABLE [dbo].[MobileAds] ADD  DEFAULT ((0)) FOR [views]
GO
ALTER TABLE [dbo].[MobileAds] ADD  DEFAULT ((0)) FOR [likes]
GO
ALTER TABLE [dbo].[MobileAds] ADD  DEFAULT ((0)) FOR [dislikes]
GO
ALTER TABLE [dbo].[MobileAds] ADD  DEFAULT ((0)) FOR [publish]
GO
ALTER TABLE [dbo].[MobileAds] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[MobileCategory] ADD  DEFAULT ((0)) FOR [Parantid]
GO
ALTER TABLE [dbo].[MobileCategory] ADD  DEFAULT (getdate()) FOR [creationDate]
GO
ALTER TABLE [dbo].[MobilePartAds] ADD  DEFAULT ((0)) FOR [views]
GO
ALTER TABLE [dbo].[MobilePartAds] ADD  DEFAULT ((0)) FOR [likes]
GO
ALTER TABLE [dbo].[MobilePartAds] ADD  DEFAULT ((0)) FOR [dislikes]
GO
ALTER TABLE [dbo].[MobilePartAds] ADD  DEFAULT ((0)) FOR [publish]
GO
ALTER TABLE [dbo].[MobilePartAds] ADD  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[MobilePosts] ADD  CONSTRAINT [DF__MobilePos__views__6383C8BA]  DEFAULT ((0)) FOR [views]
GO
ALTER TABLE [dbo].[MobilePosts] ADD  CONSTRAINT [DF__MobilePos__likes__6477ECF3]  DEFAULT ((0)) FOR [likes]
GO
ALTER TABLE [dbo].[MobilePosts] ADD  CONSTRAINT [DF__MobilePos__disli__656C112C]  DEFAULT ((0)) FOR [dislikes]
GO
ALTER TABLE [dbo].[MobilePosts] ADD  CONSTRAINT [DF__MobilePos__publi__66603565]  DEFAULT ((0)) FOR [publish]
GO
ALTER TABLE [dbo].[MobilePosts] ADD  CONSTRAINT [DF__MobilePos__Creat__6754599E]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[MobileSpecs] ADD  CONSTRAINT [DF_MobileSpecs_Launched]  DEFAULT (getdate()) FOR [Launched]
GO
ALTER TABLE [dbo].[MobileSubCategory] ADD  DEFAULT ((0)) FOR [Parantid]
GO
ALTER TABLE [dbo].[MobileSubCategory] ADD  DEFAULT (getdate()) FOR [creationDate]
GO
ALTER TABLE [dbo].[ProductImage] ADD  CONSTRAINT [DF_ProductImage_UploadDate]  DEFAULT (getdate()) FOR [UploadDate]
GO
ALTER TABLE [dbo].[ProductReview] ADD  CONSTRAINT [DF_ProductReview_ReviewDate]  DEFAULT (getdate()) FOR [ReviewDate]
GO
ALTER TABLE [dbo].[profile] ADD  DEFAULT (getdate()) FOR [uploadDate]
GO
ALTER TABLE [dbo].[ProfilePhoto] ADD  CONSTRAINT [DF_ProfilePhoto_PhotoDate]  DEFAULT (getdate()) FOR [PhotoDate]
GO
ALTER TABLE [dbo].[SocialCategories] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[SocialCommunities] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Users_Communities] ADD  DEFAULT (getdate()) FOR [JoinedAt]
GO
ALTER TABLE [dbo].[UsersFourm] ADD  CONSTRAINT [DF__UsersFour__views__7D439ABD]  DEFAULT ((0)) FOR [views]
GO
ALTER TABLE [dbo].[UsersFourm] ADD  CONSTRAINT [DF__UsersFour__likes__7E37BEF6]  DEFAULT ((0)) FOR [likes]
GO
ALTER TABLE [dbo].[UsersFourm] ADD  CONSTRAINT [DF__UsersFour__disli__7F2BE32F]  DEFAULT ((0)) FOR [dislikes]
GO
ALTER TABLE [dbo].[UsersFourm] ADD  CONSTRAINT [DF_UsersFourm_ParantId]  DEFAULT ((0)) FOR [ParantId]
GO
ALTER TABLE [dbo].[UsersFourm] ADD  CONSTRAINT [DF__UsersFour__publi__00200768]  DEFAULT ((0)) FOR [publish]
GO
ALTER TABLE [dbo].[UsersFourm] ADD  CONSTRAINT [DF__UsersFour__Creat__01142BA1]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [gsmsharing].[blog_files] ADD  DEFAULT (NULL) FOR [Blog_ID]
GO
ALTER TABLE [gsmsharing].[blog_files] ADD  DEFAULT (NULL) FOR [FileName]
GO
ALTER TABLE [gsmsharing].[blog_files] ADD  DEFAULT (NULL) FOR [FileSize]
GO
ALTER TABLE [gsmsharing].[blog_files] ADD  DEFAULT (NULL) FOR [Link]
GO
ALTER TABLE [gsmsharing].[blog_files] ADD  DEFAULT (getdate()) FOR [UploadDate]
GO
ALTER TABLE [gsmsharing].[blogcat] ADD  CONSTRAINT [DF__blogcat__Blog_ID__7928F116]  DEFAULT (NULL) FOR [Blog_ID]
GO
ALTER TABLE [gsmsharing].[blogcat] ADD  CONSTRAINT [DF__blogcat__catId__7A1D154F]  DEFAULT (NULL) FOR [catId]
GO
ALTER TABLE [gsmsharing].[blogcat] ADD  CONSTRAINT [DF__blogcat__SubCat__7B113988]  DEFAULT (NULL) FOR [SubCat]
GO
ALTER TABLE [gsmsharing].[blogcat] ADD  CONSTRAINT [DF__blogcat__catDate__7C055DC1]  DEFAULT (getdate()) FOR [catDate]
GO
ALTER TABLE [gsmsharing].[blogfolder] ADD  CONSTRAINT [DF__blogfolde__Blog___7CF981FA]  DEFAULT (NULL) FOR [Blog_ID]
GO
ALTER TABLE [gsmsharing].[blogfolder] ADD  CONSTRAINT [DF__blogfolde__folde__7DEDA633]  DEFAULT (NULL) FOR [folderid]
GO
ALTER TABLE [gsmsharing].[blogfolder] ADD  CONSTRAINT [DF__blogfolder__date__7EE1CA6C]  DEFAULT (getdate()) FOR [date]
GO
ALTER TABLE [gsmsharing].[blogposts] ADD  CONSTRAINT [DF__blogposts__UserI__7FD5EEA5]  DEFAULT (NULL) FOR [UserID]
GO
ALTER TABLE [gsmsharing].[blogposts] ADD  CONSTRAINT [DF__blogposts__FileI__00CA12DE]  DEFAULT (NULL) FOR [FileId]
GO
ALTER TABLE [gsmsharing].[blogposts] ADD  CONSTRAINT [DF__blogposts__FileN__01BE3717]  DEFAULT (NULL) FOR [FileName]
GO
ALTER TABLE [gsmsharing].[blogposts] ADD  CONSTRAINT [DF__blogposts__views__02B25B50]  DEFAULT ((0)) FOR [views]
GO
ALTER TABLE [gsmsharing].[blogposts] ADD  CONSTRAINT [DF__blogposts__likes__03A67F89]  DEFAULT ((0)) FOR [likes]
GO
ALTER TABLE [gsmsharing].[blogposts] ADD  CONSTRAINT [DF__blogposts__disli__049AA3C2]  DEFAULT ((0)) FOR [dislike]
GO
ALTER TABLE [gsmsharing].[blogposts] ADD  CONSTRAINT [DF__blogposts__size__058EC7FB]  DEFAULT (NULL) FOR [size]
GO
ALTER TABLE [gsmsharing].[blogposts] ADD  CONSTRAINT [DF__blogposts__webli__0682EC34]  DEFAULT (NULL) FOR [weblink]
GO
ALTER TABLE [gsmsharing].[blogposts] ADD  CONSTRAINT [DF__blogposts__creat__0777106D]  DEFAULT (getdate()) FOR [creationDate]
GO
ALTER TABLE [gsmsharing].[blogposts] ADD  CONSTRAINT [DF__blogposts__publi__086B34A6]  DEFAULT ((0)) FOR [publish]
GO
ALTER TABLE [gsmsharing].[category] ADD  DEFAULT ((0)) FOR [parantid]
GO
ALTER TABLE [gsmsharing].[category] ADD  DEFAULT (getdate()) FOR [creationDate]
GO
ALTER TABLE [gsmsharing].[categoryforum] ADD  DEFAULT (NULL) FOR [forumid]
GO
ALTER TABLE [gsmsharing].[categoryforum] ADD  DEFAULT (NULL) FOR [catName]
GO
ALTER TABLE [gsmsharing].[categoryforum] ADD  DEFAULT (NULL) FOR [parantID]
GO
ALTER TABLE [gsmsharing].[categoryforum] ADD  DEFAULT (NULL) FOR [creationDate]
GO
ALTER TABLE [gsmsharing].[categoryforum] ADD  DEFAULT (NULL) FOR [categoryforumcol]
GO
ALTER TABLE [gsmsharing].[code] ADD  CONSTRAINT [DF__code__UserId__100C566E]  DEFAULT (NULL) FOR [UserId]
GO
ALTER TABLE [gsmsharing].[code] ADD  CONSTRAINT [DF__code__title__11007AA7]  DEFAULT (NULL) FOR [title]
GO
ALTER TABLE [gsmsharing].[code] ADD  CONSTRAINT [DF__code__likes__11F49EE0]  DEFAULT ((0)) FOR [likes]
GO
ALTER TABLE [gsmsharing].[code] ADD  CONSTRAINT [DF__code__dislike__12E8C319]  DEFAULT ((0)) FOR [dislike]
GO
ALTER TABLE [gsmsharing].[code] ADD  CONSTRAINT [DF__code__views__13DCE752]  DEFAULT ((0)) FOR [views]
GO
ALTER TABLE [gsmsharing].[code] ADD  CONSTRAINT [DF__code__published__14D10B8B]  DEFAULT ((0)) FOR [published]
GO
ALTER TABLE [gsmsharing].[code] ADD  CONSTRAINT [DF__code__publishdat__15C52FC4]  DEFAULT (getdate()) FOR [publishdate]
GO
ALTER TABLE [gsmsharing].[codecategory] ADD  DEFAULT (NULL) FOR [CatName]
GO
ALTER TABLE [gsmsharing].[codecategory] ADD  DEFAULT (getdate()) FOR [PublishDate]
GO
ALTER TABLE [gsmsharing].[codecombine] ADD  DEFAULT (NULL) FOR [codeId]
GO
ALTER TABLE [gsmsharing].[codecombine] ADD  DEFAULT (NULL) FOR [catid]
GO
ALTER TABLE [gsmsharing].[codecombine] ADD  DEFAULT (NULL) FOR [SubId]
GO
ALTER TABLE [gsmsharing].[codecomments] ADD  CONSTRAINT [DF__codecomme__Useri__1B7E091A]  DEFAULT (NULL) FOR [Userid]
GO
ALTER TABLE [gsmsharing].[codecomments] ADD  CONSTRAINT [DF__codecomme__codei__1C722D53]  DEFAULT (NULL) FOR [codeid]
GO
ALTER TABLE [gsmsharing].[codecomments] ADD  CONSTRAINT [DF__codecomme__paran__1D66518C]  DEFAULT ((0)) FOR [parantid]
GO
ALTER TABLE [gsmsharing].[codecomments] ADD  CONSTRAINT [DF__codecomme__publi__1E5A75C5]  DEFAULT (getdate()) FOR [publishDate]
GO
ALTER TABLE [gsmsharing].[codesubcat] ADD  DEFAULT (getdate()) FOR [PublishDate]
GO
ALTER TABLE [gsmsharing].[comments] ADD  CONSTRAINT [DF__comments__UserID__2042BE37]  DEFAULT (NULL) FOR [UserID]
GO
ALTER TABLE [gsmsharing].[comments] ADD  CONSTRAINT [DF__comments__parant__2136E270]  DEFAULT ((0)) FOR [parantId]
GO
ALTER TABLE [gsmsharing].[comments] ADD  CONSTRAINT [DF__comments__commen__222B06A9]  DEFAULT (getdate()) FOR [commentDate]
GO
ALTER TABLE [gsmsharing].[gsmlog] ADD  DEFAULT (NULL) FOR [ipAddress]
GO
ALTER TABLE [gsmsharing].[gsmlog] ADD  DEFAULT (NULL) FOR [hostName]
GO
ALTER TABLE [gsmsharing].[gsmlog] ADD  DEFAULT (getdate()) FOR [visitTime]
GO
ALTER TABLE [gsmsharing].[news] ADD  CONSTRAINT [DF__news__UserID__25FB978D]  DEFAULT (NULL) FOR [UserID]
GO
ALTER TABLE [gsmsharing].[news] ADD  CONSTRAINT [DF__news__title__26EFBBC6]  DEFAULT (NULL) FOR [title]
GO
ALTER TABLE [gsmsharing].[news] ADD  CONSTRAINT [DF__news__newsdate__27E3DFFF]  DEFAULT (NULL) FOR [newsdate]
GO
ALTER TABLE [gsmsharing].[subcat] ADD  DEFAULT (NULL) FOR [catid]
GO
ALTER TABLE [gsmsharing].[subcat] ADD  DEFAULT (NULL) FOR [subName]
GO
ALTER TABLE [gsmsharing].[subcat] ADD  DEFAULT (getdate()) FOR [subDate]
GO
ALTER TABLE [gsmsharing].[user] ADD  CONSTRAINT [DF__user__admin__2BB470E3]  DEFAULT ((0)) FOR [admin]
GO
ALTER TABLE [gsmsharing].[user] ADD  CONSTRAINT [DF__user__CreationDa__2CA8951C]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [gsmsharing].[userforum] ADD  CONSTRAINT [DF__userforum__forum__2D9CB955]  DEFAULT (NULL) FOR [forumTitle]
GO
ALTER TABLE [gsmsharing].[userforum] ADD  CONSTRAINT [DF__userforum__paran__2E90DD8E]  DEFAULT ((0)) FOR [parantid]
GO
ALTER TABLE [gsmsharing].[userforum] ADD  CONSTRAINT [DF__userforum__voteu__2F8501C7]  DEFAULT ((0)) FOR [voteup]
GO
ALTER TABLE [gsmsharing].[userforum] ADD  CONSTRAINT [DF__userforum__voted__30792600]  DEFAULT ((0)) FOR [votedown]
GO
ALTER TABLE [gsmsharing].[userforum] ADD  CONSTRAINT [DF__userforum__forum__316D4A39]  DEFAULT (getdate()) FOR [forumDate]
GO
ALTER TABLE [dbo].[AdPostCat]  WITH CHECK ADD  CONSTRAINT [FK_AdPostCat_AdCategory] FOREIGN KEY([CatId])
REFERENCES [dbo].[AdCategory] ([Id])
GO
ALTER TABLE [dbo].[AdPostCat] CHECK CONSTRAINT [FK_AdPostCat_AdCategory]
GO
ALTER TABLE [dbo].[AdPostCat]  WITH CHECK ADD  CONSTRAINT [FK_AdPostCat_MobileAds] FOREIGN KEY([AdsId])
REFERENCES [dbo].[MobileAds] ([AdsId])
GO
ALTER TABLE [dbo].[AdPostCat] CHECK CONSTRAINT [FK_AdPostCat_MobileAds]
GO
ALTER TABLE [dbo].[AdsImage]  WITH CHECK ADD  CONSTRAINT [FK_AdsImage_MobileAds] FOREIGN KEY([AdsId])
REFERENCES [dbo].[MobileAds] ([AdsId])
GO
ALTER TABLE [dbo].[AdsImage] CHECK CONSTRAINT [FK_AdsImage_MobileAds]
GO
ALTER TABLE [dbo].[AdSubCat]  WITH CHECK ADD  CONSTRAINT [FK_AdSubCat_AdSubCat] FOREIGN KEY([MainCatId])
REFERENCES [dbo].[AdCategory] ([Id])
GO
ALTER TABLE [dbo].[AdSubCat] CHECK CONSTRAINT [FK_AdSubCat_AdSubCat]
GO
ALTER TABLE [dbo].[AffiliationProgram]  WITH CHECK ADD  CONSTRAINT [FKA_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[AffiliationProgram] CHECK CONSTRAINT [FKA_User]
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
ALTER TABLE [dbo].[BlogCatContainer]  WITH CHECK ADD  CONSTRAINT [FK_BlogCatContainer_MobileCategory] FOREIGN KEY([CatId])
REFERENCES [dbo].[MobileCategory] ([CategoryId])
GO
ALTER TABLE [dbo].[BlogCatContainer] CHECK CONSTRAINT [FK_BlogCatContainer_MobileCategory]
GO
ALTER TABLE [dbo].[BlogCatContainer]  WITH CHECK ADD  CONSTRAINT [FK_BlogCatContainer_MobilePosts] FOREIGN KEY([BlogId])
REFERENCES [dbo].[MobilePosts] ([BlogId])
GO
ALTER TABLE [dbo].[BlogCatContainer] CHECK CONSTRAINT [FK_BlogCatContainer_MobilePosts]
GO
ALTER TABLE [dbo].[BlogCatContainer]  WITH CHECK ADD  CONSTRAINT [FK_BlogCatContainer_MobileSubCategory] FOREIGN KEY([SubCatId])
REFERENCES [dbo].[MobileSubCategory] ([SubCategoryId])
GO
ALTER TABLE [dbo].[BlogCatContainer] CHECK CONSTRAINT [FK_BlogCatContainer_MobileSubCategory]
GO
ALTER TABLE [dbo].[BlogComments]  WITH CHECK ADD  CONSTRAINT [FK_BComment] FOREIGN KEY([BlogId])
REFERENCES [dbo].[MobilePosts] ([BlogId])
GO
ALTER TABLE [dbo].[BlogComments] CHECK CONSTRAINT [FK_BComment]
GO
ALTER TABLE [dbo].[BlogComments]  WITH CHECK ADD  CONSTRAINT [FK_BUComment] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[BlogComments] CHECK CONSTRAINT [FK_BUComment]
GO
ALTER TABLE [dbo].[BlogFolder]  WITH CHECK ADD  CONSTRAINT [FK_FB] FOREIGN KEY([BlogId])
REFERENCES [dbo].[MobilePosts] ([BlogId])
GO
ALTER TABLE [dbo].[BlogFolder] CHECK CONSTRAINT [FK_FB]
GO
ALTER TABLE [dbo].[BlogSEO]  WITH CHECK ADD  CONSTRAINT [FK_BlogSEO_GsmBlog] FOREIGN KEY([BlogId])
REFERENCES [dbo].[GsmBlog] ([BlogId])
GO
ALTER TABLE [dbo].[BlogSEO] CHECK CONSTRAINT [FK_BlogSEO_GsmBlog]
GO
ALTER TABLE [dbo].[BlogSpecContainer]  WITH CHECK ADD  CONSTRAINT [FK_BlogSpecContainer_MobilePosts] FOREIGN KEY([BlogId])
REFERENCES [dbo].[MobilePosts] ([BlogId])
GO
ALTER TABLE [dbo].[BlogSpecContainer] CHECK CONSTRAINT [FK_BlogSpecContainer_MobilePosts]
GO
ALTER TABLE [dbo].[BlogSpecContainer]  WITH CHECK ADD  CONSTRAINT [FK_BlogSpecContainer_MobileSpecs] FOREIGN KEY([SpecId])
REFERENCES [dbo].[MobileSpecs] ([Specid])
GO
ALTER TABLE [dbo].[BlogSpecContainer] CHECK CONSTRAINT [FK_BlogSpecContainer_MobileSpecs]
GO
ALTER TABLE [dbo].[ForumCategory]  WITH CHECK ADD  CONSTRAINT [FK_CUFourm] FOREIGN KEY([UserFourmID])
REFERENCES [dbo].[UsersFourm] ([UserFourmID])
GO
ALTER TABLE [dbo].[ForumCategory] CHECK CONSTRAINT [FK_CUFourm]
GO
ALTER TABLE [dbo].[ForumReplys]  WITH CHECK ADD  CONSTRAINT [Reply] FOREIGN KEY([ThreadId])
REFERENCES [dbo].[UsersFourm] ([UserFourmID])
GO
ALTER TABLE [dbo].[ForumReplys] CHECK CONSTRAINT [Reply]
GO
ALTER TABLE [dbo].[ForumReplys]  WITH CHECK ADD  CONSTRAINT [UserFK] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[ForumReplys] CHECK CONSTRAINT [UserFK]
GO
ALTER TABLE [dbo].[FourmComments]  WITH CHECK ADD  CONSTRAINT [FK_FComment] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[FourmComments] CHECK CONSTRAINT [FK_FComment]
GO
ALTER TABLE [dbo].[GsmBlog]  WITH NOCHECK ADD  CONSTRAINT [FK_GsmBlog_CatId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[GsmBlogCategory] ([CategoryId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GsmBlog] NOCHECK CONSTRAINT [FK_GsmBlog_CatId]
GO
ALTER TABLE [dbo].[GsmBlog]  WITH CHECK ADD  CONSTRAINT [FK_GsmBlog_GsmBlog] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GsmBlog] CHECK CONSTRAINT [FK_GsmBlog_GsmBlog]
GO
ALTER TABLE [dbo].[GsmBlogComments]  WITH CHECK ADD  CONSTRAINT [FK_GsmBlogComments_GsmBlogId] FOREIGN KEY([BlogId])
REFERENCES [dbo].[GsmBlog] ([BlogId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GsmBlogComments] CHECK CONSTRAINT [FK_GsmBlogComments_GsmBlogId]
GO
ALTER TABLE [dbo].[GsmBlogComments]  WITH CHECK ADD  CONSTRAINT [FK_GsmBlogComments_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[GsmBlogComments] CHECK CONSTRAINT [FK_GsmBlogComments_UserId]
GO
ALTER TABLE [dbo].[MobileAds]  WITH CHECK ADD  CONSTRAINT [FK_AdsU] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[MobileAds] CHECK CONSTRAINT [FK_AdsU]
GO
ALTER TABLE [dbo].[MobilePartAds]  WITH CHECK ADD  CONSTRAINT [FK_AdsMobAd] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[MobilePartAds] CHECK CONSTRAINT [FK_AdsMobAd]
GO
ALTER TABLE [dbo].[MobilePosts]  WITH CHECK ADD  CONSTRAINT [FK_MPostU] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[MobilePosts] CHECK CONSTRAINT [FK_MPostU]
GO
ALTER TABLE [dbo].[MobileSpecs]  WITH CHECK ADD  CONSTRAINT [FK_SpecUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[MobileSpecs] CHECK CONSTRAINT [FK_SpecUser]
GO
ALTER TABLE [dbo].[MobileSubCategory]  WITH CHECK ADD  CONSTRAINT [FK_MSCategory] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[MobileCategory] ([CategoryId])
GO
ALTER TABLE [dbo].[MobileSubCategory] CHECK CONSTRAINT [FK_MSCategory]
GO
ALTER TABLE [dbo].[ProductImage]  WITH CHECK ADD  CONSTRAINT [FK_ProductImage_AffiliationProgram] FOREIGN KEY([ProductId])
REFERENCES [dbo].[AffiliationProgram] ([ProductId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProductImage] CHECK CONSTRAINT [FK_ProductImage_AffiliationProgram]
GO
ALTER TABLE [dbo].[ProductReview]  WITH CHECK ADD  CONSTRAINT [FK_ProductReview_AffiliationProgram] FOREIGN KEY([BlogId])
REFERENCES [dbo].[AffiliationProgram] ([ProductId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProductReview] CHECK CONSTRAINT [FK_ProductReview_AffiliationProgram]
GO
ALTER TABLE [dbo].[ProductReview]  WITH CHECK ADD  CONSTRAINT [FK_ProductReview_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProductReview] CHECK CONSTRAINT [FK_ProductReview_AspNetUsers]
GO
ALTER TABLE [dbo].[profile]  WITH CHECK ADD  CONSTRAINT [FK_ProUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[profile] CHECK CONSTRAINT [FK_ProUser]
GO
ALTER TABLE [dbo].[ProfilePhoto]  WITH CHECK ADD  CONSTRAINT [FK_ProfilePhoto_AspNetUsers] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProfilePhoto] CHECK CONSTRAINT [FK_ProfilePhoto_AspNetUsers]
GO
ALTER TABLE [dbo].[rating_distribution]  WITH CHECK ADD  CONSTRAINT [fk_rating_distribution_product] FOREIGN KEY([product_id])
REFERENCES [dbo].[AmazonProducts] ([product_id])
GO
ALTER TABLE [dbo].[rating_distribution] CHECK CONSTRAINT [fk_rating_distribution_product]
GO
ALTER TABLE [dbo].[Review]  WITH CHECK ADD FOREIGN KEY([ProductAsin])
REFERENCES [dbo].[ReviewProduct] ([Asin])
GO
ALTER TABLE [dbo].[review_aspects]  WITH CHECK ADD  CONSTRAINT [fk_review_aspects_product] FOREIGN KEY([product_id])
REFERENCES [dbo].[AmazonProducts] ([product_id])
GO
ALTER TABLE [dbo].[review_aspects] CHECK CONSTRAINT [fk_review_aspects_product]
GO
ALTER TABLE [dbo].[ReviewImage]  WITH CHECK ADD FOREIGN KEY([ReviewId])
REFERENCES [dbo].[Review] ([ReviewId])
GO
ALTER TABLE [dbo].[SocialCommunities]  WITH CHECK ADD FOREIGN KEY([CategoryId])
REFERENCES [dbo].[SocialCategories] ([CategoryId])
GO
ALTER TABLE [dbo].[Users_Communities]  WITH CHECK ADD FOREIGN KEY([CommunityId])
REFERENCES [dbo].[SocialCommunities] ([CommunityId])
GO
ALTER TABLE [dbo].[Users_Communities]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[UsersFourm]  WITH CHECK ADD  CONSTRAINT [FK_UFourm] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[UsersFourm] CHECK CONSTRAINT [FK_UFourm]
GO
ALTER TABLE [gsmsharing].[blogcat]  WITH NOCHECK ADD  CONSTRAINT [blogcat$FK_container] FOREIGN KEY([Blog_ID])
REFERENCES [gsmsharing].[blogposts] ([Blog_ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [gsmsharing].[blogcat] CHECK CONSTRAINT [blogcat$FK_container]
GO
ALTER TABLE [gsmsharing].[blogcat]  WITH NOCHECK ADD  CONSTRAINT [blogcat$FK_containerCat] FOREIGN KEY([catId])
REFERENCES [gsmsharing].[category] ([catid])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [gsmsharing].[blogcat] CHECK CONSTRAINT [blogcat$FK_containerCat]
GO
ALTER TABLE [gsmsharing].[blogcat]  WITH NOCHECK ADD  CONSTRAINT [blogcat$FK_subcat] FOREIGN KEY([SubCat])
REFERENCES [gsmsharing].[subcat] ([subid])
GO
ALTER TABLE [gsmsharing].[blogcat] CHECK CONSTRAINT [blogcat$FK_subcat]
GO
ALTER TABLE [gsmsharing].[blogfolder]  WITH NOCHECK ADD  CONSTRAINT [blogfolder$blog_key] FOREIGN KEY([Blog_ID])
REFERENCES [gsmsharing].[blogposts] ([Blog_ID])
GO
ALTER TABLE [gsmsharing].[blogfolder] CHECK CONSTRAINT [blogfolder$blog_key]
GO
ALTER TABLE [gsmsharing].[blogposts]  WITH NOCHECK ADD  CONSTRAINT [blogposts$FK_User] FOREIGN KEY([UserID])
REFERENCES [gsmsharing].[user] ([UserID])
GO
ALTER TABLE [gsmsharing].[blogposts] CHECK CONSTRAINT [blogposts$FK_User]
GO
ALTER TABLE [gsmsharing].[categoryforum]  WITH NOCHECK ADD  CONSTRAINT [categoryforum$FK_ForumKeyUser] FOREIGN KEY([forumid])
REFERENCES [gsmsharing].[userforum] ([forumid])
GO
ALTER TABLE [gsmsharing].[categoryforum] CHECK CONSTRAINT [categoryforum$FK_ForumKeyUser]
GO
ALTER TABLE [gsmsharing].[code]  WITH NOCHECK ADD  CONSTRAINT [code$Programming_FK] FOREIGN KEY([UserId])
REFERENCES [gsmsharing].[user] ([UserID])
GO
ALTER TABLE [gsmsharing].[code] CHECK CONSTRAINT [code$Programming_FK]
GO
ALTER TABLE [gsmsharing].[codecombine]  WITH NOCHECK ADD  CONSTRAINT [codecombine$FK_Blog] FOREIGN KEY([codeId])
REFERENCES [gsmsharing].[code] ([idcode])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [gsmsharing].[codecombine] CHECK CONSTRAINT [codecombine$FK_Blog]
GO
ALTER TABLE [gsmsharing].[codecombine]  WITH NOCHECK ADD  CONSTRAINT [codecombine$FK_CodeSub] FOREIGN KEY([SubId])
REFERENCES [gsmsharing].[codesubcat] ([SubId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [gsmsharing].[codecombine] CHECK CONSTRAINT [codecombine$FK_CodeSub]
GO
ALTER TABLE [gsmsharing].[codecombine]  WITH NOCHECK ADD  CONSTRAINT [codecombine$FK_MainCat] FOREIGN KEY([catid])
REFERENCES [gsmsharing].[codecategory] ([catid])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [gsmsharing].[codecombine] CHECK CONSTRAINT [codecombine$FK_MainCat]
GO
ALTER TABLE [gsmsharing].[codecomments]  WITH NOCHECK ADD  CONSTRAINT [codecomments$FK_PostCode] FOREIGN KEY([codeid])
REFERENCES [gsmsharing].[code] ([idcode])
GO
ALTER TABLE [gsmsharing].[codecomments] CHECK CONSTRAINT [codecomments$FK_PostCode]
GO
ALTER TABLE [gsmsharing].[codecomments]  WITH NOCHECK ADD  CONSTRAINT [codecomments$FK_UserCode] FOREIGN KEY([Userid])
REFERENCES [gsmsharing].[user] ([UserID])
GO
ALTER TABLE [gsmsharing].[codecomments] CHECK CONSTRAINT [codecomments$FK_UserCode]
GO
ALTER TABLE [gsmsharing].[codesubcat]  WITH NOCHECK ADD  CONSTRAINT [codesubcat$Fk_Main] FOREIGN KEY([Main_Id])
REFERENCES [gsmsharing].[codecategory] ([catid])
GO
ALTER TABLE [gsmsharing].[codesubcat] CHECK CONSTRAINT [codesubcat$Fk_Main]
GO
ALTER TABLE [gsmsharing].[comments]  WITH NOCHECK ADD  CONSTRAINT [comments$F_User] FOREIGN KEY([UserID])
REFERENCES [gsmsharing].[user] ([UserID])
GO
ALTER TABLE [gsmsharing].[comments] CHECK CONSTRAINT [comments$F_User]
GO
ALTER TABLE [gsmsharing].[news]  WITH NOCHECK ADD  CONSTRAINT [news$FK_News] FOREIGN KEY([UserID])
REFERENCES [gsmsharing].[user] ([UserID])
GO
ALTER TABLE [gsmsharing].[news] CHECK CONSTRAINT [news$FK_News]
GO
ALTER TABLE [gsmsharing].[subcat]  WITH NOCHECK ADD  CONSTRAINT [subcat$FK_Sub] FOREIGN KEY([catid])
REFERENCES [gsmsharing].[category] ([catid])
GO
ALTER TABLE [gsmsharing].[subcat] CHECK CONSTRAINT [subcat$FK_Sub]
GO
ALTER TABLE [gsmsharing].[userforum]  WITH NOCHECK ADD  CONSTRAINT [userforum$FK_forumKey] FOREIGN KEY([UserID])
REFERENCES [gsmsharing].[user] ([UserID])
GO
ALTER TABLE [gsmsharing].[userforum] CHECK CONSTRAINT [userforum$FK_forumKey]
GO
ALTER TABLE [dbo].[review_aspects]  WITH CHECK ADD  CONSTRAINT [chk_sentiment] CHECK  (([sentiment]='MIXED' OR [sentiment]='NEGATIVE' OR [sentiment]='POSITIVE'))
GO
ALTER TABLE [dbo].[review_aspects] CHECK CONSTRAINT [chk_sentiment]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.blog_files' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'blog_files'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.blogcat' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'blogcat'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.blogfolder' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'blogfolder'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.blogposts' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'blogposts'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.category' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'category'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.categoryforum' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'categoryforum'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.code' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'code'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.codecategory' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'codecategory'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.codecombine' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'codecombine'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.codecomments' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'codecomments'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.codesubcat' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'codesubcat'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.comments' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'comments'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.gsmlog' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'gsmlog'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.news' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'news'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.subcat' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'subcat'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.`user`' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'user'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'gsmsharing.userforum' , @level0type=N'SCHEMA',@level0name=N'gsmsharing', @level1type=N'TABLE',@level1name=N'userforum'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[12] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "MobilePosts"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 200
               Right = 257
            End
            DisplayFlags = 280
            TopColumn = 10
         End
         Begin Table = "AspNetUsers"
            Begin Extent = 
               Top = 7
               Left = 290
               Bottom = 233
               Right = 691
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 24
         Width = 284
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'GetAllPosts'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'GetAllPosts'
GO
