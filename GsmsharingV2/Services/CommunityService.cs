using AutoMapper;
using GsmsharingV2.Database;
using GsmsharingV2.DTOs;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GsmsharingV2.Services
{
    public class CommunityService : ICommunityService
    {
        private readonly ICommunityRepository _communityRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CommunityService> _logger;

        public CommunityService(
            ICommunityRepository communityRepository,
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<CommunityService> logger)
        {
            _communityRepository = communityRepository;
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CommunityDto?> GetByIdAsync(int id)
        {
            try
            {
                var community = await _communityRepository.GetByIdAsync(id);
                if (community == null) return null;

                return _mapper.Map<CommunityDto>(community);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting community by ID: {CommunityId}", id);
                throw;
            }
        }

        public async Task<CommunityDto?> GetBySlugAsync(string slug)
        {
            try
            {
                var community = await _communityRepository.GetBySlugAsync(slug);
                if (community == null) return null;

                return _mapper.Map<CommunityDto>(community);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting community by slug: {Slug}", slug);
                throw;
            }
        }

        public async Task<IEnumerable<CommunityDto>> GetAllAsync()
        {
            try
            {
                var communities = await _communityRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<CommunityDto>>(communities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all communities");
                throw;
            }
        }

        public async Task<IEnumerable<CommunityDto>> GetByCategoryIdAsync(int categoryId)
        {
            try
            {
                var communities = await _context.Communities
                    .Include(c => c.Creator)
                    .Include(c => c.Category)
                    .Where(c => c.CategoryID == categoryId)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<CommunityDto>>(communities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting communities by category ID: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<IEnumerable<CommunityDto>> GetByUserIdAsync(string userId)
        {
            try
            {
                var communities = await _context.Communities
                    .Include(c => c.Creator)
                    .Include(c => c.Category)
                    .Where(c => c.CreatorId == userId)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<CommunityDto>>(communities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting communities by user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<CommunityDto> CreateAsync(CreateCommunityDto createCommunityDto, string userId)
        {
            try
            {
                var community = _mapper.Map<Community>(createCommunityDto);
                community.CreatorId = userId;
                community.CreatedAt = DateTime.UtcNow;
                community.UpdatedAt = DateTime.UtcNow;
                community.MemberCount = 1; // Creator is first member

                var createdCommunity = await _communityRepository.CreateAsync(community);

                // Add creator as member
                var member = new CommunityMember
                {
                    CommunityID = createdCommunity.CommunityID,
                    UserId = userId,
                    Role = "Admin",
                    JoinedAt = DateTime.UtcNow
                };
                _context.CommunityMembers.Add(member);
                await _context.SaveChangesAsync();

                return _mapper.Map<CommunityDto>(createdCommunity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating community for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<CommunityDto> UpdateAsync(UpdateCommunityDto updateCommunityDto, string userId)
        {
            try
            {
                var existingCommunity = await _communityRepository.GetByIdAsync(updateCommunityDto.CommunityID);
                if (existingCommunity == null)
                {
                    throw new KeyNotFoundException($"Community with ID {updateCommunityDto.CommunityID} not found");
                }

                if (existingCommunity.CreatorId != userId)
                {
                    throw new UnauthorizedAccessException("User does not have permission to update this community");
                }

                _mapper.Map(updateCommunityDto, existingCommunity);
                existingCommunity.UpdatedAt = DateTime.UtcNow;

                var updatedCommunity = await _communityRepository.UpdateAsync(existingCommunity);
                return _mapper.Map<CommunityDto>(updatedCommunity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating community: {CommunityId} for user: {UserId}", updateCommunityDto.CommunityID, userId);
                throw;
            }
        }

        public async Task DeleteAsync(int id, string userId)
        {
            try
            {
                var community = await _communityRepository.GetByIdAsync(id);
                if (community == null)
                {
                    throw new KeyNotFoundException($"Community with ID {id} not found");
                }

                if (community.CreatorId != userId)
                {
                    throw new UnauthorizedAccessException("User does not have permission to delete this community");
                }

                await _communityRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting community: {CommunityId} for user: {UserId}", id, userId);
                throw;
            }
        }

        public async Task<bool> JoinCommunityAsync(int communityId, string userId)
        {
            try
            {
                var existingMember = await _context.CommunityMembers
                    .FirstOrDefaultAsync(m => m.CommunityID == communityId && m.UserId == userId);

                if (existingMember != null)
                {
                    return false; // Already a member
                }

                var member = new CommunityMember
                {
                    CommunityID = communityId,
                    UserId = userId,
                    Role = "Member",
                    JoinedAt = DateTime.UtcNow
                };

                _context.CommunityMembers.Add(member);

                // Update member count
                var community = await _context.Communities.FindAsync(communityId);
                if (community != null)
                {
                    community.MemberCount = (community.MemberCount ?? 0) + 1;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining community: {CommunityId} for user: {UserId}", communityId, userId);
                throw;
            }
        }

        public async Task<bool> LeaveCommunityAsync(int communityId, string userId)
        {
            try
            {
                var member = await _context.CommunityMembers
                    .FirstOrDefaultAsync(m => m.CommunityID == communityId && m.UserId == userId);

                if (member == null)
                {
                    return false; // Not a member
                }

                _context.CommunityMembers.Remove(member);

                // Update member count
                var community = await _context.Communities.FindAsync(communityId);
                if (community != null)
                {
                    community.MemberCount = Math.Max(0, (community.MemberCount ?? 0) - 1);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving community: {CommunityId} for user: {UserId}", communityId, userId);
                throw;
            }
        }

        public async Task<bool> IsMemberAsync(int communityId, string userId)
        {
            try
            {
                return await _context.CommunityMembers
                    .AnyAsync(m => m.CommunityID == communityId && m.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking membership: {CommunityId} for user: {UserId}", communityId, userId);
                throw;
            }
        }
    }
}

