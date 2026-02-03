using System.ComponentModel.DataAnnotations;

namespace Pdfpeaks.Models;

/// <summary>
/// History of user subscriptions
/// </summary>
public class SubscriptionHistory
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to user
    /// </summary>
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Previous subscription tier
    /// </summary>
    public SubscriptionTier PreviousTier { get; set; }

    /// <summary>
    /// New subscription tier
    /// </summary>
    public SubscriptionTier NewTier { get; set; }

    /// <summary>
    /// Stripe subscription ID
    /// </summary>
    public string? StripeSubscriptionId { get; set; }

    /// <summary>
    /// Stripe invoice ID
    /// </summary>
    public string? StripeInvoiceId { get; set; }

    /// <summary>
    /// Amount paid
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Type of action: Upgrade, Downgrade, Cancel, Renew
    /// </summary>
    public SubscriptionAction Action { get; set; }

    /// <summary>
    /// Reason for change
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// When the subscription started
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// When the subscription ended (for cancelled subscriptions)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// When the record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Types of subscription actions
/// </summary>
public enum SubscriptionAction
{
    Initial = 1,
    Upgrade = 2,
    Downgrade = 3,
    Cancel = 4,
    Renew = 5,
    Refund = 6
}
