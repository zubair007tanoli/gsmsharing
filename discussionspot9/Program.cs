
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using discussionspot9.Data.DbContext;
using discussionspot9.Helpers;
using discussionspot9.Hubs;
using discussionspot9.Interfaces;
using discussionspot9.Services;
using discussionspot9.Repositories;
using discussionspot9.Middleware;
using DiscussionSpot9.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var primaryConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(primaryConnectionString))
{
    primaryConnectionString = builder.Configuration.GetConnectionString("LocalDB");
}

if (string.IsNullOrWhiteSpace(primaryConnectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured. Set ConnectionStrings:DefaultConnection via configuration or environment variables.");
}

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
// CRITICAL FIX: Use PooledDbContextFactory to avoid service lifetime conflicts
builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
{
    // Add performance optimizations with remote server resilience
    options.UseSqlServer(primaryConnectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(120); // Increased for remote server
        // Enhanced retry on transient failures (SSL errors, network issues, connection drops)
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5, // Increased from 3 to 5
            maxRetryDelay: TimeSpan.FromSeconds(60), // Increased from 30 to 60
            errorNumbersToAdd: new[] { -2, 2, 53, 121, 233 }); // Add specific error codes for connection issues
        sqlOptions.MigrationsAssembly("discussionspot9");
    });
    
    // CRITICAL FIX: Changed from NoTracking to TrackAll
    // NoTracking was breaking poll voting and other write operations
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    
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

// Register scoped DbContext for services that inject ApplicationDbContext directly
builder.Services.AddScoped<ApplicationDbContext>(serviceProvider =>
{
    var factory = serviceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    return factory.CreateDbContext();
});

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

string? missingGoogleAuthPath = null;
var googleCredentialWarnings = new List<string>();

// Determine Google OAuth credentials (supports Secrets/AuthKeys.json, wwwroot/GoogleApiAccess/AuthKeys.json, configuration, or environment variables)
var defaultGoogleSecretsPath = Path.Combine(builder.Environment.ContentRootPath, "Secrets", "AuthKeys.json");
var configuredGoogleSecretsPath = builder.Configuration["Authentication:Google:CredentialsPath"];
var envGoogleSecretsPath = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CREDENTIALS_PATH");
if (!string.IsNullOrWhiteSpace(envGoogleSecretsPath))
{
    configuredGoogleSecretsPath = envGoogleSecretsPath;
}

var resolvedGoogleSecretsPath = string.IsNullOrWhiteSpace(configuredGoogleSecretsPath)
    ? defaultGoogleSecretsPath
    : (Path.IsPathRooted(configuredGoogleSecretsPath)
        ? configuredGoogleSecretsPath
        : Path.Combine(builder.Environment.ContentRootPath, configuredGoogleSecretsPath));

var wwwrootGoogleSecretsPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "GoogleApiAccess", "AuthKeys.json");

