using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pdfpeaks.Models;

namespace Pdfpeaks.Database;

/// <summary>
/// Application database context for Entity Framework Core
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<FileProcessingLog> FileProcessingLogs { get; set; }
    public DbSet<DownloadLog> DownloadLogs { get; set; }
    public DbSet<SubscriptionHistory> SubscriptionHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.SubscriptionTier).HasDefaultValue(SubscriptionTier.Free);
            entity.Property(e => e.DownloadsRemaining).HasDefaultValue(1);
            entity.Property(e => e.TotalStorageUsed).HasDefaultValue(0L);
            entity.Property(e => e.MaxStorageAllowed).HasDefaultValue(104857600L); // 100MB
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // Configure FileProcessingLog
        builder.Entity<FileProcessingLog>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.OperationType);
            
            entity.Property(e => e.OriginalFileSize).HasDefaultValue(0);
            entity.Property(e => e.ProcessedFileSize).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue(ProcessingStatus.Pending);
            entity.Property(e => e.WasDownloaded).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure DownloadLog
        builder.Entity<DownloadLog>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.DownloadedAt);
            
            entity.Property(e => e.DownloadType).HasDefaultValue(DownloadType.Single);
            entity.Property(e => e.IsPaidUser).HasDefaultValue(false);
            entity.Property(e => e.DownloadedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure SubscriptionHistory
        builder.Entity<SubscriptionHistory>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            
            entity.Property(e => e.Amount).HasDefaultValue(0);
            entity.Property(e => e.Currency).HasDefaultValue("USD");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
