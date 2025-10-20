using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels
{
    public class CommunityRulesViewModel
    {
        public int CommunityId { get; set; }
        public string CommunityName { get; set; } = string.Empty;
        public string CommunitySlug { get; set; } = string.Empty;
        
        // Rules management
        public List<RuleViewModel> Rules { get; set; } = new();
        public List<RuleTemplateViewModel> RuleTemplates { get; set; } = new();
        
        // Current user permissions
        public bool CanEditRules { get; set; }
        public bool CanDeleteRules { get; set; }
        public bool CanReorderRules { get; set; }
    }
    
    public class RuleViewModel
    {
        public int RuleId { get; set; }
        public int CommunityId { get; set; }
        
        [Required(ErrorMessage = "Rule title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Rule description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;
        
        public int DisplayOrder { get; set; }
        public RuleSeverity Severity { get; set; } = RuleSeverity.General;
        public string Icon { get; set; } = "fas fa-info-circle";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
    
    public class RuleTemplateViewModel
    {
        public int TemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public RuleSeverity Severity { get; set; }
        public string Icon { get; set; } = string.Empty;
        public bool IsPopular { get; set; }
    }
    
    public enum RuleSeverity
    {
        General = 0,
        Important = 1,
        Critical = 2
    }
}
