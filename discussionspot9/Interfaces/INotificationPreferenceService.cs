using discussionspot9.Models.Domain;

namespace discussionspot9.Interfaces
{
    public interface INotificationPreferenceService
    {
        // Get preferences
        Task<UserNotificationSettings?> GetUserSettingsAsync(string userId);
        Task<List<NotificationPreference>> GetUserPreferencesAsync(string userId);
        Task<NotificationPreference?> GetPreferenceAsync(string userId, string notificationType);
        
        // Update preferences
        Task<bool> UpdateUserSettingsAsync(string userId, UserNotificationSettings settings);
        Task<bool> UpdatePreferenceAsync(string userId, string notificationType, bool webEnabled, bool emailEnabled, string emailFrequency);
        Task<bool> SetAllPreferencesAsync(string userId, Dictionary<string, (bool web, bool email, string frequency)> preferences);
        
        // Initialize default preferences for new users
        Task InitializeDefaultPreferencesAsync(string userId);
        
        // Utility
        Task<bool> ShouldNotifyAsync(string userId, string notificationType, string channel); // channel: web, email, push
        Task<bool> IsInQuietHoursAsync(string userId);
    }
}

