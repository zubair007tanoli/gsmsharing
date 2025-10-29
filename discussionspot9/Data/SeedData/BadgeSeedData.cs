using discussionspot9.Data.DbContext;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Data.SeedData
{
    public static class BadgeSeedData
    {
        public static async Task SeedBadgesAsync(ApplicationDbContext context)
        {
            // Check if badges already exist
            if (await context.Badges.AnyAsync())
            {
                return; // Already seeded
            }

            var badges = new List<Badge>
            {
                // ============================================
                // ACTIVITY BADGES
                // ============================================
                new Badge
                {
                    Name = "First Steps",
                    Description = "Created your first post",
                    Category = "Activity",
                    Rarity = "Common",
                    IconClass = "fas fa-baby-carriage",
                    Color = "#10b981",
                    SortOrder = 1
                },
                new Badge
                {
                    Name = "Conversation Starter",
                    Description = "Created your first comment",
                    Category = "Activity",
                    Rarity = "Common",
                    IconClass = "fas fa-comment",
                    Color = "#3b82f6",
                    SortOrder = 2
                },
                new Badge
                {
                    Name = "Century Club",
                    Description = "Created 100 posts",
                    Category = "Activity",
                    Rarity = "Rare",
                    IconClass = "fas fa-hundred-points",
                    Color = "#8b5cf6",
                    SortOrder = 3
                },
                new Badge
                {
                    Name = "Comment King",
                    Description = "Posted 1,000 comments",
                    Category = "Activity",
                    Rarity = "Epic",
                    IconClass = "fas fa-crown",
                    Color = "#f59e0b",
                    SortOrder = 4
                },
                new Badge
                {
                    Name = "Daily Devotee",
                    Description = "Active for 30 consecutive days",
                    Category = "Activity",
                    Rarity = "Rare",
                    IconClass = "fas fa-fire",
                    Color = "#ef4444",
                    SortOrder = 5
                },
                new Badge
                {
                    Name = "Early Bird",
                    Description = "One of the first 100 members",
                    Category = "Activity",
                    Rarity = "Legendary",
                    IconClass = "fas fa-egg",
                    Color = "#06b6d4",
                    SortOrder = 6
                },
                new Badge
                {
                    Name = "Dedicated",
                    Description = "Posted every day for a week",
                    Category = "Activity",
                    Rarity = "Common",
                    IconClass = "fas fa-calendar-check",
                    Color = "#14b8a6",
                    SortOrder = 7
                },

                // ============================================
                // QUALITY BADGES
                // ============================================
                new Badge
                {
                    Name = "Golden Post",
                    Description = "Post reached 1,000 upvotes",
                    Category = "Quality",
                    Rarity = "Epic",
                    IconClass = "fas fa-award",
                    Color = "#fbbf24",
                    SortOrder = 10
                },
                new Badge
                {
                    Name = "Viral Creator",
                    Description = "Post reached trending page",
                    Category = "Quality",
                    Rarity = "Epic",
                    IconClass = "fas fa-rocket",
                    Color = "#ec4899",
                    SortOrder = 11
                },
                new Badge
                {
                    Name = "Award Collector",
                    Description = "Received 50 awards",
                    Category = "Quality",
                    Rarity = "Rare",
                    IconClass = "fas fa-medal",
                    Color = "#a855f7",
                    SortOrder = 12
                },
                new Badge
                {
                    Name = "Quality Contributor",
                    Description = "10 posts with 85%+ upvote ratio",
                    Category = "Quality",
                    Rarity = "Rare",
                    IconClass = "fas fa-star",
                    Color = "#f59e0b",
                    SortOrder = 13
                },
                new Badge
                {
                    Name = "Silver Tongue",
                    Description = "Comment reached 100 upvotes",
                    Category = "Quality",
                    Rarity = "Rare",
                    IconClass = "fas fa-comment-dots",
                    Color = "#9ca3af",
                    SortOrder = 14
                },
                new Badge
                {
                    Name = "Perfectionist",
                    Description = "5 posts with 95%+ upvote ratio",
                    Category = "Quality",
                    Rarity = "Epic",
                    IconClass = "fas fa-check-double",
                    Color = "#10b981",
                    SortOrder = 15
                },

                // ============================================
                // COMMUNITY BADGES
                // ============================================
                new Badge
                {
                    Name = "Community Founder",
                    Description = "Created a community",
                    Category = "Community",
                    Rarity = "Rare",
                    IconClass = "fas fa-users",
                    Color = "#3b82f6",
                    SortOrder = 20
                },
                new Badge
                {
                    Name = "Super Moderator",
                    Description = "Moderator of 3+ communities",
                    Category = "Community",
                    Rarity = "Epic",
                    IconClass = "fas fa-shield-alt",
                    Color = "#10b981",
                    SortOrder = 21
                },
                new Badge
                {
                    Name = "Helpful Helper",
                    Description = "500+ helpful comments",
                    Category = "Community",
                    Rarity = "Rare",
                    IconClass = "fas fa-hands-helping",
                    Color = "#14b8a6",
                    SortOrder = 22
                },
                new Badge
                {
                    Name = "Ambassador",
                    Description = "Invited 10+ active users",
                    Category = "Community",
                    Rarity = "Epic",
                    IconClass = "fas fa-handshake",
                    Color = "#6366f1",
                    SortOrder = 23
                },
                new Badge
                {
                    Name = "Welcomer",
                    Description = "Welcomed 25 new members",
                    Category = "Community",
                    Rarity = "Common",
                    IconClass = "fas fa-hand-wave",
                    Color = "#fbbf24",
                    SortOrder = 24
                },
                new Badge
                {
                    Name = "Peacekeeper",
                    Description = "Helped resolve 10 disputes",
                    Category = "Community",
                    Rarity = "Rare",
                    IconClass = "fas fa-balance-scale",
                    Color = "#8b5cf6",
                    SortOrder = 25
                },

                // ============================================
                // SPECIAL BADGES
                // ============================================
                new Badge
                {
                    Name = "Verified Expert",
                    Description = "Verified by moderators as expert",
                    Category = "Special",
                    Rarity = "Legendary",
                    IconClass = "fas fa-check-circle",
                    Color = "#0ea5e9",
                    SortOrder = 30
                },
                new Badge
                {
                    Name = "Staff Member",
                    Description = "DiscussionSpot team member",
                    Category = "Special",
                    Rarity = "Legendary",
                    IconClass = "fas fa-briefcase",
                    Color = "#7c3aed",
                    SortOrder = 31
                },
                new Badge
                {
                    Name = "Beta Tester",
                    Description = "Tested features before launch",
                    Category = "Special",
                    Rarity = "Legendary",
                    IconClass = "fas fa-flask",
                    Color = "#06b6d4",
                    SortOrder = 32
                },
                new Badge
                {
                    Name = "1 Year Club",
                    Description = "Member for 1 year",
                    Category = "Special",
                    Rarity = "Rare",
                    IconClass = "fas fa-birthday-cake",
                    Color = "#ec4899",
                    SortOrder = 33
                },
                new Badge
                {
                    Name = "OG Member",
                    Description = "Member for 5+ years",
                    Category = "Special",
                    Rarity = "Legendary",
                    IconClass = "fas fa-gem",
                    Color = "#f59e0b",
                    SortOrder = 34
                },
                new Badge
                {
                    Name = "Supporter",
                    Description = "Supported the platform",
                    Category = "Special",
                    Rarity = "Rare",
                    IconClass = "fas fa-heart",
                    Color = "#ef4444",
                    SortOrder = 35
                },

                // ============================================
                // ENGAGEMENT BADGES
                // ============================================
                new Badge
                {
                    Name = "Pollster",
                    Description = "Created 10 polls",
                    Category = "Activity",
                    Rarity = "Common",
                    IconClass = "fas fa-poll",
                    Color = "#8b5cf6",
                    SortOrder = 40
                },
                new Badge
                {
                    Name = "Storyteller",
                    Description = "Created 5 stories",
                    Category = "Activity",
                    Rarity = "Common",
                    IconClass = "fas fa-book-open",
                    Color = "#ec4899",
                    SortOrder = 41
                },
                new Badge
                {
                    Name = "Social Butterfly",
                    Description = "Following 50+ users",
                    Category = "Community",
                    Rarity = "Common",
                    IconClass = "fas fa-user-friends",
                    Color = "#10b981",
                    SortOrder = 42
                },
                new Badge
                {
                    Name = "Influencer",
                    Description = "100+ followers",
                    Category = "Community",
                    Rarity = "Rare",
                    IconClass = "fas fa-bullhorn",
                    Color = "#f59e0b",
                    SortOrder = 43
                }
            };

            await context.Badges.AddRangeAsync(badges);
            await context.SaveChangesAsync();
            
            // Log success
            Console.WriteLine($"✅ Successfully seeded {badges.Count} badges");
        }
    }
}

