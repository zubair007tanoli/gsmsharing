using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class UserProfileRepository : EfRepository<UserProfile>, IUserProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public UserProfileRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<UserProfile?> GetProfileByUserIdAsync(string userId)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<IEnumerable<UserProfile>> GetTopUsersByKarmaAsync(int count)
        {
            return await _context.UserProfiles
                .OrderByDescending(p => p.KarmaPoints)
                .Take(count)
                .ToListAsync();
        }

        public async Task UpdateKarmaPointsAsync(string userId, int points)
        {
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile != null)
            {
                profile.KarmaPoints += points;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<UserProfile>> GetRecentlyActiveUsersAsync(int count)
        {
            return await _context.UserProfiles
                .OrderByDescending(p => p.LastActive)
                .Take(count)
                .ToListAsync();
        }
    }
}
