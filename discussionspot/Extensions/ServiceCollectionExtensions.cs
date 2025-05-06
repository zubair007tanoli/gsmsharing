using discussionspot.Data;
using discussionspot.Models.Domain;
using discussionspot.Repositories;
using discussionspot.Services;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Extensions
{
    /// <summary>
    /// Extension methods for service collection configuration
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds application services to the container
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure database context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection") ??
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

            // Register HttpContextAccessor
            services.AddHttpContextAccessor();

            // Register repositories
            services.AddScoped(typeof(EfRepository<>));

            // Register specific ADO.NET repositories
            services.AddScoped<PostAdoRepository>();
            // Add other ADO.NET repositories as needed

            // Register model factory
            services.AddScoped<ModelFactory>();

            // Register application services
            services.AddScoped<PostService>();
            // Add other services as needed

            return services;
        }
    }
}
