
using discussionspot9.Data.DbContext;
using discussionspot9.Helpers;
using discussionspot9.Hubs;
using discussionspot9.Interfaces;
using discussionspot9.Services;
using DiscussionSpot9.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// PERFORMANCE OPTIMIZATIONS
// =============================================

// Add output caching for static content (disabled for now to fix CSS/JS loading)
// builder.Services.AddOutputCache(options =>
// {
//     options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromHours(1)));
//     options.AddPolicy("Posts", builder => builder.Expire(TimeSpan.FromMinutes(5)));
//     options.AddPolicy("Communities", builder => builder.Expire(TimeSpan.FromMinutes(15)));
//     options.AddPolicy("Categories", builder => builder.Expire(TimeSpan.FromHours(1)));
// });

// Add services to the container
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Add performance optimizations
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(30); // Reduce command timeout
        sqlOptions.EnableRetryOnFailure(3); // Retry failed commands
    });
    
    // Disable change tracking for read-only operations
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    
    // Configure warnings
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    
    // Enable sensitive data logging only in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add Google Authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        var googleAuthPath = Path.Combine(builder.Environment.WebRootPath, "GoogleApiAccess", "AuthKeys.json");
        if (File.Exists(googleAuthPath))
        {
            var jsonString = File.ReadAllText(googleAuthPath);
            var googleAuth = System.Text.Json.JsonSerializer.Deserialize<discussionspot9.Models.GoogleAuthConfig>(
                jsonString,
                new System.Text.Json.JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower
                });
            
            if (googleAuth?.Web != null)
            {
                options.ClientId = googleAuth.Web.ClientId ?? "";
                options.ClientSecret = googleAuth.Web.ClientSecret ?? "";
            }
        }
        else
        {
            // Fallback: try to get from configuration
            options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
            options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
        }
        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;
    });
// ADD THIS SECTION:
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "DiscussionSpot9Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Account/Auth";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ReturnUrlParameter = "returnUrl"; // Explicitly set return URL parameter name
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.IsEssential = true;
});

// ADD DATA PROTECTION:
builder.Services.AddDataProtection()
    .SetApplicationName("DiscussionSpot9");

// Response Compression for faster page loads
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "image/svg+xml", "application/json", "text/css", "application/javascript" });
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.AddControllersWithViews();

// Add memory cache for performance
builder.Services.AddMemoryCache(options =>
{
    options.CompactionPercentage = 0.25; // Compact when 25% full
});

// Add Response Caching
builder.Services.AddResponseCaching();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

//Interfaces & Repositories
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddHttpClient<ILinkMetadataService, LinkMetadataService>();
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
builder.Services.AddScoped<IPostTest, PostTest>();
builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<ISeoAnalyzerService, PythonSeoAnalyzerService>();

// Add Performance Services (simplified)
// builder.Services.AddScoped<PerformanceOptimizationService>(); // Commented out for now

// Add Semrush services
builder.Services.Configure<discussionspot9.Models.Semrush.SemrushConfig>(
    builder.Configuration.GetSection("Semrush"));
builder.Services.AddHttpClient<discussionspot9.Services.SemrushService>();
builder.Services.AddScoped<discussionspot9.Interfaces.ISemrushService, discussionspot9.Services.SemrushService>();
builder.Services.AddScoped<discussionspot9.Services.EnhancedSeoService>();
builder.Services.AddSingleton<IBackgroundSeoService, BackgroundSeoService>(); // Singleton for background tasks

// SEO & Revenue Optimization Services
builder.Services.AddScoped<GoogleAdSenseService>();
builder.Services.AddScoped<GoogleSearchConsoleService>();
builder.Services.AddScoped<SmartPostSelectorService>();
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<EnhancedHomeService>();

// Enhanced SEO & Multi-Site Revenue Services
builder.Services.Configure<discussionspot9.Models.Configuration.AdSenseConfiguration>(
    builder.Configuration.GetSection("GoogleAdSense"));
builder.Services.Configure<discussionspot9.Models.Configuration.GoogleAdsConfiguration>(
    builder.Configuration.GetSection("GoogleAds"));
builder.Services.AddScoped<MultiSiteAdSenseService>();
builder.Services.AddScoped<GoogleKeywordPlannerService>();
builder.Services.AddScoped<EnhancedSeoService>();

// Background services
builder.Services.AddHostedService<WeeklySeoOptimizationService>();
builder.Services.AddHostedService<DailyDataSyncService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();

// Performance monitoring middleware (temporarily disabled to test static files)
// app.Use(async (context, next) =>
// {
//     var stopwatch = System.Diagnostics.Stopwatch.StartNew();
//     
//     await next();
//     
//     stopwatch.Stop();
//     
//     // Log slow requests
//     if (stopwatch.ElapsedMilliseconds > 1000)
//     {
//         var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//         logger.LogWarning("Slow request: {Method} {Path} took {ElapsedMs}ms", 
//             context.Request.Method, 
//             context.Request.Path, 
//             stopwatch.ElapsedMilliseconds);
//     }
//     
//     // Add performance headers
//     context.Response.Headers.Add("X-Response-Time", $"{stopwatch.ElapsedMilliseconds}ms");
// });

