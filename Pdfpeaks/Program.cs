using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Hangfire;
using Hangfire.SqlServer;
using Pdfpeaks.Database;
using Swashbuckle.AspNetCore.SwaggerUI;
using Pdfpeaks.Models;
using Pdfpeaks.Services;
using Pdfpeaks.Services.AI;
using Pdfpeaks.Services.Auth;
using Pdfpeaks.Services.Caching;
using Pdfpeaks.Services.Infrastructure;
using Pdfpeaks.Services.Realtime;
using Serilog;
using StackExchange.Redis;
using System.Security.Claims;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// ============ Configure Kestrel ============
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 150_000_000; // 150MB
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
});

// ============ Configure Serilog ============
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/pdfpeaks-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// ============ Add Services ============
builder.Services.AddAntiforgery(options =>
{
    options.FormFieldName = "__RequestVerificationToken";
    options.HeaderName = "X-XSRF-TOKEN";
    // Keep framework antiforgery cookie separate from the JS-readable request-token cookie.
    // The middleware below writes the request token to "XSRF-TOKEN" for fetch/AJAX.
    options.Cookie.Name = "CSRF-COOKIE";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.None 
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = builder.Environment.IsDevelopment() 
        ? SameSiteMode.Lax 
        : SameSiteMode.Strict;
});

// Add MVC with global CSRF validation
builder.Services.AddControllersWithViews(options =>
{
    // Global CSRF validation for all POST/PUT/DELETE requests
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
})
.AddNewtonsoftJson();

// Add HttpContext accessor
builder.Services.AddHttpContextAccessor();

// ============ Database Configuration ============
// Priority: appsettings.Development.json > env var DB_CONNECTION_STRING > appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString))
{
    if (builder.Environment.IsProduction())
    {
        Log.Fatal("DB_CONNECTION_STRING environment variable is not set. Application cannot start in Production without a database connection.");
        throw new InvalidOperationException("DB_CONNECTION_STRING is required in Production. Set the environment variable.");
    }
    // Development fallback — uses local SQL Server with default dev credentials
    connectionString = "Server=localhost;Database=Pdfpeaks;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;";
    Log.Warning("No DB connection string configured. Using local development fallback. Set ConnectionStrings__DefaultConnection or DB_CONNECTION_STRING for production.");
}

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
        }));

    // Add Identity services
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        
        // User settings
        options.User.RequireUniqueEmail = true;
        
        // Sign-in settings
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
        
        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddPasswordValidator<CustomPasswordValidator>();

    // Configure application cookie
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/UserAccounts/Login";
        options.LogoutPath = "/UserAccounts/Logout";
        options.AccessDeniedPath = "/UserAccounts/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
            ? CookieSecurePolicy.None 
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = builder.Environment.IsDevelopment() 
            ? SameSiteMode.Lax 
            : SameSiteMode.Strict;
    });

    // ============ Hangfire Background Jobs ============
    builder.Services.AddHangfire(config =>
    {
        config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
              .UseSimpleAssemblyNameTypeSerializer()
              .UseRecommendedSerializerSettings()
              .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
              {
                  CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                  SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                  QueuePollInterval = TimeSpan.FromSeconds(15),
                  UseRecommendedIsolationLevel = true,
                  DisableGlobalLocks = true
              });
    });

    builder.Services.AddHangfireServer();
}

// ============ Register Custom Services ============

// Register IConnectionMultiplexer for Redis (lazy singleton - connects on first use)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Redis") 
        ?? configuration["Redis:ConnectionString"] 
        ?? "localhost:6379";

    var config = ConfigurationOptions.Parse(connectionString);
    config.AbortOnConnectFail = false;  // Don't throw if Redis unavailable
    config.ConnectTimeout = 100;        // Quick timeout (100ms) - fail fast
    config.SyncTimeout = 100;
    config.AsyncTimeout = 100;

    try
    {
        // Try to connect with very short timeout - will retry on first use
        return ConnectionMultiplexer.Connect(config);
    }
    catch
    {
        // If Redis unavailable, return null multiplexer - services handle gracefully
        Log.Warning("Redis connection failed during startup. Services will retry on demand.");
        return null!;  // Services must handle null gracefully
    }
});

// AI Services (Self-dependent - no external APIs)
builder.Services.AddSingleton<DocumentAnalysisService>();
builder.Services.AddScoped<AIService>();

// Configure HttpClientFactory for AIService
builder.Services.AddHttpClient("AIService")
    .ConfigureHttpClient((serviceProvider, client) =>
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var aiServiceUrl = configuration["AIService:Url"] ?? "http://localhost:8000";
        client.BaseAddress = new Uri(aiServiceUrl);
        client.Timeout = TimeSpan.FromMinutes(5);
    });

// Caching Services
builder.Services.AddSingleton<RedisCacheService>();

// Authentication Services
builder.Services.AddScoped<JwtTokenService>();

