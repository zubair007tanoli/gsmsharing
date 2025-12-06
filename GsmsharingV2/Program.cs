using GsmsharingV2.Database;
using GsmsharingV2.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GsmsharingConnection")));

// ASP.NET Core Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie();

builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/UserAccounts/Login";
    options.AccessDeniedPath = "/UserAccounts/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register Repositories
builder.Services.AddScoped<GsmsharingV2.Interfaces.IPostRepository, GsmsharingV2.Repositories.PostRepository>();
builder.Services.AddScoped<GsmsharingV2.Interfaces.ICommunityRepository, GsmsharingV2.Repositories.CommunityRepository>();
builder.Services.AddScoped<GsmsharingV2.Interfaces.ICategoryRepository, GsmsharingV2.Repositories.CategoryRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Custom route for Reddit-style post URLs
app.MapControllerRoute(
    name: "postBySlug",
    pattern: "r/{community}/posts/{slug}",
    defaults: new { controller = "Posts", action = "Details" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Ensure database is created BEFORE app starts
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("=== Database Initialization Started ===");
        
        // First, ensure database exists
        var canConnect = await dbContext.Database.CanConnectAsync();
        logger.LogInformation("Database connection status: {CanConnect}", canConnect);
        
        if (!canConnect)
        {
            logger.LogInformation("Database does not exist. Creating database and all tables...");
            await dbContext.Database.EnsureCreatedAsync();
            logger.LogInformation("✅ Database and all tables created successfully!");
        }
        else
        {
            logger.LogInformation("Database exists. Checking and creating missing tables...");
            
            // List of all tables that need to be created
            var tablesToCreate = new Dictionary<string, string>
            {
                ["Posts"] = @"
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
                    )",
                ["Communities"] = @"
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
                    )",
                ["Comments"] = @"
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
                    )",
                ["Categories"] = @"
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
                    )",
                ["Tags"] = @"
                    CREATE TABLE [dbo].[Tags](
                        [TagID] [int] IDENTITY(1,1) NOT NULL,
                        [Name] [nvarchar](max) NULL,
                        [Slug] [nvarchar](max) NULL,
                        [Description] [nvarchar](max) NULL,
                        [CreatedAt] [datetime2](7) NULL,
                        [CreatedBy] [nvarchar](max) NULL,
                        CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED ([TagID] ASC)
                    )",
                ["PostTags"] = @"
                    CREATE TABLE [dbo].[PostTags](
                        [PostID] [int] NOT NULL,
                        [TagID] [int] NOT NULL,
                        CONSTRAINT [PK_PostTags] PRIMARY KEY CLUSTERED ([PostID] ASC, [TagID] ASC)
                    )",
                ["Reactions"] = @"
                    CREATE TABLE [dbo].[Reactions](
                        [ReactionID] [int] IDENTITY(1,1) NOT NULL,
                        [UserId] [nvarchar](450) NULL,
                        [PostID] [int] NULL,
                        [CommentID] [int] NULL,
                        [ReactionType] [nvarchar](50) NULL,
                        [CreatedAt] [datetime2](7) NULL,
                        CONSTRAINT [PK_Reactions] PRIMARY KEY CLUSTERED ([ReactionID] ASC)
                    )",
                ["UserProfiles"] = @"
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
                    )",
                ["CommunityMembers"] = @"
                    CREATE TABLE [dbo].[CommunityMembers](
                        [CommunityMemberID] [int] IDENTITY(1,1) NOT NULL,
                        [CommunityID] [int] NOT NULL,
                        [UserId] [nvarchar](450) NULL,
                        [Role] [nvarchar](50) NULL,
                        [JoinedAt] [datetime2](7) NULL,
                        CONSTRAINT [PK_CommunityMembers] PRIMARY KEY CLUSTERED ([CommunityMemberID] ASC)
                    )",
                ["ChatRooms"] = @"
                    CREATE TABLE [dbo].[ChatRooms](
                        [RoomID] [int] IDENTITY(1,1) NOT NULL,
                        [RoomType] [nvarchar](50) NULL,
                        [CommunityID] [int] NULL,
                        [Name] [nvarchar](max) NULL,
                        [CreatedAt] [datetime2](7) NULL,
                        [UpdatedAt] [datetime2](7) NULL,
                        [CreatedBy] [nvarchar](450) NULL,
                        CONSTRAINT [PK_ChatRooms] PRIMARY KEY CLUSTERED ([RoomID] ASC)
                    )",
                ["ChatRoomMembers"] = @"
                    CREATE TABLE [dbo].[ChatRoomMembers](
                        [RoomID] [int] NOT NULL,
                        [UserId] [nvarchar](450) NOT NULL,
                        [JoinedAt] [datetime2](7) NULL,
                        [LastReadAt] [datetime2](7) NULL,
                        CONSTRAINT [PK_ChatRoomMembers] PRIMARY KEY CLUSTERED ([RoomID] ASC, [UserId] ASC)
                    )",
                ["Notifications"] = @"
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
                    )"
            };
            
            // Check and create each table
            foreach (var table in tablesToCreate)
            {
                try
                {
                    // Try to query the table to see if it exists
                    await dbContext.Database.ExecuteSqlRawAsync($"SELECT TOP 1 * FROM {table.Key}");
                    logger.LogInformation("✅ {Table} table exists.", table.Key);
                }
                catch (Exception)
                {
                    // Table doesn't exist, create it
                    logger.LogInformation("Creating {Table} table...", table.Key);
                    try
                    {
                        await dbContext.Database.ExecuteSqlRawAsync(table.Value);
                        logger.LogInformation("✅ {Table} table created successfully!", table.Key);
                    }
                    catch (Exception createEx)
                    {
                        // If error is "already exists", that's fine
                        if (createEx.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
                            createEx.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                            createEx.Message.Contains("There is already an object", StringComparison.OrdinalIgnoreCase))
                        {
                            logger.LogInformation("ℹ️ {Table} table already exists.", table.Key);
                        }
                        else
                        {
                            logger.LogError("❌ Failed to create {Table} table: {Message}", table.Key, createEx.Message);
                        }
                    }
                }
            }
        }
        
        logger.LogInformation("=== Database Initialization Completed ===");
    }
}
catch (Exception ex)
{
    // Use console as fallback if logging isn't available yet
    Console.WriteLine($"❌ CRITICAL: Error initializing database: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    // Don't throw - let the app start so user can see the error in the UI
}

app.Run();
