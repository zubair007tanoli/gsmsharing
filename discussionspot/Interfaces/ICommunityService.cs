namespace discussionspot.Interfaces
{
    public interface ICommunityService
    {
        Task<IEnumerable<CommunityViewModel>> GetCommunitiesForUserAsync(string userId);
        Task<IEnumerable<CommunityViewModel>> GetPopularCommunitiesAsync(int count);
        Task<IEnumerable<CategoryViewModel>> GetCategoriesAsync();
        Task<IEnumerable<CommunityRuleViewModel>> GetCommunityRulesAsync(int communityId);
    }
}
