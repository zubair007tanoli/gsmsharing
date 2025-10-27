using gsmsharing.Database;
using gsmsharing.Interfaces;
using gsmsharing.Models;
using gsmsharing.Models.APIGPT;
using gsmsharing.Models.ImageModels;
using gsmsharing.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using gsmsharing.Business;
using gsmsharing.Models.SEO;

var builder = WebApplication.CreateBuilder(args);
//Asp.Net Core Identity DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GsmsharingConnection")));
//builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddAuthentication(options => {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie();

builder.Services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/UserAccount/Login"; // Path to your login page
    options.AccessDeniedPath = "/Account/AccessDenied"; // Optional: Path for access denied 
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});
//Asp.Net Core Identity DB Last

builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ICategoryRepository,CategoryRepository>();
builder.Services.AddScoped<DatabaseConnection>();
builder.Services.AddScoped<IFileStorage, ImageRepository>();
builder.Services.AddScoped<ICommunityRepository, CommunityRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ISeo, SEORepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<PostSEODataAccess>();
builder.Services.AddSingleton<AIContentGenerator>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Show detailed error pages in development
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint(); // Database error page
}
else
{
    // Use custom error pages in production
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseMiddleware<RedirectAuthorized>();
app.UseHttpsRedirection();

// Serve static files from wwwroot
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // Add authentication middleware
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


app.Run();
