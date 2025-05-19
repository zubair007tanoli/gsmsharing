using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot.Repositories
{
    public class TagRepository : EfRepository<Tag>, ITagRepository
    {
        private readonly ApplicationDbContext _context;

        public TagRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Tag?> GetTagBySlugAsync(string slug)
        {
            return await _context.Tags.FirstOrDefaultAsync(t => t.Slug == slug);
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count)
        {
            return await _context.Tags
                .OrderByDescending(t => t.PostCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> SearchTagsAsync(string searchTerm)
        {
            return await _context.Tags
                .Where(t => t.Name.Contains(searchTerm))
                .OrderByDescending(t => t.PostCount)
                .ToListAsync();
        }

        public async Task<Tag?> GetOrCreateTagAsync(string name)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);

            if (tag == null)
            {
                tag = new Tag
                {
                    Name = name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                tag.GenerateSlug();

                // Check if slug exists and make it unique if needed
                string baseSlug = tag.Slug;
                int counter = 1;
                while (await SlugExistsAsync(tag.Slug))
                {
                    tag.Slug = $"{baseSlug}-{counter}";
                    counter++;
                }

                await _context.Tags.AddAsync(tag);
                await _context.SaveChangesAsync();
            }

            return tag;
        }

        public async Task UpdatePostCountAsync(int tagId)
        {
            var tag = await _context.Tags.FindAsync(tagId);
            if (tag != null)
            {
                tag.PostCount = await _context.PostTags.CountAsync(pt => pt.TagId == tagId);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Tags.AnyAsync(t => t.Slug == slug);
        }
    }
}