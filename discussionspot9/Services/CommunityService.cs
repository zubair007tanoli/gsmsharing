using discussionspot9.Data.DbContext;
using discussionspot9.Helpers;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace discussionspot9.Services
{
    public class CommunityService : ICommunityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CommunityService> _logger;

        public CommunityService(ApplicationDbContext context, IMemoryCache cache, ILogger<CommunityService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<CommunityListViewModel> GetAllCommunitiesAsync(string sort, int page)
        {
            const int pageSize = 20;
            var skip = (page - 1) * pageSize;

            var query = _context.Communities
                .Include(c => c.Category)
                .Where(c => !c.IsDeleted);

            // Apply sorting
            query = sort switch
            {
                "newest" => query.OrderByDescending(c => c.CreatedAt),
                "active" => query.OrderByDescending(c => c.UpdatedAt),
                "members" => query.OrderByDescending(c => c.MemberCount),
                _ => query.OrderByDescending(c => c.PostCount).ThenByDescending(c => c.MemberCount) // "popular" or default
            };

            var totalCommunities = await query.CountAsync();
            var communities = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(c => new CommunityCardViewModel
                {
                    CommunityId = c.CommunityId,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.ShortDescription ?? c.Description.Substring(0, Math.Min(c.Description.Length, 200)),
                    IconUrl = c.IconUrl,
                    MemberCount = c.MemberCount,
                    PostCount = c.PostCount,                    
                    //Categories = c.Category != null ? new List<string> { c.Category.Name } : new List<string>(),
                    IsCurrentUserMember = false // Will be populated if user is authenticated
                })
                .ToListAsync();

            // Get category counts for filters
            var categoryCounts = await _context.Communities
                .Where(c => !c.IsDeleted && c.Category != null)
                .GroupBy(c => c.Category.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            // If user is authenticated, check membership status
            if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var userMemberships = await _context.CommunityMembers
                        .Where(cm => cm.UserId == userId && communities.Select(c => c.CommunityId).Contains(cm.CommunityId))
                        .Select(cm => cm.CommunityId)
                        .ToListAsync();

                    foreach (var community in communities)
                    {
                        community.IsUserMember = userMemberships.Contains(community.CommunityId);
                    }
                }
            }

            return new CommunityListViewModel
            {
                Communities = communities,
                TotalCommunities = totalCommunities,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCommunities / (double)pageSize),
                CurrentSort = sort,
                CategoryCounts = categoryCounts
            };
        }
        //public async Task<CommunityListViewModel> GetAllCommunitiesAsync(string sort = "popular", int page = 1)
        //{
        //    const int pageSize = 20;
        //    var skip = (page - 1) * pageSize;

        //    var query = _context.Communities
        //        .Include(c => c.Category)
        //        .Where(c => !c.IsDeleted);

        //    query = sort switch
        //    {
        //        "new" => query.OrderByDescending(c => c.CreatedAt),
        //        "active" => query.OrderByDescending(c => c.Posts.Max(p => (DateTime?)p.CreatedAt) ?? c.CreatedAt),
        //        _ => query.OrderByDescending(c => c.MemberCount)
        //    };

        //    var totalCommunities = await query.CountAsync();
        //    var communities = await query
        //        .Skip(skip)
        //        .Take(pageSize)
        //        .Select(c => new CommunityCardViewModel
        //        {
        //            CommunityId = c.CommunityId,
        //            Name = c.Name,
        //            Slug = c.Slug,
        //            Title = c.Title,
        //            Description = c.ShortDescription,
        //            IconUrl = c.IconUrl,
        //            MemberCount = c.MemberCount,
        //            PostCount = c.PostCount,
        //            IsNSFW = c.IsNSFW,
        //            CategoryName = c.Category!.Name,
        //            CategorySlug = c.Category.Slug,
        //            LastActivity = c.Posts.Max(p => (DateTime?)p.CreatedAt)
        //        })
        //        .ToListAsync();

        //    return new CommunityListViewModel
        //    {
        //        Communities = communities,
        //        TotalCommunities = totalCommunities,
        //        CurrentPage = page,
        //        TotalPages = (int)Math.Ceiling(totalCommunities / (double)pageSize),
        //        CurrentSort = sort
        //    };
        //}

        public async Task<CommunityDetailViewModel?> GetCommunityDetailsAsync(string slug)
        {
            var community = await _context.Communities
                .Include(c => c.Category)
                .Include(c => c.Members.Where(m => m.Role == "moderator"))
                .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted);

            if (community == null) return null;

            return new CommunityDetailViewModel
            {
                CommunityId = community.CommunityId,
                Name = community.Name,
                Slug = community.Slug,
                Title = community.Title,
                Description = community.Description,
                IconUrl = community.IconUrl,
                BannerUrl = community.BannerUrl,
                MemberCount = community.MemberCount,
                PostCount = community.PostCount,
                CreatedAt = community.CreatedAt,
                Rules = community.Rules,
                IsNSFW = community.IsNSFW,
                CommunityType = community.CommunityType,
                CategoryName = community.Category?.Name,
                CategorySlug = community.Category?.Slug,
                CreatorId = community.CreatorId,
                Moderators = community.Members.Select(m => new ModeratorViewModel
                {
                    UserId = m.UserId,
                    DisplayName = _context.UserProfiles
                        .Where(up => up.UserId == m.UserId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault() ?? "Unknown",
                    Role = m.Role
                }).ToList()
            };
        }

        public async Task<CommunityDetailViewModel?> GetCommunityBySlugAsync(string slug)
        {
            return await GetCommunityDetailsAsync(slug);
        }

        public async Task<CreateCommunityResult> CreateCommunityAsync(CreateCommunityViewModel model)
        {
            // Check if name already exists
            if (await _context.Communities.AnyAsync(c => c.Name == model.Name))
            {
                return new CreateCommunityResult
                {
                    Success = false,
                    ErrorMessage = "A community with this name already exists."
                };
            }

            var slug = model.Name.ToSlug();
            var community = new Community
            {
                Name = model.Name,
                Slug = slug,
                Title = model.Title,
                Description = model.Description,
                ShortDescription = model.Description?.Length > 500 ?
                    model.Description.Substring(0, 497) + "..." : model.Description,
                CategoryId = model.CategoryId,
                CreatorId = model.CreatorId,
                CommunityType = model.CommunityType,
                IsNSFW = model.IsNSFW,
                Rules = model.Rules,
                IconUrl = model.IconUrl,
                BannerUrl = model.BannerUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Communities.Add(community);
            await _context.SaveChangesAsync();

            // Add creator as admin member
            var member = new CommunityMember
            {
                UserId = model.CreatorId!,
                CommunityId = community.CommunityId,
                Role = "admin",
                JoinedAt = DateTime.UtcNow
            };

            _context.CommunityMembers.Add(member);
            community.MemberCount = 1;
            await _context.SaveChangesAsync();

            return new CreateCommunityResult { Success = true, Slug = slug };
        }

        public async Task<ServiceResult> ToggleMembershipAsync(int communityId, string userId)
        {
            var member = await _context.CommunityMembers
                .FirstOrDefaultAsync(cm => cm.CommunityId == communityId && cm.UserId == userId);

            var community = await _context.Communities.FindAsync(communityId);
            if (community == null)
            {
                return ServiceResult.ErrorResult("Community not found.");
            }

            if (member != null)
            {
                _context.CommunityMembers.Remove(member);
                community.MemberCount = Math.Max(0, community.MemberCount - 1);
            }
            else
            {
                _context.CommunityMembers.Add(new CommunityMember
                {
                    UserId = userId,
                    CommunityId = communityId,
                    Role = "member",
                    JoinedAt = DateTime.UtcNow
                });
                community.MemberCount++;
            }

            await _context.SaveChangesAsync();
            return ServiceResult.SuccessResult();
        }

        public async Task<List<MemberViewModel>> GetCommunityMembersAsync(int communityId, int page = 1)
        {
            const int pageSize = 50;
            var skip = (page - 1) * pageSize;

            return await _context.CommunityMembers
                .Where(cm => cm.CommunityId == communityId)
                .OrderByDescending(cm => cm.JoinedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(cm => new MemberViewModel
                {
                    UserId = cm.UserId,
                    DisplayName = _context.UserProfiles
                        .Where(up => up.UserId == cm.UserId)
                        .Select(up => up.DisplayName)
                        .FirstOrDefault() ?? "Unknown",
                    JoinedAt = cm.JoinedAt,
                    Role = cm.Role
                })
                .ToListAsync();
        }

        public async Task<bool> IsCommunityMemberAsync(int communityId, string userId)
        {
            return await _context.CommunityMembers.AnyAsync(cm => cm.CommunityId == communityId && cm.UserId == userId);
        }

        public async Task<bool> IsCommunityAdminAsync(int communityId, string userId)
        {
            return await _context.CommunityMembers.AnyAsync(cm => cm.CommunityId == communityId && cm.UserId == userId && cm.Role == "admin");
        }

        public async Task<bool> IsCommunityModeratorAsync(int communityId, string userId)
        {
            return await _context.CommunityMembers.AnyAsync(cm => cm.CommunityId == communityId && cm.UserId == userId && (cm.Role == "moderator" || cm.Role == "admin"));
        }

        public async Task<ServiceResult> UpdateCommunityDetailsAsync(CommunityDetailViewModel model)
        {
            var community = await _context.Communities.FindAsync(model.CommunityId);
            if (community == null)
            {
                return ServiceResult.ErrorResult("Community not found.");
            }

            // You might want to add permission checks here, e.g., only admins can update
            // if (!await IsCommunityAdminAsync(model.CommunityId, model.CurrentUserId)) { /* return permission error */ }

            community.Title = model.Title;
            community.Description = model.Description;
            community.ShortDescription = model.Description?.Length > 500 ?
                model.Description.Substring(0, 497) + "..." : model.Description;
            community.CategoryId = model.CategoryId;
            community.CommunityType = model.CommunityType;
            community.IsNSFW = model.IsNSFW;
            community.Rules = model.Rules;
            community.IconUrl = model.IconUrl;
            community.BannerUrl = model.BannerUrl;
            community.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ServiceResult.SuccessResult();
        }

        public async Task<ServiceResult> DeleteCommunityAsync(int communityId, string userId)
        {
            var community = await _context.Communities.FindAsync(communityId);
            if (community == null)
            {
                return ServiceResult.ErrorResult("Community not found.");
            }

            // Only the creator or a global admin should be able to truly delete/soft delete a community
            if (community.CreatorId != userId /* && !UserIsGlobalAdmin(userId) */)
            {
                return ServiceResult.ErrorResult("You don't have permission to delete this community.");
            }

            // Soft delete
            community.IsDeleted = true;
            community.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ServiceResult.SuccessResult();
        }

        public async Task<ServiceResult> BanUserFromCommunityAsync(int communityId, string userIdToBan, string moderatorId)
        {
            // Implement logic for banning a user from a community
            // This would likely involve a new table/entity for BannedCommunityMembers
            // For now, let's assume it removes the user from CommunityMembers and marks them
            // as banned in a new property (e.g., IsBanned) or a separate collection.

            var communityMember = await _context.CommunityMembers
                .FirstOrDefaultAsync(cm => cm.CommunityId == communityId && cm.UserId == userIdToBan);

            if (communityMember == null)
            {
                return ServiceResult.ErrorResult("User is not a member of this community.");
            }

            // Check if moderatorId has permission to ban
            if (!await IsCommunityModeratorAsync(communityId, moderatorId))
            {
                return ServiceResult.ErrorResult("You do not have permission to ban users in this community.");
            }

            // Simple removal for now, consider a proper 'BannedCommunityMember' entity for more robust tracking
            _context.CommunityMembers.Remove(communityMember);
            var community = await _context.Communities.FindAsync(communityId);
            if (community != null)
            {
                community.MemberCount = Math.Max(0, community.MemberCount - 1);
            }
            await _context.SaveChangesAsync();

            // Potentially add a record to a BannedUsers table for this community
            // Example: _context.BannedCommunityUsers.Add(new BannedCommunityUser { CommunityId = communityId, UserId = userIdToBan, BannedBy = moderatorId, BannedAt = DateTime.UtcNow });
            // await _context.SaveChangesAsync();

            return ServiceResult.SuccessResult();
        }

        public async Task<ServiceResult> PromoteDemoteCommunityMemberAsync(int communityId, string userId, string newRole, string moderatorId)
        {
            if (newRole != "member" && newRole != "moderator" && newRole != "admin")
            {
                return ServiceResult.ErrorResult("Invalid role specified. Role must be 'member', 'moderator', or 'admin'.");
            }

            var communityMember = await _context.CommunityMembers
                .FirstOrDefaultAsync(cm => cm.CommunityId == communityId && cm.UserId == userId);

            if (communityMember == null)
            {
                return ServiceResult.ErrorResult("User is not a member of this community.");
            }

            // Logic to prevent promoting/demoting self
            if (userId == moderatorId)
            {
                return ServiceResult.ErrorResult("You cannot change your own role.");
            }

            // Permission checks:
            // Only an admin can promote/demote other moderators/admins
            // Only an admin/moderator can promote/demote members
            var moderatorRole = await GetCommunityMemberRoleAsync(communityId, moderatorId);
            if (moderatorRole == null || (moderatorRole != "admin" && moderatorRole != "moderator"))
            {
                return ServiceResult.ErrorResult("You do not have sufficient permissions to change roles in this community.");
            }

            // If moderator tries to change an admin's role
            if (moderatorRole == "moderator" && communityMember.Role == "admin")
            {
                return ServiceResult.ErrorResult("Moderators cannot change the role of an admin.");
            }

            // If an admin tries to promote to admin, but they are not the creator, you might want to add more checks
            if (newRole == "admin" && moderatorRole != "admin")
            {
                return ServiceResult.ErrorResult("Only existing administrators can promote users to admin role.");
            }


            communityMember.Role = newRole;
            await _context.SaveChangesAsync();

            return ServiceResult.SuccessResult();
        }

        public async Task<string?> GetCommunityMemberRoleAsync(int communityId, string userId)
        {
            var member = await _context.CommunityMembers
                .FirstOrDefaultAsync(cm => cm.CommunityId == communityId && cm.UserId == userId);
            return member?.Role;
        }

        public async Task<List<CommunityViewModel>> GetUserJoinedCommunitiesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<CommunityViewModel>();
            }

            return await _context.CommunityMembers
                .Where(cm => cm.UserId == userId && !cm.Community!.IsDeleted)
                .Select(cm => new CommunityViewModel
                {
                    CommunityId = cm.CommunityId,
                    Name = cm.Community!.Name,
                    Slug = cm.Community.Slug,
                    Title = cm.Community.Title,
                    Description = cm.Community.ShortDescription,
                    IconUrl = cm.Community.IconUrl,
                    MemberCount = cm.Community.MemberCount
                })
                .OrderBy(cm => cm.Title) // Order by title for consistent display
                .ToListAsync();
        }

        public async Task<List<CommunityViewModel>> GetSuggestedCommunitiesAsync(string userId, int count = 5)
        {
            // For suggested communities, we can fetch communities the user is NOT a member of.
            // You can enhance this logic based on categories the user has interacted with,
            // popular communities they haven't joined, or other criteria.

            var joinedCommunityIds = await _context.CommunityMembers
                                                .Where(cm => cm.UserId == userId)
                                                .Select(cm => cm.CommunityId)
                                                .ToListAsync();

            var suggestedCommunities = await _context.Communities
                .Where(c => !c.IsDeleted && !joinedCommunityIds.Contains(c.CommunityId))
                .OrderByDescending(c => c.MemberCount) // Suggest popular unjoined communities
                .Take(count)
                .Select(c => new CommunityViewModel
                {
                    CommunityId = c.CommunityId,
                    Name = c.Name,
                    Slug = c.Slug,
                    Title = c.Title,
                    Description = c.ShortDescription,
                    IconUrl = c.IconUrl,
                    MemberCount = c.MemberCount
                })
                .ToListAsync();

            return suggestedCommunities;
        }

    }
}