string? googleClientId = FirstNonEmpty(
    builder.Configuration["Authentication:Google:ClientId"],
    Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID"));

string? googleClientSecret = FirstNonEmpty(
    builder.Configuration["Authentication:Google:ClientSecret"],
    Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET"));

var googleCredentialsJson = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CREDENTIALS_JSON");
if (string.IsNullOrWhiteSpace(googleCredentialsJson))
{
    var googleCredentialsBase64 = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CREDENTIALS_BASE64");
    if (!string.IsNullOrWhiteSpace(googleCredentialsBase64))
    {
        try
        {
            googleCredentialsJson = Encoding.UTF8.GetString(Convert.FromBase64String(googleCredentialsBase64));
        }
        catch (FormatException ex)
        {
            googleCredentialWarnings.Add($"GOOGLE_OAUTH_CREDENTIALS_BASE64 is not valid base64: {ex.Message}");
        }
    }
}

if (!string.IsNullOrWhiteSpace(googleCredentialsJson))
{
    try
    {
        using var doc = JsonDocument.Parse(googleCredentialsJson);
        if (doc.RootElement.TryGetProperty("web", out var webElement))
        {
            var candidateClientId = webElement.TryGetProperty("client_id", out var clientIdElement)
                ? clientIdElement.GetString()
                : webElement.TryGetProperty("clientId", out var clientIdCamelElement)
                    ? clientIdCamelElement.GetString()
                    : null;

            var candidateClientSecret = webElement.TryGetProperty("client_secret", out var clientSecretElement)
                ? clientSecretElement.GetString()
                : webElement.TryGetProperty("clientSecret", out var clientSecretCamelElement)
                    ? clientSecretCamelElement.GetString()
                    : null;

            if (!string.IsNullOrWhiteSpace(candidateClientId) &&
                !string.IsNullOrWhiteSpace(candidateClientSecret))
            {
                googleClientId ??= candidateClientId;
                googleClientSecret ??= candidateClientSecret;
                missingGoogleAuthPath = null;
            }
        }
    }
    catch (JsonException ex)
    {
        googleCredentialWarnings.Add($"GOOGLE_OAUTH_CREDENTIALS_JSON contains invalid JSON: {ex.Message}");
    }
}

var candidatePaths = new[]
{
    resolvedGoogleSecretsPath,
    wwwrootGoogleSecretsPath
}.Where(path => !string.IsNullOrWhiteSpace(path)).Distinct();

foreach (var candidatePath in candidatePaths)
{
    if (!File.Exists(candidatePath))
    {
        continue;
    }

    try
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(candidatePath));

        if (doc.RootElement.TryGetProperty("web", out var webElement))
        {
            var candidateClientId = webElement.TryGetProperty("client_id", out var clientIdElement)
                ? clientIdElement.GetString()
                : webElement.TryGetProperty("clientId", out var clientIdCamelElement)
                    ? clientIdCamelElement.GetString()
                    : null;

            var candidateClientSecret = webElement.TryGetProperty("client_secret", out var clientSecretElement)
                ? clientSecretElement.GetString()
                : webElement.TryGetProperty("clientSecret", out var clientSecretCamelElement)
                    ? clientSecretCamelElement.GetString()
                    : null;

            if (!string.IsNullOrWhiteSpace(candidateClientId) &&
                !string.IsNullOrWhiteSpace(candidateClientSecret))
            {
                googleClientId ??= candidateClientId;
                googleClientSecret ??= candidateClientSecret;
                missingGoogleAuthPath = null;
                break;
            }
        }
    }
    catch (JsonException ex)
    {
        googleCredentialWarnings.Add($"Failed to parse Google OAuth credentials at {candidatePath}: {ex.Message}");
        googleClientId = null;
        googleClientSecret = null;
    }
}

if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
{
    googleClientId = FirstNonEmpty(
        googleClientId,
        builder.Configuration["Authentication:Google:ClientId"],
        Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID"));

    googleClientSecret = FirstNonEmpty(
        googleClientSecret,
        builder.Configuration["Authentication:Google:ClientSecret"],
        Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET"));

    if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
    {
        missingGoogleAuthPath ??= resolvedGoogleSecretsPath;
    }
}

var authenticationBuilder = builder.Services.AddAuthentication();

if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authenticationBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId!;
        options.ClientSecret = googleClientSecret!;
        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;
    });
}
else if (string.IsNullOrWhiteSpace(missingGoogleAuthPath))
{
    missingGoogleAuthPath = resolvedGoogleSecretsPath;
}

builder.Services.AddSingleton(new discussionspot9.Models.Configuration.GoogleOAuthRuntimeState(
    googleClientId,
    googleClientSecret,
    missingGoogleAuthPath));
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
builder.Services.AddScoped<LiveStatsService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddHttpClient<ILinkMetadataService, LinkMetadataService>();
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<IKarmaService, KarmaService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();

// Email & Notification Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
builder.Services.AddHostedService<EmailWorkerService>();

