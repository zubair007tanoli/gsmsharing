using Microsoft.AspNetCore.Identity;

namespace Pdfpeaks.Models;

/// <summary>
/// Extended user model with subscription tier support
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// User's first name
    /// </summary>
    [PersonalData]
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    [PersonalData]
    public string? LastName { get; set; }

    /// <summary>
    /// User's subscription tier: Free, Pro, Enterprise
    /// </summary>
    [PersonalData]
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;

    /// <summary>
    /// Stripe customer ID for payment processing
    /// </summary>
    public string? StripeCustomerId { get; set; }

    /// <summary>
    /// Subscription end date
    /// </summary>
    public DateTime? SubscriptionEndDate { get; set; }

    /// <summary>
    /// Number of downloads remaining for free tier
    /// </summary>
    public int DownloadsRemaining { get; set; } = 1;

    /// <summary>
    /// Total files processed by the user
    /// </summary>
    public int TotalFilesProcessed { get; set; } = 0;

    /// <summary>
    /// Total storage used in bytes
    /// </summary>
    public long TotalStorageUsed { get; set; } = 0;

    /// <summary>
    /// Maximum storage allowed in bytes (based on tier)
    /// </summary>
    public long MaxStorageAllowed { get; set; } = 100 * 1024 * 1024; // 100MB for free tier

    /// <summary>
    /// Whether the user has verified their email
    /// </summary>
    public bool EmailVerified { get; set; } = false;

    /// <summary>
    /// User's avatar URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// User's timezone
    /// </summary>
    public string? Timezone { get; set; } = "UTC";

    /// <summary>
    /// User's preferred language
    /// </summary>
    public string? PreferredLanguage { get; set; } = "en";

    /// <summary>
    /// Last login date
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Account creation date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update date
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for user's file processing history
    /// </summary>
    public ICollection<FileProcessingLog> FileProcessingLogs { get; set; } = new List<FileProcessingLog>();

    /// <summary>
    /// Navigation property for user's download history
    /// </summary>
    public ICollection<DownloadLog> DownloadLogs { get; set; } = new List<DownloadLog>();

    /// <summary>
    /// Navigation property for user's subscription history
    /// </summary>
    public ICollection<SubscriptionHistory> SubscriptionHistories { get; set; } = new List<SubscriptionHistory>();
}

/// <summary>
/// User subscription tiers
/// </summary>
public enum SubscriptionTier
{
    Free = 0,
    Pro = 1,
    Enterprise = 2
}
