
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

// Add services to the container
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DiscussionspotConnection"));
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();
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

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
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
builder.Services.AddHttpClient<ILinkMetadataService, LinkMetadataService>();
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
builder.Services.AddScoped<IPostTest, PostTest>();
builder.Services.AddScoped<IUserHelper, UserHelper>();
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
app.MapHub<PostHub>("/posthub");
app.MapHub<NotificationHub>("/notificationHub");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

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