// Story Generation Services
builder.Services.AddScoped<PythonStoryEnhancerService>();
builder.Services.AddScoped<IStoryGenerationService, StoryGenerationService>();
builder.Services.AddScoped<IStoryEngagementService, StoryEngagementService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();

// Media Upload Service
builder.Services.AddScoped<IMediaUploadService, MediaUploadService>();
builder.Services.AddScoped<IMediaOptimizationService, MediaOptimizationService>();

// AI Content Enhancement Service
builder.Services.AddScoped<IAIContentEnhancementService, AIContentEnhancementService>();
builder.Services.AddScoped<IPostTest, PostTest>();
builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<ISeoAnalyzerService, PythonSeoAnalyzerService>();

// AI-Powered SEO Service
builder.Services.AddScoped<AISeoService>();

// Add Performance Services (simplified)
// builder.Services.AddScoped<PerformanceOptimizationService>(); // Commented out for now

// Google Search API Services (Primary SEO Engine)
var googleSearchOptionsBuilder = builder.Services
    .AddOptions<discussionspot9.Models.GoogleSearch.GoogleSearchConfig>()
    .Bind(builder.Configuration.GetSection("GoogleSearch"))
    .PostConfigure(options =>
    {
        options.ApiKey = FirstNonEmpty(
            options.ApiKey,
            builder.Configuration["GoogleSearch:ApiKey"],
            Environment.GetEnvironmentVariable("RAPIDAPI_GOOGLE_SEARCH_KEY"),
            Environment.GetEnvironmentVariable("GOOGLE_SEARCH_RAPIDAPI_KEY"));

        options.SerpApiKey = FirstNonEmpty(
            options.SerpApiKey,
            builder.Configuration["SerpApi:ApiKey"],
            Environment.GetEnvironmentVariable("SERPAPI_API_KEY"),
            Environment.GetEnvironmentVariable("SERP_API_KEY")) ?? string.Empty;

        if (options.EnableSerpApiFallback && string.IsNullOrWhiteSpace(options.SerpApiKey))
        {
            options.EnableSerpApiFallback = false;
            googleCredentialWarnings.Add("SerpApi fallback disabled because SERPAPI_API_KEY was not provided.");
        }

        if (options.EnableRapidApi && string.IsNullOrWhiteSpace(options.ApiKey))
        {
            googleCredentialWarnings.Add("RapidAPI Google Search is enabled but RAPIDAPI_GOOGLE_SEARCH_KEY was not provided.");
        }
    });

if (!builder.Environment.IsDevelopment())
{
    googleSearchOptionsBuilder
        .Validate(
            options => !options.EnableSerpApiFallback || !string.IsNullOrWhiteSpace(options.SerpApiKey),
            "SerpApi fallback is enabled but no SerpApi key was provided.")
        .Validate(
            options => !options.EnableRapidApi || !string.IsNullOrWhiteSpace(options.ApiKey),
            "RapidAPI Google Search is enabled but no API key was provided.")
        .ValidateOnStart();
}

builder.Services.AddHttpClient<discussionspot9.Services.GoogleSearchService>();
builder.Services.AddHttpClient("CompetitorContent");
builder.Services.AddHttpClient("AdSenseApi", client =>
{
    client.BaseAddress = new Uri("https://adsense.googleapis.com/v2/");
    client.Timeout = TimeSpan.FromSeconds(100);
});
builder.Services.AddScoped<discussionspot9.Services.SearchContentAggregator>();
builder.Services.AddScoped<discussionspot9.Services.GoogleSearchSeoService>();
builder.Services.AddScoped<discussionspot9.Services.HybridSeoService>();
builder.Services.AddScoped<discussionspot9.Services.EnhancedSeoService>();
builder.Services.AddScoped<discussionspot9.Services.SeoScoringService>();

// Image SEO Services
builder.Services.AddScoped<discussionspot9.Services.ImageSeoOptimizer>();
builder.Services.AddScoped<discussionspot9.Services.ImageStructuredDataService>();

builder.Services.AddSingleton<IBackgroundSeoService, BackgroundSeoService>(); // Singleton for background tasks

