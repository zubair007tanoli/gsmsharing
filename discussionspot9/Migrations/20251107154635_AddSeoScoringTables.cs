using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace discussionspot9.Migrations
{
    /// <inheritdoc />
    public partial class AddSeoScoringTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeoScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Critical"),
                    GoogleCompetitivenessScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ContentQualityScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MetaCompletenessScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FreshnessScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Issues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecommendedKeywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TopCompetitors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriorityRank = table.Column<int>(type: "int", nullable: false),
                    ScoredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Hybrid")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeoScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeoScores_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeoOptimizationProposals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    ProposedTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProposedContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProposedMetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProposedKeywords = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChangesSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedScoreDelta = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CurrentScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ExpectedScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ReviewedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Hybrid")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeoOptimizationProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeoOptimizationProposals_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeoScores_PostId",
                table: "SeoScores",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_SeoOptimizationProposals_PostId",
                table: "SeoOptimizationProposals",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_SeoScores_Tier",
                table: "SeoScores",
                column: "Tier");

            migrationBuilder.CreateIndex(
                name: "IX_SeoOptimizationProposals_Status",
                table: "SeoOptimizationProposals",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeoScores");

            migrationBuilder.DropTable(
                name: "SeoOptimizationProposals");
        }
    }
}

