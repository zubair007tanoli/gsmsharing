using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class MediaRepository : EfRepository<Media>, IMediaRepository
    {
        private readonly ApplicationDbContext _context;

        public MediaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Media>> GetMediaForPostAsync(int postId)
        {
            return await _context.Media
                .Where(m => m.PostId == postId)
                .OrderBy(m => m.UploadedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Media>> GetUserUploadsAsync(string userId, int page, int pageSize)
        {
            return await _context.Media
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Media>> GetMediaByTypeAsync(string mediaType, int page, int pageSize)
        {
            return await _context.Media
                .Where(m => m.MediaType == mediaType)
                .OrderByDescending(m => m.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}