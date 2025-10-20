using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels
{
    public class ContentFiltersViewModel
    {
        public int CommunityId { get; set; }
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        
        // Banned words
        public List<BannedWordViewModel> BannedWords { get; set; } = new();
        public string NewBannedWord { get; set; } = string.Empty;
        public bool CaseSensitive { get; set; } = false;
        public bool WholeWordOnly { get; set; } = true;
        
        // AutoMod settings
        public AutoModSettingsViewModel AutoModSettings { get; set; } = new();
        
        // Spam detection
        public SpamDetectionSettingsViewModel SpamSettings { get; set; } = new();
        
        // Content approval
        public ContentApprovalSettingsViewModel ApprovalSettings { get; set; } = new();
        
        // Filter logs
        public List<FilterLogViewModel> RecentLogs { get; set; } = new();
        public int TotalFilteredItems { get; set; }
        public int FilteredToday { get; set; }
        public int FilteredThisWeek { get; set; }
        
        // Current user permissions
        public bool CanEditFilters { get; set; }
        public bool CanViewLogs { get; set; }
    }
    
    public class BannedWordViewModel
    {
        public int Id { get; set; }
        public string Word { get; set; } = string.Empty;
        public bool CaseSensitive { get; set; }
        public bool WholeWordOnly { get; set; }
        public string Action { get; set; } = "remove"; // remove, replace, flag
        public string? Replacement { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public int UsageCount { get; set; }
    }
    
    public class AutoModSettingsViewModel
    {
        public bool IsEnabled { get; set; }
        public int MinAccountAge { get; set; } = 0; // days
        public int MinKarma { get; set; } = 0;
        public int MaxPostsPerDay { get; set; } = 10;
        public int MaxCommentsPerDay { get; set; } = 50;
        public bool RequireApprovalForNewUsers { get; set; } = false;
        public bool AutoRemoveSpam { get; set; } = true;
        public bool AutoRemoveDuplicateContent { get; set; } = true;
        public int DuplicateThreshold { get; set; } = 80; // percentage similarity
    }
    
    public class SpamDetectionSettingsViewModel
    {
        public bool IsEnabled { get; set; }
        public double SpamThreshold { get; set; } = 0.7; // 0-1 scale
        public bool CheckLinks { get; set; } = true;
        public bool CheckImages { get; set; } = true;
        public bool CheckRepetitiveContent { get; set; } = true;
        public int MaxLinksPerPost { get; set; } = 3;
        public int MaxImagesPerPost { get; set; } = 5;
        public List<string> AllowedDomains { get; set; } = new();
        public List<string> BlockedDomains { get; set; } = new();
    }
    
    public class ContentApprovalSettingsViewModel
    {
        public bool RequireApproval { get; set; }
        public bool RequireApprovalForNewUsers { get; set; } = true;
        public bool RequireApprovalForPosts { get; set; } = false;
        public bool RequireApprovalForComments { get; set; } = false;
        public int ApprovalQueueSize { get; set; }
        public int PendingApprovals { get; set; }
        public DateTime? LastApproval { get; set; }
    }
    
    public class FilterLogViewModel
    {
        public int Id { get; set; }
        public string ContentType { get; set; } = string.Empty; // post, comment
        public string ContentId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string FilterReason { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // removed, flagged, approved
        public DateTime FilteredAt { get; set; }
        public string FilteredBy { get; set; } = string.Empty;
        public string ContentPreview { get; set; } = string.Empty;
    }
}
