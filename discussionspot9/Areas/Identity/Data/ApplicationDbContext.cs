using discussionspot9.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace DiscussionSpot9.Data.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // UserProfile table from schema
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure UserProfile entity
            builder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Bio).HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.AvatarUrl).HasMaxLength(2048);
                entity.Property(e => e.BannerUrl).HasMaxLength(2048);
                entity.Property(e => e.Website).HasMaxLength(2048);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.JoinDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.LastActive).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.KarmaPoints).HasDefaultValue(0);
                entity.Property(e => e.IsVerified).HasDefaultValue(false);

                // Foreign key to AspNetUsers
                entity.HasOne<IdentityUser>()
                    .WithOne()
                    .HasForeignKey<UserProfile>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed Roles - matching schema expectations
            var adminRoleId = Guid.NewGuid().ToString();
            var moderatorRoleId = Guid.NewGuid().ToString();
            var userRoleId = Guid.NewGuid().ToString();

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new IdentityRole
                {
                    Id = moderatorRoleId,
                    Name = "Moderator",
                    NormalizedName = "MODERATOR",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new IdentityRole
                {
                    Id = userRoleId,
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            );

            // Seed Test User (matching schema test user)
            var testUserId = Guid.NewGuid().ToString();
            var hasher = new PasswordHasher<IdentityUser>();

            builder.Entity<IdentityUser>().HasData(
                new IdentityUser
                {
                    Id = testUserId,
                    UserName = "testuser@discussionspot.com",
                    NormalizedUserName = "TESTUSER@DISCUSSIONSPOT.COM",
                    Email = "testuser@discussionspot.com",
                    NormalizedEmail = "TESTUSER@DISCUSSIONSPOT.COM",
                    EmailConfirmed = true,
                    PasswordHash = hasher.HashPassword(new IdentityUser(), "Password123!"),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0
                }
            );

            // Assign User role to test user
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = userRoleId,
                    UserId = testUserId
                }
            );

            // Seed UserProfile for test user
            builder.Entity<UserProfile>().HasData(
                new UserProfile
                {
                    UserId = testUserId,
                    DisplayName = "TestUser",
                    Bio = "This is a test account for DiscussionSpot.",
                    JoinDate = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow,
                    KarmaPoints = 0,
                    IsVerified = false
                }
            );
        }
    }
}