// SEO & Revenue Optimization Services
builder.Services.AddScoped<GoogleAdSenseService>();
builder.Services.AddScoped<GoogleSearchConsoleService>();
builder.Services.AddScoped<SmartPostSelectorService>();
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<EnhancedHomeService>();

// Enhanced SEO & Multi-Site Revenue Services
var adSenseOptionsBuilder = builder.Services
    .AddOptions<discussionspot9.Models.Configuration.AdSenseConfiguration>()
    .Bind(builder.Configuration.GetSection("GoogleAdSense"))
    .PostConfigure(options =>
    {
        options.ServiceAccountKeyEnvironmentVariable = FirstNonEmpty(
            options.ServiceAccountKeyEnvironmentVariable,
            builder.Configuration["GoogleAdSense:ServiceAccountKeyEnvironmentVariable"],
            "GOOGLE_ADSENSE_SERVICE_ACCOUNT_KEY") ?? "GOOGLE_ADSENSE_SERVICE_ACCOUNT_KEY";

        options.ServiceAccountKeyBase64 = FirstNonEmpty(
            options.ServiceAccountKeyBase64,
            builder.Configuration["AdSense:ServiceAccountKeyBase64"],
            Environment.GetEnvironmentVariable("GOOGLE_ADSENSE_SERVICE_ACCOUNT_KEY_BASE64")) ?? string.Empty;

        options.ServiceAccountKeyJson = FirstNonEmpty(
            options.ServiceAccountKeyJson,
            builder.Configuration["AdSense:ServiceAccountKeyJson"],
            Environment.GetEnvironmentVariable("GOOGLE_ADSENSE_SERVICE_ACCOUNT_KEY_JSON")) ?? string.Empty;
    });

if (!builder.Environment.IsDevelopment())
{
    adSenseOptionsBuilder
        .Validate(options =>
        {
            if (!options.UseServiceAccount)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(options.ServiceAccountKeyJson) ||
                !string.IsNullOrWhiteSpace(options.ServiceAccountKeyBase64) ||
                !string.IsNullOrWhiteSpace(options.ServiceAccountKeyPath))
            {
                return true;
            }

            var environmentCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(options.ServiceAccountKeyEnvironmentVariable))
            {
                environmentCandidates.Add(options.ServiceAccountKeyEnvironmentVariable);
            }
            environmentCandidates.Add("GOOGLE_ADSENSE_SERVICE_ACCOUNT_KEY");

            return environmentCandidates.Any(name =>
                !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)));
        }, "AdSense service account is enabled but no credentials were provided.")
        .ValidateOnStart();
}

builder.Services.Configure<discussionspot9.Models.Configuration.GoogleAdsConfiguration>(
    builder.Configuration.GetSection("GoogleAds"));
builder.Services.AddScoped<MultiSiteAdSenseService>();
builder.Services.AddScoped<GoogleKeywordPlannerService>();

builder.Services.AddHealthChecks()
    .AddCheck<discussionspot9.Services.HealthChecks.GoogleIntegrationsHealthCheck>("google_integrations");

// Background services
builder.Services.AddHostedService<WeeklySeoOptimizationService>();
builder.Services.AddHostedService<DailyDataSyncService>();
builder.Services.AddHostedService<NightlySeoRescoreService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

// =============================================
// CHAT SYSTEM SERVICES
// =============================================
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IPresenceService, PresenceService>();
builder.Services.AddScoped<IChatAdService, ChatAdService>();

// =============================================
// FILE STORAGE SERVICE
// =============================================
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// =============================================
// ADMIN & MODERATION SYSTEM
// =============================================
builder.Services.AddScoped<IAdminService, AdminService>();

// =============================================
// ANNOUNCEMENT SYSTEM
// =============================================
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();

var app = builder.Build();

if (googleCredentialWarnings.Count > 0)
{
    foreach (var warning in googleCredentialWarnings)
    {
        app.Logger.LogWarning(warning);
    }
}

