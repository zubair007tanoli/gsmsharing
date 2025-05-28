using discussionspot9.Data.DbContext;
using discussionspot9.Helpers;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using discussionspot9.Models.ViewModels.CreativeViewModels;
using discussionspot9.Services.ServiceResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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

        public async Task<CommunityListViewModel> GetAllCommunitiesAsync(string sort = "popular", int page = 1)
        {
            const int pageSize = 20;
            var skip = (page - 1) * pageSize;

            var query = _context.Communities
                .Include(c => c.Category)
                .Where(c => !c.IsDeleted);

            query = sort switch
            {
                "new" => query.OrderByDescending(c => c.CreatedAt),
                "active" => query.OrderByDescending(c => c.Posts.Max(p => (DateTime?)p.CreatedAt) ?? c.CreatedAt),
                _ => query.OrderByDescending(c => c.MemberCount)
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
                    Title = c.Title,
                    Description = c.ShortDescription,
                    IconUrl = c.IconUrl,
                    MemberCount = c.MemberCount,
                    PostCount = c.PostCount,
                    IsNSFW = c.IsNSFW,
                    CategoryName = c.Category!.Name,
                    CategorySlug = c.Category.Slug,
                    LastActivity = c.Posts.Max(p => (DateTime?)p.CreatedAt)
                })
                .ToListAsync();

            return new CommunityListViewModel
            {
                Communities = communities,
                TotalCommunities = totalCommunities,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCommunities / (double)pageSize),
                CurrentSort = sort
            };
        }

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
    }
}