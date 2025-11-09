using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace discussionspot9.Migrations
{
    /// <inheritdoc />
    public partial class AddKeywordCategoriesToSeoMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Media");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.SeoMetadata', 'LongtailKeywords') IS NULL
    ALTER TABLE [SeoMetadata] ADD [LongtailKeywords] nvarchar(max) NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.SeoMetadata', 'PrimaryKeywords') IS NULL
    ALTER TABLE [SeoMetadata] ADD [PrimaryKeywords] nvarchar(max) NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.SeoMetadata', 'SecondaryKeywords') IS NULL
    ALTER TABLE [SeoMetadata] ADD [SecondaryKeywords] nvarchar(max) NULL;
");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SavedAt",
                table: "SavedPosts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.PollConfigurations', 'ClosedAt') IS NULL
    ALTER TABLE [PollConfigurations] ADD [ClosedAt] datetime2 NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.PollConfigurations', 'ClosedByUserId') IS NULL
    ALTER TABLE [PollConfigurations] ADD [ClosedByUserId] nvarchar(450) NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.PollConfigurations', 'PollDescription') IS NULL
    ALTER TABLE [PollConfigurations] ADD [PollDescription] nvarchar(1000) NULL;
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.PollConfigurations', 'PollQuestion') IS NULL
    ALTER TABLE [PollConfigurations] ADD [PollQuestion] nvarchar(500) NULL;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PollConfigurations_ClosedByUserId'
      AND object_id = OBJECT_ID(N'[dbo].[PollConfigurations]')
)
BEGIN
    CREATE INDEX [IX_PollConfigurations_ClosedByUserId]
    ON [dbo].[PollConfigurations]([ClosedByUserId]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[AdSenseRevenues]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AdSenseRevenues](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AdSenseRevenues] PRIMARY KEY,
        [Date] DATE NOT NULL,
        [PostId] INT NULL,
        [Earnings] DECIMAL(10,2) NOT NULL,
        [EstimatedEarnings] DECIMAL(10,2) NOT NULL,
        [PageViews] INT NOT NULL,
        [AdClicks] INT NOT NULL,
        [CTR] DECIMAL(5,2) NOT NULL,
        [CPC] DECIMAL(10,2) NOT NULL,
        [RPM] DECIMAL(10,2) NOT NULL,
        [AdImpressions] INT NOT NULL,
        [ActiveViewViewableImpressions] DECIMAL(10,2) NOT NULL,
        [Coverage] DECIMAL(5,2) NOT NULL,
        [SyncedAt] DATETIME2 NOT NULL CONSTRAINT [DF_AdSenseRevenues_SyncedAt] DEFAULT (GETDATE()),
        [Source] NVARCHAR(50) NOT NULL
    );

    ALTER TABLE [dbo].[AdSenseRevenues] WITH CHECK
        ADD CONSTRAINT [FK_AdSenseRevenues_Posts_PostId]
        FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts]([PostId]) ON DELETE CASCADE;

    CREATE INDEX [IX_AdSenseRevenues_Date_PostId] ON [dbo].[AdSenseRevenues]([Date], [PostId]);
    CREATE INDEX [IX_AdSenseRevenues_PostId] ON [dbo].[AdSenseRevenues]([PostId]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[CommentLinkPreviews]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CommentLinkPreviews](
        [CommentLinkPreviewId] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_CommentLinkPreviews] PRIMARY KEY,
        [CommentId] INT NOT NULL,
        [Url] NVARCHAR(2048) NOT NULL,
        [Title] NVARCHAR(500) NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        [Domain] NVARCHAR(255) NOT NULL,
        [ThumbnailUrl] NVARCHAR(2048) NULL,
        [FaviconUrl] NVARCHAR(2048) NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_CommentLinkPreviews_CreatedAt] DEFAULT (GETDATE()),
        [LastFetchedAt] DATETIME2 NULL,
        [FetchSucceeded] BIT NOT NULL CONSTRAINT [DF_CommentLinkPreviews_FetchSucceeded] DEFAULT (1)
    );

    ALTER TABLE [dbo].[CommentLinkPreviews] WITH CHECK
        ADD CONSTRAINT [FK_CommentLinkPreviews_Comments_CommentId]
        FOREIGN KEY([CommentId]) REFERENCES [dbo].[Comments]([CommentId]) ON DELETE CASCADE;

    CREATE INDEX [IX_CommentLinkPreviews_CommentId] ON [dbo].[CommentLinkPreviews]([CommentId]);
    CREATE INDEX [IX_CommentLinkPreviews_Url] ON [dbo].[CommentLinkPreviews]([Url]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[ContentRecommendations]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ContentRecommendations](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ContentRecommendations] PRIMARY KEY,
        [RecommendationType] NVARCHAR(100) NOT NULL,
        [Title] NVARCHAR(500) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [RelatedPostId] INT NULL,
        [CommunityId] INT NULL,
        [EstimatedRevenueImpact] DECIMAL(10,2) NOT NULL,
        [EstimatedTrafficImpact] DECIMAL(10,2) NOT NULL,
        [ConfidenceScore] DECIMAL(5,2) NOT NULL,
        [Priority] INT NOT NULL,
        [AnalysisData] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(20) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_ContentRecommendations_CreatedAt] DEFAULT (GETDATE()),
        [ImplementedAt] DATETIME2 NULL,
        [ImplementedBy] NVARCHAR(450) NULL
    );

    ALTER TABLE [dbo].[ContentRecommendations] WITH CHECK
        ADD CONSTRAINT [FK_ContentRecommendations_Posts_RelatedPostId]
        FOREIGN KEY([RelatedPostId]) REFERENCES [dbo].[Posts]([PostId]) ON DELETE SET NULL;

    ALTER TABLE [dbo].[ContentRecommendations] WITH CHECK
        ADD CONSTRAINT [FK_ContentRecommendations_Communities_CommunityId]
        FOREIGN KEY([CommunityId]) REFERENCES [dbo].[Communities]([CommunityId]) ON DELETE SET NULL;

    CREATE INDEX [IX_ContentRecommendations_CommunityId] ON [dbo].[ContentRecommendations]([CommunityId]);
    CREATE INDEX [IX_ContentRecommendations_RelatedPostId] ON [dbo].[ContentRecommendations]([RelatedPostId]);
    CREATE INDEX [IX_ContentRecommendations_Status_Priority] ON [dbo].[ContentRecommendations]([Status], [Priority]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[EnhancedSeoMetadata]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[EnhancedSeoMetadata](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_EnhancedSeoMetadata] PRIMARY KEY,
        [PostId] INT NOT NULL,
        [OriginalMetaDescription] NVARCHAR(500) NULL,
        [OptimizedMetaDescription] NVARCHAR(500) NULL,
        [PredictedCtrImprovement] DECIMAL(5,2) NOT NULL,
        [EmotionalTriggers] NVARCHAR(500) NULL,
        [PowerWords] NVARCHAR(500) NULL,
        [PrimaryKeywords] NVARCHAR(200) NULL,
        [SecondaryKeywords] NVARCHAR(500) NULL,
        [LsiKeywords] NVARCHAR(1000) NULL,
        [TotalSearchVolume] BIGINT NOT NULL,
        [ReadabilityScore] DECIMAL(5,2) NOT NULL,
        [SeoScore] INT NOT NULL,
        [KeywordDensity] DECIMAL(5,2) NOT NULL,
        [CompetitorAnalysis] NVARCHAR(MAX) NULL,
        [SerpPreview] NVARCHAR(MAX) NULL,
        [IsApproved] BIT NOT NULL,
        [ApprovedBy] NVARCHAR(450) NULL,
        [ApprovedAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL
    );

    ALTER TABLE [dbo].[EnhancedSeoMetadata] WITH CHECK
        ADD CONSTRAINT [FK_EnhancedSeoMetadata_Posts_PostId]
        FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts]([PostId]) ON DELETE CASCADE;

    CREATE INDEX [IX_EnhancedSeoMetadata_PostId] ON [dbo].[EnhancedSeoMetadata]([PostId]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[MultiSiteRevenues]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MultiSiteRevenues](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_MultiSiteRevenues] PRIMARY KEY,
        [SiteDomain] NVARCHAR(100) NOT NULL,
        [Date] DATETIME2 NOT NULL,
        [PostId] INT NULL,
        [PostUrl] NVARCHAR(500) NULL,
        [Earnings] DECIMAL(18,2) NOT NULL,
        [EstimatedEarnings] DECIMAL(18,2) NOT NULL,
        [PageViews] INT NOT NULL,
        [AdClicks] INT NOT NULL,
        [CTR] DECIMAL(5,2) NOT NULL,
        [CPC] DECIMAL(10,2) NOT NULL,
        [RPM] DECIMAL(10,2) NOT NULL,
        [AdImpressions] INT NOT NULL,
        [ActiveViewViewableImpressions] INT NOT NULL,
        [Coverage] DECIMAL(5,2) NOT NULL,
        [SyncedAt] DATETIME2 NOT NULL,
        [Source] NVARCHAR(50) NOT NULL
    );

    ALTER TABLE [dbo].[MultiSiteRevenues] WITH CHECK
        ADD CONSTRAINT [FK_MultiSiteRevenues_Posts_PostId]
        FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts]([PostId]);

    CREATE INDEX [IX_MultiSiteRevenues_PostId] ON [dbo].[MultiSiteRevenues]([PostId]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[PostKeywords]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PostKeywords](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_PostKeywords] PRIMARY KEY,
        [PostId] INT NOT NULL,
        [Keyword] NVARCHAR(200) NOT NULL,
        [KeywordType] NVARCHAR(20) NOT NULL,
        [SearchVolume] BIGINT NOT NULL,
        [Competition] NVARCHAR(20) NOT NULL,
        [SuggestedBidLow] DECIMAL(10,2) NOT NULL,
        [SuggestedBidHigh] DECIMAL(10,2) NOT NULL,
        [DifficultyScore] INT NOT NULL,
        [Priority] INT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL
    );

    ALTER TABLE [dbo].[PostKeywords] WITH CHECK
        ADD CONSTRAINT [FK_PostKeywords_Posts_PostId]
        FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts]([PostId]) ON DELETE CASCADE;

    CREATE INDEX [IX_PostKeywords_PostId] ON [dbo].[PostKeywords]([PostId]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[PostPerformanceMetrics]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PostPerformanceMetrics](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_PostPerformanceMetrics] PRIMARY KEY,
        [PostId] INT NOT NULL,
        [Date] DATE NOT NULL,
        [Views] INT NOT NULL,
        [UniqueVisitors] INT NOT NULL,
        [BounceRate] DECIMAL(5,2) NOT NULL,
        [AvgTimeOnPage] INT NOT NULL,
        [CommentCount] INT NOT NULL,
        [ShareCount] INT NOT NULL,
        [SearchImpressions] INT NOT NULL,
        [SearchClicks] INT NOT NULL,
        [SearchCTR] DECIMAL(5,2) NOT NULL,
        [AvgSearchPosition] DECIMAL(5,2) NOT NULL,
        [AdRevenue] DECIMAL(10,2) NOT NULL,
        [AdClicks] INT NOT NULL,
        [AdCTR] DECIMAL(5,2) NOT NULL,
        [CPC] DECIMAL(10,2) NOT NULL,
        [RPM] DECIMAL(10,2) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_PostPerformanceMetrics_CreatedAt] DEFAULT (GETDATE())
    );

    ALTER TABLE [dbo].[PostPerformanceMetrics] WITH CHECK
        ADD CONSTRAINT [FK_PostPerformanceMetrics_Posts_PostId]
        FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts]([PostId]) ON DELETE CASCADE;

    CREATE INDEX [IX_PostPerformanceMetrics_Date] ON [dbo].[PostPerformanceMetrics]([Date]);
    CREATE UNIQUE INDEX [IX_PostPerformanceMetrics_PostId_Date] ON [dbo].[PostPerformanceMetrics]([PostId], [Date]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[PostSeoQueues]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PostSeoQueues](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_PostSeoQueues] PRIMARY KEY,
        [PostId] INT NOT NULL,
        [Priority] INT NOT NULL,
        [Reason] NVARCHAR(200) NOT NULL,
        [EstimatedRevenueImpact] DECIMAL(10,2) NULL,
        [AddedAt] DATETIME2 NOT NULL CONSTRAINT [DF_PostSeoQueues_AddedAt] DEFAULT (GETDATE()),
        [ProcessedAt] DATETIME2 NULL,
        [Status] NVARCHAR(20) NOT NULL,
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [SuggestedChanges] NVARCHAR(MAX) NULL,
        [RequiresApproval] BIT NOT NULL
    );

    ALTER TABLE [dbo].[PostSeoQueues] WITH CHECK
        ADD CONSTRAINT [FK_PostSeoQueues_Posts_PostId]
        FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts]([PostId]) ON DELETE CASCADE;

    CREATE INDEX [IX_PostSeoQueues_PostId] ON [dbo].[PostSeoQueues]([PostId]);
    CREATE INDEX [IX_PostSeoQueues_Status_Priority] ON [dbo].[PostSeoQueues]([Status], [Priority]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[SeoOptimizationLogs]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SeoOptimizationLogs](
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_SeoOptimizationLogs] PRIMARY KEY,
        [PostId] INT NOT NULL,
        [OptimizedAt] DATETIME2 NOT NULL CONSTRAINT [DF_SeoOptimizationLogs_OptimizedAt] DEFAULT (GETDATE()),
        [ChangeType] NVARCHAR(50) NOT NULL,
        [OldValue] NVARCHAR(MAX) NULL,
        [NewValue] NVARCHAR(MAX) NULL,
        [Trigger] NVARCHAR(200) NOT NULL,
        [PerformanceBefore] NVARCHAR(MAX) NULL,
        [PerformanceAfter] NVARCHAR(MAX) NULL,
        [RevenueImpact] DECIMAL(10,2) NULL,
        [TrafficImpact] DECIMAL(10,2) NULL,
        [Status] NVARCHAR(20) NOT NULL,
        [IsApproved] BIT NOT NULL,
        [ApprovedBy] NVARCHAR(450) NULL,
        [ApprovedAt] DATETIME2 NULL
    );

    ALTER TABLE [dbo].[SeoOptimizationLogs] WITH CHECK
        ADD CONSTRAINT [FK_SeoOptimizationLogs_Posts_PostId]
        FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts]([PostId]) ON DELETE CASCADE;

    CREATE INDEX [IX_SeoOptimizationLogs_PostId_OptimizedAt] ON [dbo].[SeoOptimizationLogs]([PostId], [OptimizedAt]);
    CREATE INDEX [IX_SeoOptimizationLogs_Status] ON [dbo].[SeoOptimizationLogs]([Status]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[UserActivities]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserActivities](
        [Id] BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_UserActivities] PRIMARY KEY,
        [UserId] NVARCHAR(450) NULL,
        [SessionId] NVARCHAR(100) NULL,
        [PostId] INT NULL,
        [CommunityId] INT NULL,
        [ActivityType] NVARCHAR(50) NOT NULL,
        [TimeSpentSeconds] INT NOT NULL,
        [ScrollDepthPercent] INT NOT NULL,
        [Referrer] NVARCHAR(500) NULL,
        [DeviceType] NVARCHAR(200) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [ActivityAt] DATETIME2 NOT NULL CONSTRAINT [DF_UserActivities_ActivityAt] DEFAULT (GETDATE()),
        [Metadata] NVARCHAR(MAX) NULL
    );

    ALTER TABLE [dbo].[UserActivities] WITH CHECK
        ADD CONSTRAINT [FK_UserActivities_Posts_PostId]
        FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts]([PostId]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[UserActivities] WITH CHECK
        ADD CONSTRAINT [FK_UserActivities_Communities_CommunityId]
        FOREIGN KEY([CommunityId]) REFERENCES [dbo].[Communities]([CommunityId]) ON DELETE SET NULL;

    CREATE INDEX [IX_UserActivities_CommunityId] ON [dbo].[UserActivities]([CommunityId]);
    CREATE INDEX [IX_UserActivities_PostId_ActivityAt] ON [dbo].[UserActivities]([PostId], [ActivityAt]);
    CREATE INDEX [IX_UserActivities_SessionId] ON [dbo].[UserActivities]([SessionId]);
    CREATE INDEX [IX_UserActivities_UserId_ActivityAt] ON [dbo].[UserActivities]([UserId], [ActivityAt]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_PollConfigurations_AspNetUsers_ClosedByUserId'
      AND parent_object_id = OBJECT_ID(N'[dbo].[PollConfigurations]')
)
BEGIN
    ALTER TABLE [dbo].[PollConfigurations] WITH CHECK
        ADD CONSTRAINT [FK_PollConfigurations_AspNetUsers_ClosedByUserId]
        FOREIGN KEY([ClosedByUserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE NO ACTION;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PollConfigurations_AspNetUsers_ClosedByUserId",
                table: "PollConfigurations");

            migrationBuilder.DropTable(
                name: "AdSenseRevenues");

            migrationBuilder.DropTable(
                name: "CommentLinkPreviews");

            migrationBuilder.DropTable(
                name: "ContentRecommendations");

            migrationBuilder.DropTable(
                name: "EnhancedSeoMetadata");

            migrationBuilder.DropTable(
                name: "MultiSiteRevenues");

            migrationBuilder.DropTable(
                name: "PostKeywords");

            migrationBuilder.DropTable(
                name: "PostPerformanceMetrics");

            migrationBuilder.DropTable(
                name: "PostSeoQueues");

            migrationBuilder.DropTable(
                name: "SeoOptimizationLogs");

            migrationBuilder.DropTable(
                name: "UserActivities");

            migrationBuilder.DropIndex(
                name: "IX_PollConfigurations_ClosedByUserId",
                table: "PollConfigurations");

            migrationBuilder.DropColumn(
                name: "LongtailKeywords",
                table: "SeoMetadata");

            migrationBuilder.DropColumn(
                name: "PrimaryKeywords",
                table: "SeoMetadata");

            migrationBuilder.DropColumn(
                name: "SecondaryKeywords",
                table: "SeoMetadata");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "PollConfigurations");

            migrationBuilder.DropColumn(
                name: "ClosedByUserId",
                table: "PollConfigurations");

            migrationBuilder.DropColumn(
                name: "PollDescription",
                table: "PollConfigurations");

            migrationBuilder.DropColumn(
                name: "PollQuestion",
                table: "PollConfigurations");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SavedAt",
                table: "SavedPosts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Media",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Media",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Media",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_SavedPosts_UserId",
                table: "SavedPosts",
                column: "UserId");
        }
    }
}
