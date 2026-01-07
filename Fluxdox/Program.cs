using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Fluxdox.Data;
using Fluxdox.Services; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // Required for MVC controllers and API controllers
builder.Services.AddRazorPages();          // Required for Razor Pages

// Example for DbContext (replace with your actual DB setup)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)); // Using PostgreSQL as per PRD

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure ASP.NET Identity (adjust as needed for your user model and roles)
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSignalR(); // For real-time progress updates (Section 5.2 Async Processing)

// Register custom services for dependency injection
builder.Services.AddSingleton<IStorageService, S3StorageService>(); // Register mock S3 storage service
builder.Services.AddSingleton<IJobQueue, RedisJobQueue>();       // Register mock Redis job queue

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map MVC controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages
app.MapRazorPages(); // This maps default Identity UI and any custom Razor Pages

app.Run();

// Placeholder for ApplicationDbContext. In a real project, this would be in a 'Data' folder.
namespace Fluxdox.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }
    }
}
