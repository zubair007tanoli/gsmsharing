using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FluxPDF.Models;

namespace FluxPDF.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<FileMetadata> FileMetadata { get; set; }
        public DbSet<ProcessingJob> ProcessingJobs { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<FileMetadata>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.FileName).IsRequired().HasMaxLength(500); entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000); entity.Property(e => e.FileSize).IsRequired(); entity.HasIndex(e => e.UserId); entity.HasIndex(e => e.CreatedAt); });
            builder.Entity<ProcessingJob>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.JobId).IsRequired().HasMaxLength(100); entity.HasIndex(e => e.JobId).IsUnique(); entity.HasIndex(e => e.UserId); entity.HasIndex(e => e.Status); });
            builder.Entity<ApiKey>(entity => { entity.HasKey(e => e.Id); entity.Property(e => e.Key).IsRequired().HasMaxLength(100); entity.HasIndex(e => e.Key).IsUnique(); entity.HasIndex(e => e.UserId); });
        }
    }
}
