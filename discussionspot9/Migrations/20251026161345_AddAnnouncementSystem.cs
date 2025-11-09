using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace discussionspot9.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.UserProfiles', 'BanExpiresAt') IS NULL
BEGIN
    ALTER TABLE [dbo].[UserProfiles] ADD [BanExpiresAt] datetime2 NULL;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.UserProfiles', 'BanReason') IS NULL
BEGIN
    ALTER TABLE [dbo].[UserProfiles] ADD [BanReason] nvarchar(max) NULL;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.UserProfiles', 'IsBanned') IS NULL
BEGIN
    ALTER TABLE [dbo].[UserProfiles] ADD [IsBanned] bit NOT NULL CONSTRAINT DF_UserProfiles_IsBanned DEFAULT(0);
    ALTER TABLE [dbo].[UserProfiles] DROP CONSTRAINT DF_UserProfiles_IsBanned;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Comments', 'EditedAt') IS NULL
BEGIN
    ALTER TABLE [dbo].[Comments] ADD [EditedAt] datetime2 NULL;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Comments', 'IsPinned') IS NULL
BEGIN
    ALTER TABLE [dbo].[Comments] ADD [IsPinned] bit NOT NULL CONSTRAINT DF_Comments_IsPinned DEFAULT(0);
    ALTER TABLE [dbo].[Comments] DROP CONSTRAINT DF_Comments_IsPinned;
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Announcements]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Announcements] (
        [AnnouncementId] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(500) NOT NULL,
        [Type] nvarchar(20) NOT NULL DEFAULT N'info',
        [Icon] nvarchar(50) NOT NULL DEFAULT N'fa-info-circle',
        [LinkUrl] nvarchar(500) NULL,
        [LinkText] nvarchar(100) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [IsDismissible] bit NOT NULL DEFAULT CAST(1 AS bit),
        [Priority] int NOT NULL DEFAULT 0,
        [StartDate] datetime2 NULL,
        [EndDate] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        [CreatedByUserId] nvarchar(450) NULL,
        CONSTRAINT [PK_Announcements] PRIMARY KEY ([AnnouncementId]),
        CONSTRAINT [CK_Announcement_Type] CHECK ([Type] IN ('info', 'success', 'warning', 'danger'))
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[ModerationLogs]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ModerationLogs] (
        [LogId] bigint NOT NULL IDENTITY,
        [ModeratorId] nvarchar(450) NOT NULL,
        [TargetUserId] nvarchar(450) NOT NULL,
        [CommunityId] int NULL,
        [ActionType] nvarchar(max) NOT NULL,
        [EntityType] nvarchar(max) NOT NULL,
        [EntityId] nvarchar(max) NULL,
        [Reason] nvarchar(max) NOT NULL,
        [PerformedAt] datetime2 NOT NULL,
        [OldValue] nvarchar(max) NULL,
        [NewValue] nvarchar(max) NULL,
        [ModeratorIp] nvarchar(max) NULL,
        [Metadata] nvarchar(max) NULL,
        CONSTRAINT [PK_ModerationLogs] PRIMARY KEY ([LogId]),
        CONSTRAINT [FK_ModerationLogs_AspNetUsers_ModeratorId] FOREIGN KEY ([ModeratorId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ModerationLogs_AspNetUsers_TargetUserId] FOREIGN KEY ([TargetUserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ModerationLogs_Communities_CommunityId] FOREIGN KEY ([CommunityId]) REFERENCES [Communities]([CommunityId])
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[PostReports]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PostReports] (
        [ReportId] int NOT NULL IDENTITY,
        [PostId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Reason] nvarchar(200) NOT NULL,
        [Details] NVARCHAR(MAX) NULL,
        [Status] nvarchar(50) NOT NULL DEFAULT N'pending',
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        [ReviewedAt] datetime2 NULL,
        [ReviewedByUserId] nvarchar(450) NULL,
        [AdminNotes] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_PostReports] PRIMARY KEY ([ReportId]),
        CONSTRAINT [CK_PostReport_Status] CHECK ([Status] IN ('pending', 'reviewed', 'resolved', 'dismissed')),
        CONSTRAINT [FK_PostReports_AspNetUsers_ReviewedByUserId] FOREIGN KEY ([ReviewedByUserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_PostReports_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PostReports_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts]([PostId]) ON DELETE CASCADE
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[ShareActivities]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ShareActivities] (
        [ShareId] int NOT NULL IDENTITY,
        [ContentType] nvarchar(50) NOT NULL,
        [ContentId] int NOT NULL,
        [Platform] nvarchar(50) NOT NULL,
        [UserId] nvarchar(max) NULL,
        [SharedAt] datetime2 NOT NULL,
        [IpAddress] nvarchar(50) NULL,
        [UserAgent] nvarchar(500) NULL,
        [ReferrerUrl] nvarchar(2048) NULL,
        [CountryCode] nvarchar(max) NULL,
        [City] nvarchar(max) NULL,
        [DeviceType] nvarchar(max) NULL,
        [BrowserName] nvarchar(max) NULL,
        [OsName] nvarchar(max) NULL,
        CONSTRAINT [PK_ShareActivities] PRIMARY KEY ([ShareId])
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[SiteRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SiteRoles] (
        [RoleId] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [RoleName] nvarchar(50) NOT NULL,
        [AssignedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        [AssignedByUserId] nvarchar(450) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [Notes] NVARCHAR(MAX) NULL,
        [RemovedAt] datetime2 NULL,
        [RemovedByUserId] nvarchar(450) NULL,
        CONSTRAINT [PK_SiteRoles] PRIMARY KEY ([RoleId]),
        CONSTRAINT [CK_SiteRole_RoleName] CHECK ([RoleName] IN ('SiteAdmin', 'Moderator', 'Verified', 'Partner')),
        CONSTRAINT [FK_SiteRoles_AspNetUsers_AssignedByUserId] FOREIGN KEY ([AssignedByUserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SiteRoles_AspNetUsers_RemovedByUserId] FOREIGN KEY ([RemovedByUserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_SiteRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE NO ACTION
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Stories]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Stories] (
        [StoryId] int NOT NULL IDENTITY,
        [Title] nvarchar(300) NOT NULL,
        [Slug] nvarchar(320) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [UserId] nvarchar(450) NULL,
        [CommunityId] int NULL,
        [Status] nvarchar(20) NOT NULL DEFAULT N'draft',
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        [PublishedAt] datetime2 NULL,
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        [ViewCount] int NOT NULL DEFAULT 0,
        [SlideCount] int NOT NULL DEFAULT 0,
        [TotalDuration] int NOT NULL DEFAULT 0,
        [PosterImageUrl] nvarchar(2048) NULL,
        [PublisherLogo] nvarchar(2048) NULL,
        [PublisherName] nvarchar(200) NULL,
        [IsAmpEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CanonicalUrl] nvarchar(2048) NULL,
        [MetaDescription] nvarchar(500) NULL,
        [MetaKeywords] nvarchar(500) NULL,
        CONSTRAINT [PK_Stories] PRIMARY KEY ([StoryId]),
        CONSTRAINT [CK_Story_Status] CHECK ([Status] IN ('draft', 'published', 'archived')),
        CONSTRAINT [FK_Stories_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Stories_Communities_CommunityId] FOREIGN KEY ([CommunityId]) REFERENCES [Communities]([CommunityId]) ON DELETE SET NULL
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[UserBans]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserBans] (
        [BanId] int NOT NULL IDENTITY,
        [UserId] nvarchar(max) NOT NULL,
        [CommunityId] int NULL,
        [BannedByUserId] nvarchar(450) NOT NULL,
        [Reason] nvarchar(max) NOT NULL,
        [BannedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NULL,
        [IsPermanent] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [BanType] nvarchar(max) NOT NULL,
        [ModeratorNotes] nvarchar(max) NULL,
        [LiftedAt] datetime2 NULL,
        [LiftedByUserId] nvarchar(450) NULL,
        [LiftReason] nvarchar(max) NULL,
        [BannedUserId] nvarchar(450) NULL,
        CONSTRAINT [PK_UserBans] PRIMARY KEY ([BanId]),
        CONSTRAINT [FK_UserBans_AspNetUsers_BannedByUserId] FOREIGN KEY ([BannedByUserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserBans_AspNetUsers_BannedUserId] FOREIGN KEY ([BannedUserId]) REFERENCES [AspNetUsers]([Id]),
        CONSTRAINT [FK_UserBans_AspNetUsers_LiftedByUserId] FOREIGN KEY ([LiftedByUserId]) REFERENCES [AspNetUsers]([Id]),
        CONSTRAINT [FK_UserBans_Communities_CommunityId] FOREIGN KEY ([CommunityId]) REFERENCES [Communities]([CommunityId])
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[StorySlides]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[StorySlides] (
        [StorySlideId] int NOT NULL IDENTITY,
        [StoryId] int NOT NULL,
        [MediaId] int NULL,
        [Caption] nvarchar(1000) NULL,
        [Headline] nvarchar(200) NULL,
        [Text] nvarchar(2000) NULL,
        [Duration] int NOT NULL DEFAULT 5000,
        [OrderIndex] int NOT NULL DEFAULT 0,
        [SlideType] nvarchar(20) NOT NULL DEFAULT N'media',
        [BackgroundColor] nvarchar(20) NULL,
        [TextColor] nvarchar(20) NULL,
        [FontSize] nvarchar(20) NULL,
        [Alignment] nvarchar(20) NULL DEFAULT N'center',
        [MediaUrl] nvarchar(500) NULL,
        [MediaType] nvarchar(50) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_StorySlides] PRIMARY KEY ([StorySlideId]),
        CONSTRAINT [CK_StorySlide_Type] CHECK ([SlideType] IN ('media', 'text', 'video', 'image')),
        CONSTRAINT [FK_StorySlides_Media_MediaId] FOREIGN KEY ([MediaId]) REFERENCES [Media]([MediaId]) ON DELETE SET NULL,
        CONSTRAINT [FK_StorySlides_Stories_StoryId] FOREIGN KEY ([StoryId]) REFERENCES [Stories]([StoryId]) ON DELETE CASCADE
    );
END
");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM [dbo].[UserProfiles]
    WHERE [UserId] = '4d5e6f7g-8h9i-0j1k-2l3m-n4o5p6q7r8s9'
)
BEGIN
    UPDATE [dbo].[UserProfiles]
    SET [BanExpiresAt] = NULL,
        [BanReason] = NULL,
        [IsBanned] = 0
    WHERE [UserId] = '4d5e6f7g-8h9i-0j1k-2l3m-n4o5p6q7r8s9';
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Announcements_IsActive_Priority' AND object_id = OBJECT_ID(N'[dbo].[Announcements]'))
BEGIN
    CREATE INDEX [IX_Announcements_IsActive_Priority] ON [dbo].[Announcements] ([IsActive], [Priority]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Announcements_StartDate_EndDate' AND object_id = OBJECT_ID(N'[dbo].[Announcements]'))
BEGIN
    CREATE INDEX [IX_Announcements_StartDate_EndDate] ON [dbo].[Announcements] ([StartDate], [EndDate]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ModerationLogs_CommunityId' AND object_id = OBJECT_ID(N'[dbo].[ModerationLogs]'))
BEGIN
    CREATE INDEX [IX_ModerationLogs_CommunityId] ON [dbo].[ModerationLogs] ([CommunityId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ModerationLogs_ModeratorId' AND object_id = OBJECT_ID(N'[dbo].[ModerationLogs]'))
BEGIN
    CREATE INDEX [IX_ModerationLogs_ModeratorId] ON [dbo].[ModerationLogs] ([ModeratorId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ModerationLogs_TargetUserId' AND object_id = OBJECT_ID(N'[dbo].[ModerationLogs]'))
BEGIN
    CREATE INDEX [IX_ModerationLogs_TargetUserId] ON [dbo].[ModerationLogs] ([TargetUserId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PostReports_PostId' AND object_id = OBJECT_ID(N'[dbo].[PostReports]'))
BEGIN
    CREATE INDEX [IX_PostReports_PostId] ON [dbo].[PostReports] ([PostId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PostReports_ReviewedByUserId' AND object_id = OBJECT_ID(N'[dbo].[PostReports]'))
BEGIN
    CREATE INDEX [IX_PostReports_ReviewedByUserId] ON [dbo].[PostReports] ([ReviewedByUserId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PostReports_Status_CreatedAt' AND object_id = OBJECT_ID(N'[dbo].[PostReports]'))
BEGIN
    CREATE INDEX [IX_PostReports_Status_CreatedAt] ON [dbo].[PostReports] ([Status], [CreatedAt]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PostReports_UserId' AND object_id = OBJECT_ID(N'[dbo].[PostReports]'))
BEGIN
    CREATE INDEX [IX_PostReports_UserId] ON [dbo].[PostReports] ([UserId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SiteRoles_AssignedByUserId' AND object_id = OBJECT_ID(N'[dbo].[SiteRoles]'))
BEGIN
    CREATE INDEX [IX_SiteRoles_AssignedByUserId] ON [dbo].[SiteRoles] ([AssignedByUserId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SiteRoles_RemovedByUserId' AND object_id = OBJECT_ID(N'[dbo].[SiteRoles]'))
BEGIN
    CREATE INDEX [IX_SiteRoles_RemovedByUserId] ON [dbo].[SiteRoles] ([RemovedByUserId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SiteRoles_RoleName' AND object_id = OBJECT_ID(N'[dbo].[SiteRoles]'))
BEGIN
    CREATE INDEX [IX_SiteRoles_RoleName] ON [dbo].[SiteRoles] ([RoleName]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SiteRoles_UserId_RoleName_IsActive' AND object_id = OBJECT_ID(N'[dbo].[SiteRoles]'))
BEGIN
    CREATE INDEX [IX_SiteRoles_UserId_RoleName_IsActive] ON [dbo].[SiteRoles] ([UserId], [RoleName], [IsActive]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Stories_CommunityId' AND object_id = OBJECT_ID(N'[dbo].[Stories]'))
BEGIN
    CREATE INDEX [IX_Stories_CommunityId] ON [dbo].[Stories] ([CommunityId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Stories_Slug' AND object_id = OBJECT_ID(N'[dbo].[Stories]'))
BEGIN
    CREATE INDEX [IX_Stories_Slug] ON [dbo].[Stories] ([Slug]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Stories_Slug_CommunityId' AND object_id = OBJECT_ID(N'[dbo].[Stories]'))
BEGIN
    CREATE UNIQUE INDEX [IX_Stories_Slug_CommunityId] ON [dbo].[Stories] ([Slug], [CommunityId]) WHERE [CommunityId] IS NOT NULL;
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Stories_Status_PublishedAt' AND object_id = OBJECT_ID(N'[dbo].[Stories]'))
BEGIN
    CREATE INDEX [IX_Stories_Status_PublishedAt] ON [dbo].[Stories] ([Status], [PublishedAt]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Stories_UserId_CreatedAt' AND object_id = OBJECT_ID(N'[dbo].[Stories]'))
BEGIN
    CREATE INDEX [IX_Stories_UserId_CreatedAt] ON [dbo].[Stories] ([UserId], [CreatedAt]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StorySlides_MediaId' AND object_id = OBJECT_ID(N'[dbo].[StorySlides]'))
BEGIN
    CREATE INDEX [IX_StorySlides_MediaId] ON [dbo].[StorySlides] ([MediaId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StorySlides_StoryId_OrderIndex' AND object_id = OBJECT_ID(N'[dbo].[StorySlides]'))
BEGIN
    CREATE INDEX [IX_StorySlides_StoryId_OrderIndex] ON [dbo].[StorySlides] ([StoryId], [OrderIndex]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserBans_BannedByUserId' AND object_id = OBJECT_ID(N'[dbo].[UserBans]'))
BEGIN
    CREATE INDEX [IX_UserBans_BannedByUserId] ON [dbo].[UserBans] ([BannedByUserId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserBans_BannedUserId' AND object_id = OBJECT_ID(N'[dbo].[UserBans]'))
BEGIN
    CREATE INDEX [IX_UserBans_BannedUserId] ON [dbo].[UserBans] ([BannedUserId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserBans_CommunityId' AND object_id = OBJECT_ID(N'[dbo].[UserBans]'))
BEGIN
    CREATE INDEX [IX_UserBans_CommunityId] ON [dbo].[UserBans] ([CommunityId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserBans_LiftedByUserId' AND object_id = OBJECT_ID(N'[dbo].[UserBans]'))
BEGIN
    CREATE INDEX [IX_UserBans_LiftedByUserId] ON [dbo].[UserBans] ([LiftedByUserId]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[StorySlides]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[StorySlides];
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[UserBans]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[UserBans];
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Stories]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Stories];
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[SiteRoles]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[SiteRoles];
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[ShareActivities]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[ShareActivities];
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[PostReports]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[PostReports];
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[ModerationLogs]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[ModerationLogs];
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Announcements]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Announcements];
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.UserProfiles', 'BanExpiresAt') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[UserProfiles] DROP COLUMN [BanExpiresAt];
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.UserProfiles', 'BanReason') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[UserProfiles] DROP COLUMN [BanReason];
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.UserProfiles', 'IsBanned') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[UserProfiles] DROP COLUMN [IsBanned];
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Comments', 'EditedAt') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Comments] DROP COLUMN [EditedAt];
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Comments', 'IsPinned') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Comments] DROP COLUMN [IsPinned];
END
");
        }
    }
}
