using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace discussionspot9.Migrations
{
    /// <inheritdoc />
    public partial class SyncStoriesSchemaForAmp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // BackgroundAudioUrl and PostId columns removed - they don't exist in database
            // migrationBuilder.AddColumn<string>(
            //     name: "BackgroundAudioUrl",
            //     table: "Stories",
            //     type: "nvarchar(max)",
            //     nullable: true);

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Stories', 'PostId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Stories] ADD [PostId] INT NULL;
END
");

            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    BadgeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rarity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.BadgeId);
                });

            migrationBuilder.CreateTable(
                name: "StoryAnalytics",
                columns: table => new
                {
                    StoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    UniqueViewCount = table.Column<int>(type: "int", nullable: false),
                    CompletionCount = table.Column<int>(type: "int", nullable: false),
                    ShareCount = table.Column<int>(type: "int", nullable: false),
                    ReactionCount = table.Column<int>(type: "int", nullable: false),
                    AverageViewDuration = table.Column<int>(type: "int", nullable: false),
                    CompletionRate = table.Column<double>(type: "float", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    LoveCount = table.Column<int>(type: "int", nullable: false),
                    WowCount = table.Column<int>(type: "int", nullable: false),
                    SadCount = table.Column<int>(type: "int", nullable: false),
                    LaughCount = table.Column<int>(type: "int", nullable: false),
                    StoryId1 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryAnalytics", x => x.StoryId);
                    table.ForeignKey(
                        name: "FK_StoryAnalytics_Stories_StoryId1",
                        column: x => x.StoryId1,
                        principalTable: "Stories",
                        principalColumn: "StoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoryReactions",
                columns: table => new
                {
                    StoryReactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoryId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReactionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryReactions", x => x.StoryReactionId);
                    table.ForeignKey(
                        name: "FK_StoryReactions_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "StoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoryShares",
                columns: table => new
                {
                    StoryShareId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoryId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SharedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryShares", x => x.StoryShareId);
                    table.ForeignKey(
                        name: "FK_StoryShares_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "StoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoryViews",
                columns: table => new
                {
                    StoryViewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoryId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SlidesViewed = table.Column<int>(type: "int", nullable: false),
                    TimeSpent = table.Column<int>(type: "int", nullable: false),
                    Completed = table.Column<bool>(type: "bit", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferrerUrl = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryViews", x => x.StoryViewId);
                    table.ForeignKey(
                        name: "FK_StoryViews_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "StoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BadgeRequirements",
                columns: table => new
                {
                    BadgeRequirementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BadgeId = table.Column<int>(type: "int", nullable: false),
                    RequirementType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequiredValue = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeRequirements", x => x.BadgeRequirementId);
                    table.ForeignKey(
                        name: "FK_BadgeRequirements_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "BadgeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBadges",
                columns: table => new
                {
                    UserBadgeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BadgeId = table.Column<int>(type: "int", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EarnedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDisplayed = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsNotified = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBadges", x => x.UserBadgeId);
                    table.ForeignKey(
                        name: "FK_UserBadges_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "BadgeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Stories', 'PostId') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.indexes
       WHERE name = 'IX_Stories_PostId'
         AND object_id = OBJECT_ID(N'[dbo].[Stories]')
   )
BEGIN
    CREATE INDEX [IX_Stories_PostId] ON [dbo].[Stories]([PostId]);
END
");

            migrationBuilder.CreateIndex(
                name: "IX_BadgeRequirements_BadgeId",
                table: "BadgeRequirements",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryAnalytics_StoryId1",
                table: "StoryAnalytics",
                column: "StoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_StoryReactions_StoryId",
                table: "StoryReactions",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryShares_StoryId",
                table: "StoryShares",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryViews_StoryId",
                table: "StoryViews",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_BadgeId",
                table: "UserBadges",
                column: "BadgeId");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Stories', 'PostId') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.foreign_keys
       WHERE name = 'FK_Stories_Posts_PostId'
         AND parent_object_id = OBJECT_ID(N'[dbo].[Stories]')
   )
BEGIN
    ALTER TABLE [dbo].[Stories] WITH CHECK
        ADD CONSTRAINT [FK_Stories_Posts_PostId]
        FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts]([PostId]) ON DELETE SET NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // PostId foreign key removal - column doesn't exist
            // migrationBuilder.DropForeignKey(
            //     name: "FK_Stories_Posts_PostId",
            //     table: "Stories");

            migrationBuilder.DropTable(
                name: "BadgeRequirements");

            migrationBuilder.DropTable(
                name: "StoryAnalytics");

            migrationBuilder.DropTable(
                name: "StoryReactions");

            migrationBuilder.DropTable(
                name: "StoryShares");

            migrationBuilder.DropTable(
                name: "StoryViews");

            migrationBuilder.DropTable(
                name: "UserBadges");

            migrationBuilder.DropTable(
                name: "Badges");

            // PostId index removal - column doesn't exist
            // migrationBuilder.DropIndex(
            //     name: "IX_Stories_PostId",
            //     table: "Stories");

            // BackgroundAudioUrl and PostId column removal - they don't exist
            // migrationBuilder.DropColumn(
            //     name: "BackgroundAudioUrl",
            //     table: "Stories");

            // migrationBuilder.DropColumn(
            //     name: "PostId",
            //     table: "Stories");
        }
    }
}
