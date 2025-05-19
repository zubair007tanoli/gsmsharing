using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using discussionspot.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace discussionspot.Services
{
    public class CommunityService : ICommunityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUsers> _userManager;

        public CommunityService(IUnitOfWork unitOfWork, UserManager<ApplicationUsers> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<CommunityViewModel> GetCommunityByIdAsync(int communityId)
        {
            var community = await _unitOfWork.Communities.GetByIdAsync(communityId);
            if (community == null)
                return null;

            return MapCommunityToViewModel(community);
        }

        public async Task<Community> GetCommunityEntityAsync(int communityId)
        {
            return await _unitOfWork.Communities.GetByIdAsync(communityId);
        }

        public async Task<CommunityViewModel> GetCommunityBySlugAsync(string slug)
        {
            var community = await _unitOfWork.Communities.GetCommunityBySlugAsync(slug);
            if (community == null)
                return null;

            return MapCommunityToViewModel(community);
        }

        public async Task<IEnumerable<CommunityViewModel>> GetCommunitiesAsync()
        {
            var communities = await _unitOfWork.Communities.GetAllAsync();
            return communities.Select(MapCommunityToViewModel);
        }

        public async Task<IEnumerable<CommunityViewModel>> GetPopularCommunitiesAsync(int count)
        {
            var communities = await _unitOfWork.Communities.GetPopularCommunitiesAsync(count);
            return communities.Select(MapCommunityToViewModel);
        }

        public async Task<int> CreateCommunityAsync(CommunityCreateViewModel model, string userId)
        {
            var community = new Community
            {
                Name = model.Name,
                Title = model.Title,
                Description = model.Description,
                ShortDescription = model.ShortDescription,
                CategoryId = model.CategoryId,
                CreatorId = userId,
                CommunityType = model.CommunityType,
                ThemeColor = model.ThemeColor,
                Rules = model.Rules,
                IsNSFW = model.IsNSFW,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            community.GenerateSlug();

            // Check if slug exists and make it unique if needed
            string baseSlug = community.Slug;
            int counter = 1;
            while (await _unitOfWork.Communities.SlugExistsAsync(community.Slug))
            {
                community.Slug = $"{baseSlug}-{counter}";
                counter++;
            }

            await _unitOfWork.Communities.AddAsync(community);
            await _unitOfWork.SaveChangesAsync();

            // Add creator as admin
            var membership = new CommunityMember
            {
                UserId = userId,
                CommunityId = community.CommunityId,
                Role = "admin",
                JoinedAt = DateTime.UtcNow
            };

            await _unitOfWork.CommunityMembers.AddAsync(membership);
            await _unitOfWork.SaveChangesAsync();

            return community.CommunityId;
        }

        public async Task<bool> JoinCommunityAsync(int communityId, string userId)
        {
            // Check if already a member
            var existingMembership = await _unitOfWork.CommunityMembers.GetMembershipAsync(userId, communityId);
            if (existingMembership != null)
                return false;

            // Create new membership
            var membership = new CommunityMember
            {
                UserId = userId,
                CommunityId = communityId,
                Role = "member",
                JoinedAt = DateTime.UtcNow
            };

            await _unitOfWork.CommunityMembers.AddAsync(membership);

            // Update member count
            await _unitOfWork.Communities.UpdateMemberCountAsync(communityId);

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LeaveCommunityAsync(int communityId, string userId)
        {
            // Check if a member
            var membership = await _unitOfWork.CommunityMembers.GetMembershipAsync(userId, communityId);
            if (membership == null)
                return false;

            // Can't leave if you're the admin/creator
            if (membership.Role == "admin")
                return false;

            // Remove membership
            _unitOfWork.CommunityMembers.Remove(membership);

            // Update member count
            await _unitOfWork.Communities.UpdateMemberCountAsync(communityId);

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsMemberAsync(int communityId, string userId)
        {
            return await _unitOfWork.CommunityMembers.IsMemberAsync(userId, communityId);
        }

        // Implement other methods...

        private CommunityViewModel MapCommunityToViewModel(Community community)
        {
            return new CommunityViewModel
            {
                CommunityId = community.CommunityId,
                Name = community.Name,
                Slug = community.Slug,
                Title = community.Title,
                Description = community.Description,
                ShortDescription = community.ShortDescription,
                CategoryId = community.CategoryId,
                CategoryName = community.Category?.Name,
                CreatorId = community.CreatorId,
                CreatorName = community.Creator?.UserName,
                CommunityType = community.CommunityType,
                CreatedAt = community.CreatedAt,
                UpdatedAt = community.UpdatedAt,
                IconUrl = community.IconUrl,
                BannerUrl = community.BannerUrl,
                ThemeColor = community.ThemeColor,
                MemberCount = community.MemberCount,
                PostCount = community.PostCount,
                IsNSFW = community.IsNSFW,
                IsDeleted = community.IsDeleted
            };
        }

        public async Task<bool> UpdateCommunityAsync(int communityId, CommunityCreateViewModel model, string userId)
        {
            var community = await _unitOfWork.Communities.GetByIdAsync(communityId);
            if (community == null)
                return false;

            // Check if user has permission (is admin or creator)
            var membership = await _unitOfWork.CommunityMembers.GetMembershipAsync(userId, communityId);
            if (membership == null || membership.Role != "admin")
                return false;

            // Update properties
            community.Name = model.Name;
            community.Title = model.Title;
            community.Description = model.Description;
            community.ShortDescription = model.ShortDescription;
            community.CategoryId = model.CategoryId;
            community.CommunityType = model.CommunityType;
            community.ThemeColor = model.ThemeColor;
            community.Rules = model.Rules;
            community.IsNSFW = model.IsNSFW;
            community.UpdatedAt = DateTime.UtcNow;

            // Update slug if name changed
            if (community.Name != model.Name)
            {
                community.GenerateSlug();

                // Check if slug exists and make it unique if needed
                string baseSlug = community.Slug;
                int counter = 1;
                while (await _unitOfWork.Communities.SlugExistsAsync(community.Slug) &&
                       community.Slug != model.Slug) // Skip check for current slug
                {
                    community.Slug = $"{baseSlug}-{counter}";
                    counter++;
                }
            }

            _unitOfWork.Communities.Update(community);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
