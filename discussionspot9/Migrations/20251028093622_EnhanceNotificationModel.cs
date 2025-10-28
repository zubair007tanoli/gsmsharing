using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace discussionspot9.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceNotificationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActorAvatarUrl",
                table: "Notifications",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActorDisplayName",
                table: "Notifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActorUserId",
                table: "Notifications",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailSent",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailSentAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupId",
                table: "Notifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Notifications",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmailQueues",
                columns: table => new
                {
                    EmailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ToName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HtmlBody = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    PlainTextBody = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MaxRetries = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledFor = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    NotificationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    EmailType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "notification"),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 5)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQueues", x => x.EmailId);
                    table.CheckConstraint("CK_EmailQueue_Priority", "Priority BETWEEN 1 AND 10");
                    table.CheckConstraint("CK_EmailQueue_Status", "Status IN ('pending', 'sent', 'failed', 'cancelled')");
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    PreferenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WebEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PushEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmailFrequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "instant"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.PreferenceId);
                    table.CheckConstraint("CK_NotificationPreference_EmailFrequency", "EmailFrequency IN ('instant', 'daily', 'weekly', 'never')");
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFollows",
                columns: table => new
                {
                    FollowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FollowerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FollowedId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FollowedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    NotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollows", x => x.FollowId);
                    table.CheckConstraint("CK_UserFollow_NoSelfFollow", "FollowerId != FollowedId");
                    table.ForeignKey(
                        name: "FK_UserFollows_AspNetUsers_FollowedId",
                        column: x => x.FollowedId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserFollows_AspNetUsers_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserNotificationSettings",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EmailNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    WebNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PushNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmailDigestFrequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "never"),
                    QuietHoursEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    QuietHoursStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    QuietHoursEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    GroupNotifications = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ShowNotificationPreviews = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PlayNotificationSound = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UnsubscribeFromAll = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastDigestSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationSettings", x => x.UserId);
                    table.CheckConstraint("CK_UserNotificationSettings_EmailDigestFrequency", "EmailDigestFrequency IN ('never', 'daily', 'weekly')");
                    table.ForeignKey(
                        name: "FK_UserNotificationSettings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ActorUserId",
                table: "Notifications",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_Type_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "Type", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_CreatedAt",
                table: "EmailQueues",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_Status",
                table: "EmailQueues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_Status_Priority_ScheduledFor",
                table: "EmailQueues",
                columns: new[] { "Status", "Priority", "ScheduledFor" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_UserId",
                table: "EmailQueues",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId",
                table: "NotificationPreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId_NotificationType",
                table: "NotificationPreferences",
                columns: new[] { "UserId", "NotificationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowedId",
                table: "UserFollows",
                column: "FollowedId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowedId_IsActive",
                table: "UserFollows",
                columns: new[] { "FollowedId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowerId",
                table: "UserFollows",
                column: "FollowerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowerId_FollowedId",
                table: "UserFollows",
                columns: new[] { "FollowerId", "FollowedId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_ActorUserId",
                table: "Notifications",
                column: "ActorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_ActorUserId",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "EmailQueues");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "UserFollows");

            migrationBuilder.DropTable(
                name: "UserNotificationSettings");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ActorUserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_Type_IsRead",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ActorAvatarUrl",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ActorDisplayName",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ActorUserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EmailSent",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EmailSentAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Notifications");
        }
    }
}
