using discussionspot9.Data.DbContext;
using discussionspot9.Interfaces;
using discussionspot9.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace discussionspot9.Services
{
    public class NotificationPreferenceService : INotificationPreferenceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationPreferenceService> _logger;

        // Default notification types
        private readonly string[] _defaultNotificationTypes = new[]
        {
            "comment", "reply", "vote", "follow", "mention",
            "award", "community_post", "milestone", "announcement", "message"
        };

        public NotificationPreferenceService(
            ApplicationDbContext context,
            ILogger<NotificationPreferenceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserNotificationSettings?> GetUserSettingsAsync(string userId)
        {
            try
            {
                var settings = await _context.UserNotificationSettings.FindAsync(userId);
                
                // Create default settings if none exist
                if (settings == null)
                {
                    settings = new UserNotificationSettings
                    {
                        UserId = userId,
                        EmailNotificationsEnabled = true,
                        WebNotificationsEnabled = true,
                        PushNotificationsEnabled = false,
                        EmailDigestFrequency = "never",
                        QuietHoursEnabled = false,
                        GroupNotifications = true,
                        ShowNotificationPreviews = true,
                        PlayNotificationSound = false,
                        UnsubscribeFromAll = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.UserNotificationSettings.Add(settings);
                    await _context.SaveChangesAsync();
                }

                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user settings for {UserId}", userId);
                return null;
            }
        }

        public async Task<List<NotificationPreference>> GetUserPreferencesAsync(string userId)
        {
            try
            {
                var preferences = await _context.NotificationPreferences
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                // If no preferences exist, initialize defaults
                if (!preferences.Any())
                {
                    await InitializeDefaultPreferencesAsync(userId);
                    preferences = await _context.NotificationPreferences
                        .Where(p => p.UserId == userId)
                        .ToListAsync();
                }

                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preferences for {UserId}", userId);
                return new List<NotificationPreference>();
            }
        }

        public async Task<NotificationPreference?> GetPreferenceAsync(string userId, string notificationType)
        {
            try
            {
                return await _context.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == notificationType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preference for {UserId}, type {Type}", userId, notificationType);
                return null;
            }
        }

        public async Task<bool> UpdateUserSettingsAsync(string userId, UserNotificationSettings settings)
        {
            try
            {
                var existing = await _context.UserNotificationSettings.FindAsync(userId);
                
                if (existing == null)
                {
                    _context.UserNotificationSettings.Add(settings);
                }
                else
                {
                    existing.EmailNotificationsEnabled = settings.EmailNotificationsEnabled;
                    existing.WebNotificationsEnabled = settings.WebNotificationsEnabled;
                    existing.PushNotificationsEnabled = settings.PushNotificationsEnabled;
                    existing.EmailDigestFrequency = settings.EmailDigestFrequency;
                    existing.QuietHoursEnabled = settings.QuietHoursEnabled;
                    existing.QuietHoursStart = settings.QuietHoursStart;
                    existing.QuietHoursEnd = settings.QuietHoursEnd;
                    existing.GroupNotifications = settings.GroupNotifications;
                    existing.ShowNotificationPreviews = settings.ShowNotificationPreviews;
                    existing.PlayNotificationSound = settings.PlayNotificationSound;
                    existing.UnsubscribeFromAll = settings.UnsubscribeFromAll;
                    existing.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("User settings updated for {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user settings for {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UpdatePreferenceAsync(string userId, string notificationType, bool webEnabled, bool emailEnabled, string emailFrequency)
        {
            try
            {
                var preference = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == notificationType);

                if (preference == null)
                {
                    preference = new NotificationPreference
                    {
                        UserId = userId,
                        NotificationType = notificationType,
                        WebEnabled = webEnabled,
                        EmailEnabled = emailEnabled,
                        EmailFrequency = emailFrequency,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.NotificationPreferences.Add(preference);
                }
                else
                {
                    preference.WebEnabled = webEnabled;
                    preference.EmailEnabled = emailEnabled;
                    preference.EmailFrequency = emailFrequency;
                    preference.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating preference for {UserId}, type {Type}", userId, notificationType);
                return false;
            }
        }

        public async Task<bool> SetAllPreferencesAsync(string userId, Dictionary<string, (bool web, bool email, string frequency)> preferences)
        {
            try
            {
                foreach (var pref in preferences)
                {
                    await UpdatePreferenceAsync(userId, pref.Key, pref.Value.web, pref.Value.email, pref.Value.frequency);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting all preferences for {UserId}", userId);
                return false;
            }
        }

        public async Task InitializeDefaultPreferencesAsync(string userId)
        {
            try
            {
                foreach (var type in _defaultNotificationTypes)
                {
                    var exists = await _context.NotificationPreferences
                        .AnyAsync(p => p.UserId == userId && p.NotificationType == type);

                    if (!exists)
                    {
                        var preference = new NotificationPreference
                        {
                            UserId = userId,
                            NotificationType = type,
                            WebEnabled = true,
                            EmailEnabled = true,
                            PushEnabled = false,
                            EmailFrequency = "instant",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.NotificationPreferences.Add(preference);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Default preferences initialized for {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing preferences for {UserId}", userId);
            }
        }

        public async Task<bool> ShouldNotifyAsync(string userId, string notificationType, string channel)
        {
            try
            {
                // Check global settings
                var settings = await GetUserSettingsAsync(userId);
                if (settings != null)
                {
                    if (settings.UnsubscribeFromAll)
                        return false;

                    if (channel == "email" && !settings.EmailNotificationsEnabled)
                        return false;

                    if (channel == "web" && !settings.WebNotificationsEnabled)
                        return false;

                    if (channel == "push" && !settings.PushNotificationsEnabled)
                        return false;

                    // Check quiet hours for any channel
                    if (await IsInQuietHoursAsync(userId))
                        return false;
                }

                // Check specific notification type preference
                var preference = await GetPreferenceAsync(userId, notificationType);
                if (preference != null)
                {
                    if (channel == "email" && (!preference.EmailEnabled || preference.EmailFrequency == "never"))
                        return false;

                    if (channel == "web" && !preference.WebEnabled)
                        return false;

                    if (channel == "push" && !preference.PushEnabled)
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if should notify {UserId}", userId);
                return true; // Default to allowing on error
            }
        }

        public async Task<bool> IsInQuietHoursAsync(string userId)
        {
            try
            {
                var settings = await GetUserSettingsAsync(userId);
                
                if (settings == null || !settings.QuietHoursEnabled || !settings.QuietHoursStart.HasValue || !settings.QuietHoursEnd.HasValue)
                    return false;

                var now = DateTime.UtcNow.TimeOfDay;
                var start = settings.QuietHoursStart.Value;
                var end = settings.QuietHoursEnd.Value;

                if (start < end)
                {
                    return now >= start && now <= end;
                }
                else
                {
                    // Crosses midnight
                    return now >= start || now <= end;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking quiet hours for {UserId}", userId);
                return false;
            }
        }
    }
}