if (!string.IsNullOrWhiteSpace(missingGoogleAuthPath))
{
    app.Logger.LogWarning("Google OAuth credentials not found. Provide credentials at {Path} or set GOOGLE_OAUTH_CLIENT_ID / GOOGLE_OAUTH_CLIENT_SECRET environment variables.", missingGoogleAuthPath);
}

// Ensure database is up-to-date (covers new chat tables in production)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var pending = dbContext.Database.GetPendingMigrations();
        if (pending.Any())
        {
            dbContext.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        logger.LogError(ex, "Failed to apply database migrations on startup.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // IMPROVED: Use custom error controller for better GSC error handling
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Development: Show detailed errors but still log them
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
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

// GOOGLE SEARCH CONSOLE FIX: Canonical URL enforcement
app.UseCanonicalUrls();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map SignalR hubs AFTER routing and authentication
app.MapHub<PostHub>("/posthub");
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<ChatHub>("/chatHub");
app.MapHub<PresenceHub>("/presenceHub");

app.MapRazorPages();
app.MapHealthChecks("/healthz/google");
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

// Web Stories Routes
app.MapControllerRoute(
    name: "stories_index",
    pattern: "stories",
    defaults: new { controller = "Stories", action = "Index" });

app.MapControllerRoute(
    name: "stories_create",
    pattern: "stories/create",
    defaults: new { controller = "Stories", action = "Create" });

app.MapControllerRoute(
    name: "stories_editor",
    pattern: "stories/editor",
    defaults: new { controller = "Stories", action = "Editor" });

app.MapControllerRoute(
    name: "stories_info",
    pattern: "stories/info",
    defaults: new { controller = "Stories", action = "Info" });

app.MapControllerRoute(
    name: "stories_amp",
    pattern: "stories/amp/{slug}",
    defaults: new { controller = "Stories", action = "Amp" },
    constraints: new { slug = @"^[a-zA-Z0-9_-]+$" });

app.MapControllerRoute(
    name: "stories_viewer",
    pattern: "stories/viewer/{slug}",
    defaults: new { controller = "Stories", action = "Viewer" },
    constraints: new { slug = @"^[a-zA-Z0-9_-]+$" });

app.MapControllerRoute(
    name: "stories_view",
    pattern: "stories/{slug}",
    defaults: new { controller = "Stories", action = "ViewStory" },
    constraints: new { slug = @"^[a-zA-Z0-9_-]+$" });

app.MapControllerRoute(
    name: "stories_edit",
    pattern: "stories/edit/{id}",
    defaults: new { controller = "Stories", action = "Edit" });

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

// Sitemap Routes
app.MapControllerRoute(
    name: "sitemap",
    pattern: "sitemap.xml",
    defaults: new { controller = "Sitemap", action = "Sitemap" });

app.MapControllerRoute(
    name: "sitemap_stories",
    pattern: "sitemap-stories.xml",
    defaults: new { controller = "Sitemap", action = "StoriesSitemap" });

app.MapControllerRoute(
    name: "post_create_general",
    pattern: "create",
    defaults: new { controller = "Post", action = "Create" });

// Chat Routes
app.MapControllerRoute(
    name: "chat_index",
    pattern: "chat",
    defaults: new { controller = "ChatView", action = "Index" });

app.MapControllerRoute(
    name: "chat_direct",
    pattern: "chat/direct/{userId}",
    defaults: new { controller = "ChatView", action = "Direct" },
    constraints: new { userId = @"^[a-zA-Z0-9_-]+$" });

app.MapControllerRoute(
    name: "chat_room",
    pattern: "chat/rooms/{roomId}",
    defaults: new { controller = "ChatView", action = "Room" });

app.MapControllerRoute(
    name: "chat_create_room",
    pattern: "chat/rooms/create",
    defaults: new { controller = "ChatView", action = "CreateRoom" });

// Default fallback route (keep this last)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string? FirstNonEmpty(params string?[] values)
{
    foreach (var value in values)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
    }

    return null;
}