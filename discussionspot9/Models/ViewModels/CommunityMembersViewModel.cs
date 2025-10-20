using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels
{
    public class CommunityMembersViewModel
    {
        public int CommunityId { get; set; }
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        
        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalMembers { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalMembers / PageSize);
        
        // Search and filters
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; } = "all";
        public string? SortBy { get; set; } = "joined";
        public string? SortOrder { get; set; } = "desc";
        
        // Member list
        public List<MemberViewModel> Members { get; set; } = new();
        
        // Statistics
        public int TotalAdmins { get; set; }
        public int TotalModerators { get; set; }
        public int TotalRegularMembers { get; set; }
        public int NewMembersThisWeek { get; set; }
        public int NewMembersThisMonth { get; set; }
        
        // Current user permissions
        public bool CanManageMembers { get; set; }
        public bool CanChangeRoles { get; set; }
        public bool CanBanMembers { get; set; }
    }
    
    public class MemberViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string Initials { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public string FormattedJoinedAt { get; set; } = string.Empty;
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public int Karma { get; set; }
        public DateTime? LastActivity { get; set; }
        public string FormattedLastActivity { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public bool IsBanned { get; set; }
        public DateTime? BannedUntil { get; set; }
        public string? BanReason { get; set; }
    }
}