// Enable Response Compression (temporarily disabled to test static files)
// app.UseResponseCompression();

// Enable Response Caching (temporarily disabled to test static files)
// app.UseResponseCaching();

app.UseHttpsRedirection();

// Static files (simplified configuration)
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map SignalR hubs AFTER routing and authentication
app.MapHub<PostHub>("/posthub");
app.MapHub<NotificationHub>("/notificationHub");

app.MapRazorPages();
// Authentication Routes
app.MapControllerRoute(
    name: "auth_combined",
    pattern: "auth",
    defaults: new { controller = "Account", action = "Auth" });

app.MapControllerRoute(
    name: "auth_login",
    pattern: "login",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "auth_register",
    pattern: "register",
    defaults: new { controller = "Account", action = "Register" });

app.MapControllerRoute(
    name: "auth_logout",
    pattern: "logout",
    defaults: new { controller = "Account", action = "Logout" });

// Email Confirmation Routes
app.MapControllerRoute(
    name: "email_confirm",
    pattern: "confirm-email",
    defaults: new { controller = "Account", action = "ConfirmEmail" });

// Password Management Routes
app.MapControllerRoute(
    name: "password_reset",
    pattern: "reset-password",
    defaults: new { controller = "Account", action = "ResetPassword" });

app.MapControllerRoute(
    name: "password_new",
    pattern: "new-password",
    defaults: new { controller = "Account", action = "NewPassword" });

// Account Settings Routes (for authenticated users)
app.MapControllerRoute(
    name: "account_profile",
    pattern: "account/profile",
    defaults: new { controller = "Account", action = "Profile" });

app.MapControllerRoute(
    name: "account_settings",
    pattern: "account/settings",
    defaults: new { controller = "Account", action = "Profile" });

app.MapControllerRoute(
    name: "account_password",
    pattern: "account/change-password",
    defaults: new { controller = "Account", action = "ChangePassword" });

// Public User Profile Routes
app.MapControllerRoute(
    name: "user_profile",
    pattern: "u/{displayName}",
    defaults: new { controller = "Account", action = "ViewUser" },
    constraints: new { displayName = @"^[a-zA-Z0-9_-]+$" });

app.MapControllerRoute(
    name: "user_posts",
    pattern: "u/{displayName}/posts",
    defaults: new { controller = "Account", action = "UserPosts" },
    constraints: new { displayName = @"^[a-zA-Z0-9_-]+$" });

app.MapControllerRoute(
    name: "user_comments",
    pattern: "u/{displayName}/comments",
    defaults: new { controller = "Account", action = "UserComments" },
    constraints: new { displayName = @"^[a-zA-Z0-9_-]+$" });

// AJAX/API Routes
app.MapControllerRoute(
    name: "ajax_check_displayname",
    pattern: "ajax/account/check-displayname",
    defaults: new { controller = "Account", action = "CheckDisplayName" });

// Access Denied
app.MapControllerRoute(
    name: "access_denied",
    pattern: "access-denied",
    defaults: new { controller = "Account", action = "AccessDenied" });

// Community Routes (existing)
app.MapControllerRoute(
    name: "community_posts",
    pattern: "r/{communitySlug}/posts/{postSlug}",
    defaults: new { controller = "Post", action = "Details" },
    constraints: new { communitySlug = @"^[a-zA-Z0-9_-]+$", postSlug = @"^[a-zA-Z0-9_-]+$" });

app.MapControllerRoute(
    name: "community_detail",
    pattern: "r/{slug}",
    defaults: new { controller = "Community", action = "Details" },
    constraints: new { slug = @"^[a-zA-Z0-9_-]+$" });

app.MapControllerRoute(
    name: "community_create",
    pattern: "create-community",
    defaults: new { controller = "Community", action = "Create" });

app.MapControllerRoute(
    name: "post_create",
    pattern: "r/{communitySlug}/create",
    defaults: new { controller = "Post", action = "Create" },
    constraints: new { communitySlug = @"^[a-zA-Z0-9_-]+$" });

app.MapControllerRoute(
    name: "communities_list",
    pattern: "communities",
    defaults: new { controller = "Community", action = "Index" });

// Search Route
app.MapControllerRoute(
    name: "search",
    pattern: "search",
    defaults: new { controller = "Search", action = "Index" });

// Home Page Alternatives
app.MapControllerRoute(
    name: "home_popular",
    pattern: "popular",
    defaults: new { controller = "Home", action = "Popular" });

app.MapControllerRoute(
    name: "home_all",
    pattern: "all",
    defaults: new { controller = "Home", action = "All" });

// Admin Routes
app.MapControllerRoute(
    name: "admin_seo",
    pattern: "admin/seo/{action=Dashboard}/{id?}",
    defaults: new { controller = "SeoAdmin" });

app.MapControllerRoute(
    name: "admin_management",
    pattern: "admin/manage/{action=Users}/{id?}",
    defaults: new { controller = "AdminManagement" });

// API Routes
app.MapControllerRoute(
    name: "api_routes",
    pattern: "api/{controller}/{action}/{id?}");

app.MapControllerRoute(
    name: "post_create_general",
    pattern: "create",
    defaults: new { controller = "Post", action = "CreateTest" });

// Default fallback route (keep this last)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();