// MCP Servers
// Optional: builder.Services.AddSingleton<SEOMcpServer>();
// Optional: builder.Services.AddSingleton<PerformanceMcpServer>();

// Processing Services
builder.Services.AddScoped<FileProcessingService>();
builder.Services.AddScoped<PdfProcessingService>();
builder.Services.AddScoped<ImageProcessingService>();
builder.Services.AddScoped<ImageEnhancementService>();
// ConversionJobService removed - conversions now handled directly in ConversionJobsController
builder.Services.AddScoped<SitemapService>();

// Infrastructure Services
builder.Services.AddSingleton<CustomRateLimitService>();
builder.Services.AddSingleton<HealthCheckAggregator>();
builder.Services.AddSingleton<ProcessingBroadcastService>();

// Background Services
builder.Services.AddHostedService<TempFileCleanupService>();

// ============ Add Rate Limiting ============
builder.Services.AddPdfpeaksRateLimiting(builder.Configuration);

// ============ Add Health Checks ============
builder.Services.AddPdfpeaksHealthChecks(builder.Configuration);

// ============ Add CORS ============
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("X-Pagination");
    });
    
    options.AddPolicy("AllowReactApp", policy =>
    {
        var reactUrl = builder.Configuration["ReactApp:Url"] ?? "http://localhost:3000";
        policy.WithOrigins(reactUrl)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============ Add Swagger/OpenAPI ============
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pdfpeaks API",
        Version = "v2.0",
        Description = "Modern PDF processing API with AI capabilities",
        Contact = new OpenApiContact
        {
            Name = "Pdfpeaks Support",
            Email = "support@pdfpeaks.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============ Add API Versioning ============
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(2, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// ============ Add SignalR ============
builder.Services.AddSignalR()
    .AddMessagePackProtocol();

var app = builder.Build();

// ============ Configure Pipeline ============

// Exception handling
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserId", httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    };
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

// Serve temp_files for PDF thumbnails and downloads
var tempFilesPath = Path.Combine(builder.Environment.ContentRootPath, "temp_files");
if (Directory.Exists(tempFilesPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(tempFilesPath),
        RequestPath = "/temp_files"
    });
}

app.UseRouting();

// XSRF/CSRF token middleware for JavaScript AJAX requests
app.Use(async (context, next) =>
{
    if (context.Request.Method == "GET")
    {
        // Generate token for GET requests (needed for subsequent POST requests)
        var antiforgery = context.RequestServices.GetService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>();
        if (antiforgery != null)
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            if (tokens.RequestToken != null)
            {
                context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = !app.Environment.IsDevelopment(),
                    SameSite = app.Environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict
                });
            }
        }
    }
    await next();
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiting();

// Swagger - only enable in Development to avoid exposing UI for MVC app in other environments
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "/swagger/{documentName}/swagger.json";
        options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
        {
            swaggerDoc.Servers = new List<OpenApiServer>
            {
                new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host}" }
            };
        });
    });
}

// Map routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action=Get}/{id?}");

// Map SignalR hubs
app.MapHub<ProcessingHub>("/hubs/processing");

// Map health check endpoint
app.MapGet("/health", async (HealthCheckAggregator aggregator, [FromServices] TempFileCleanupService cleanupService) =>
{
    var health = await aggregator.GetHealthStatusAsync();
    var tempStats = cleanupService.GetStats();
    return Results.Ok(new
    {
        health,
        tempFiles = new
        {
            totalFiles = tempStats.TotalFiles,
            expiredFiles = tempStats.ExpiredFiles,
            totalSizeMB = Math.Round(tempStats.TotalSizeMB, 2)
        }
    });
});

// Manual temp file cleanup endpoint (admin only)
app.MapPost("/admin/cleanup-temp-files", async (TempFileCleanupService cleanupService) =>
{
    await cleanupService.CleanupOldFilesAsync();
    var stats = cleanupService.GetStats();
    return Results.Ok(new
    {
        success = true,
        message = "Cleanup completed",
        remainingFiles = stats.TotalFiles,
        remainingSizeMB = Math.Round(stats.TotalSizeMB, 2)
    });
});

app.MapHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Checks = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Duration = e.Value.Duration.TotalMilliseconds,
                Description = e.Value.Description
            })
        }));
    }
});

app.Run();

// ============ Custom Password Validator ============
public class CustomPasswordValidator : IPasswordValidator<ApplicationUser>
{
    public Task<IdentityResult> ValidateAsync(
        UserManager<ApplicationUser> manager, 
        ApplicationUser user, 
        string? password)
    {
        var errors = new List<IdentityError>();
        
        if (password != null && password.Contains(user.UserName ?? ""))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordContainsUsername",
                Description = "Password cannot contain your username"
            });
        }
        
        if (password != null && !password.Any(char.IsUpper))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresUppercase",
                Description = "Password must contain at least one uppercase letter"
            });
        }
        
        return Task.FromResult(errors.Count == 0 
            ? IdentityResult.Success 
            : IdentityResult.Failed(errors.ToArray()));
    }
}
