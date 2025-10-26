using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace discussionspot9.Repositories
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync()
        {
            var now = DateTime.UtcNow;
            
            return await _context.Announcements
                .Where(a => a.IsActive 
                    && (a.StartDate == null || a.StartDate <= now)
                    && (a.EndDate == null || a.EndDate >= now))
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync()
        {
            return await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Announcement?> GetAnnouncementByIdAsync(int id)
        {
            return await _context.Announcements.FindAsync(id);
        }

        public async Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
        {
            announcement.CreatedAt = DateTime.UtcNow;
            announcement.UpdatedAt = DateTime.UtcNow;
            
            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();
            
            return announcement;
        }

        public async Task<Announcement> UpdateAnnouncementAsync(Announcement announcement)
        {
            announcement.UpdatedAt = DateTime.UtcNow;
            
            _context.Announcements.Update(announcement);
            await _context.SaveChangesAsync();
            
            return announcement;
        }

        public async Task<bool> DeleteAnnouncementAsync(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return false;

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<int> GetActiveAnnouncementCountAsync()
        {
            var now = DateTime.UtcNow;
            
            return await _context.Announcements
                .Where(a => a.IsActive 
                    && (a.StartDate == null || a.StartDate <= now)
                    && (a.EndDate == null || a.EndDate >= now))
                .CountAsync();
        }
    }
}

