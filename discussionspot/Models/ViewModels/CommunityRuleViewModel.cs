namespace discussionspot.Models.ViewModels
{
    /// <summary>
    /// View model for community rule data
    /// </summary>
    public class CommunityRuleViewModel
    {
        public int RuleId { get; set; }
        public int CommunityId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
