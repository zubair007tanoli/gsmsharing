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
            migrationBuilder.DropPrimaryKey(
                name: "PK_SavedPosts",
                table: "SavedPosts");

            migrationBuilder.DropIndex(
                name: "IX_SavedPosts_UserId",
                table: "SavedPosts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Media");

            migrationBuilder.AddColumn<string>(
                name: "LongtailKeywords",
                table: "SeoMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryKeywords",
                table: "SeoMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryKeywords",
                table: "SeoMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SavedAt",
                table: "SavedPosts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "SavedPostId",
                table: "SavedPosts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "PollConfigurations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosedByUserId",
                table: "PollConfigurations",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PollDescription",
                table: "PollConfigurations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PollQuestion",
                table: "PollConfigurations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SavedPosts",
                table: "SavedPosts",
                columns: new[] { "UserId", "PostId" });

            migrationBuilder.CreateTable(
                name: "AdSenseRevenues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    PostId = table.Column<int>(type: "int", nullable: true),
                    Earnings = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    EstimatedEarnings = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PageViews = table.Column<int>(type: "int", nullable: false),
                    AdClicks = table.Column<int>(type: "int", nullable: false),
                    CTR = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CPC = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    RPM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AdImpressions = table.Column<int>(type: "int", nullable: false),
                    ActiveViewViewableImpressions = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Coverage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdSenseRevenues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdSenseRevenues_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentLinkPreviews",
                columns: table => new
                {
                    CommentLinkPreviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    FaviconUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastFetchedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FetchSucceeded = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentLinkPreviews", x => x.CommentLinkPreviewId);
                    table.ForeignKey(
                        name: "FK_CommentLinkPreviews_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentRecommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecommendationType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    RelatedPostId = table.Column<int>(type: "int", nullable: true),
                    CommunityId = table.Column<int>(type: "int", nullable: true),
                    EstimatedRevenueImpact = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    EstimatedTrafficImpact = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    AnalysisData = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ImplementedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImplementedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentRecommendations_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "CommunityId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContentRecommendations_Posts_RelatedPostId",
                        column: x => x.RelatedPostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EnhancedSeoMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    OriginalMetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OptimizedMetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PredictedCtrImprovement = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EmotionalTriggers = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PowerWords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrimaryKeywords = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SecondaryKeywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LsiKeywords = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TotalSearchVolume = table.Column<long>(type: "bigint", nullable: false),
                    ReadabilityScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SeoScore = table.Column<int>(type: "int", nullable: false),
                    KeywordDensity = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CompetitorAnalysis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SerpPreview = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancedSeoMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnhancedSeoMetadata_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiSiteRevenues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteDomain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostId = table.Column<int>(type: "int", nullable: true),
                    PostUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Earnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PageViews = table.Column<int>(type: "int", nullable: false),
                    AdClicks = table.Column<int>(type: "int", nullable: false),
                    CTR = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CPC = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    RPM = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AdImpressions = table.Column<int>(type: "int", nullable: false),
                    ActiveViewViewableImpressions = table.Column<int>(type: "int", nullable: false),
                    Coverage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiSiteRevenues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiSiteRevenues_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId");
                });

            migrationBuilder.CreateTable(
                name: "PostKeywords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    Keyword = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    KeywordType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SearchVolume = table.Column<long>(type: "bigint", nullable: false),
                    Competition = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SuggestedBidLow = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SuggestedBidHigh = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DifficultyScore = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostKeywords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostKeywords_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostPerformanceMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Views = table.Column<int>(type: "int", nullable: false),
                    UniqueVisitors = table.Column<int>(type: "int", nullable: false),
                    BounceRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AvgTimeOnPage = table.Column<int>(type: "int", nullable: false),
                    CommentCount = table.Column<int>(type: "int", nullable: false),
                    ShareCount = table.Column<int>(type: "int", nullable: false),
                    SearchImpressions = table.Column<int>(type: "int", nullable: false),
                    SearchClicks = table.Column<int>(type: "int", nullable: false),
                    SearchCTR = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AvgSearchPosition = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AdRevenue = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AdClicks = table.Column<int>(type: "int", nullable: false),
                    AdCTR = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CPC = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    RPM = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostPerformanceMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostPerformanceMetrics_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostSeoQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EstimatedRevenueImpact = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    SuggestedChanges = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostSeoQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostSeoQueues_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeoOptimizationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    OptimizedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ChangeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OldValue = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    NewValue = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    Trigger = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PerformanceBefore = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    PerformanceAfter = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    RevenueImpact = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TrafficImpact = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeoOptimizationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeoOptimizationLogs_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostId = table.Column<int>(type: "int", nullable: true),
                    CommunityId = table.Column<int>(type: "int", nullable: true),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TimeSpentSeconds = table.Column<int>(type: "int", nullable: false),
                    ScrollDepthPercent = table.Column<int>(type: "int", nullable: false),
                    Referrer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActivityAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Metadata = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivities_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "CommunityId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserActivities_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PollConfigurations_ClosedByUserId",
                table: "PollConfigurations",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdSenseRevenues_Date_PostId",
                table: "AdSenseRevenues",
                columns: new[] { "Date", "PostId" });

            migrationBuilder.CreateIndex(
                name: "IX_AdSenseRevenues_PostId",
                table: "AdSenseRevenues",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLinkPreviews_CommentId",
                table: "CommentLinkPreviews",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLinkPreviews_Url",
                table: "CommentLinkPreviews",
                column: "Url");

            migrationBuilder.CreateIndex(
                name: "IX_ContentRecommendations_CommunityId",
                table: "ContentRecommendations",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentRecommendations_RelatedPostId",
                table: "ContentRecommendations",
                column: "RelatedPostId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentRecommendations_Status_Priority",
                table: "ContentRecommendations",
                columns: new[] { "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_EnhancedSeoMetadata_PostId",
                table: "EnhancedSeoMetadata",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiSiteRevenues_PostId",
                table: "MultiSiteRevenues",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostKeywords_PostId",
                table: "PostKeywords",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostPerformanceMetrics_Date",
                table: "PostPerformanceMetrics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_PostPerformanceMetrics_PostId_Date",
                table: "PostPerformanceMetrics",
                columns: new[] { "PostId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostSeoQueues_PostId",
                table: "PostSeoQueues",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostSeoQueues_Status_Priority",
                table: "PostSeoQueues",
                columns: new[] { "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_SeoOptimizationLogs_PostId_OptimizedAt",
                table: "SeoOptimizationLogs",
                columns: new[] { "PostId", "OptimizedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SeoOptimizationLogs_Status",
                table: "SeoOptimizationLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_CommunityId",
                table: "UserActivities",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_PostId_ActivityAt",
                table: "UserActivities",
                columns: new[] { "PostId", "ActivityAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_SessionId",
                table: "UserActivities",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_UserId_ActivityAt",
                table: "UserActivities",
                columns: new[] { "UserId", "ActivityAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_PollConfigurations_AspNetUsers_ClosedByUserId",
                table: "PollConfigurations",
                column: "ClosedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_SavedPosts",
                table: "SavedPosts");

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

            migrationBuilder.AlterColumn<int>(
                name: "SavedPostId",
                table: "SavedPosts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_SavedPosts",
                table: "SavedPosts",
                column: "SavedPostId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedPosts_UserId",
                table: "SavedPosts",
                column: "UserId");
        }
    }
}
