using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace discussionspot9.Migrations
{
    public partial class AddBanAppealTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
IF OBJECT_ID(N'[dbo].[BanAppeals]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[BanAppeals] (
        [BanAppealId] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NOT NULL,
        [AppealMessage] nvarchar(2000) NOT NULL,
        [SubmittedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
        [ReviewedAt] datetime2 NULL,
        [ReviewedByUserId] nvarchar(450) NULL,
        [Status] int NOT NULL DEFAULT 0,
        [AdminResponse] nvarchar(500) NULL,
        CONSTRAINT [PK_BanAppeals] PRIMARY KEY ([BanAppealId]),
        CONSTRAINT [FK_BanAppeals_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_BanAppeals_AspNetUsers_ReviewedByUserId] FOREIGN KEY ([ReviewedByUserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE NO ACTION
    );
    
    CREATE INDEX [IX_BanAppeals_UserId] ON [BanAppeals]([UserId]);
    CREATE INDEX [IX_BanAppeals_Status] ON [BanAppeals]([Status]);
    CREATE INDEX [IX_BanAppeals_SubmittedAt] ON [BanAppeals]([SubmittedAt]);
END
";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"
IF OBJECT_ID(N'[dbo].[BanAppeals]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[BanAppeals];
END
";
            migrationBuilder.Sql(sql);
        }
    }
}
