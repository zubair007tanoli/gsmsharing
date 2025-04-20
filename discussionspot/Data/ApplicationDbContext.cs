using discussionspot.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Data
{
    public class ApplicationDbContext:IdentityDbContext<ApplicationUsers>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            List<IdentityRole> roles = new List<IdentityRole>
    {
        new IdentityRole
        {
            Id = "1",  // Add this
            Name = "Admin",
            NormalizedName = "ADMIN",
            ConcurrencyStamp = "20250401000000"
        },
        new IdentityRole
        {
            Id = "2",  // Add this
            Name = "User",
            NormalizedName = "USER",
            ConcurrencyStamp = "20250401000001"  // Make this unique
        },
        new IdentityRole
        {
            Id = "3",  // Add this
            Name = "Editor",
            NormalizedName = "EDITOR",
            ConcurrencyStamp = "20250401000002"  // Make this unique
        }
    };
            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}
