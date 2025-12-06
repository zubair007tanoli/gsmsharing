using GsmsharingV2.Database;
using GsmsharingV2.Interfaces;
using GsmsharingV2.Models;
using Microsoft.EntityFrameworkCore;

namespace GsmsharingV2.Repositories
{
    public class CommunityRepository : ICommunityRepository
    {
        private readonly ApplicationDbContext _context;

        public CommunityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Community?> GetByIdAsync(int id)
        {
            return await _context.Communities
                .Include(c => c.Creator)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.CommunityID == id);
        }

        public async Task<Community?> GetBySlugAsync(string slug)
        {
            return await _context.Communities
                .Include(c => c.Creator)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task<IEnumerable<Community>> GetAllAsync()
        {
            return await _context.Communities
                .Include(c => c.Creator)
                .Include(c => c.Category)
                .Where(c => c.IsPrivate == false || c.IsPrivate == null)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Community> CreateAsync(Community community)
        {
            community.CreatedAt = DateTime.UtcNow;
            community.UpdatedAt = DateTime.UtcNow;
            
            if (string.IsNullOrEmpty(community.Slug))
            {
                community.Slug = GenerateSlug(community.Name);
            }

            _context.Communities.Add(community);
            await _context.SaveChangesAsync();
            return community;
        }

        public async Task<Community> UpdateAsync(Community community)
        {
            community.UpdatedAt = DateTime.UtcNow;
            _context.Communities.Update(community);
            await _context.SaveChangesAsync();
            return community;
        }

        public async Task DeleteAsync(int id)
        {
            var community = await GetByIdAsync(id);
            if (community != null)
            {
                _context.Communities.Remove(community);
                await _context.SaveChangesAsync();
            }
        }

        private string GenerateSlug(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            return name.ToLower()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace("'", "")
                .Replace("\"", "");
        }
    }
}

