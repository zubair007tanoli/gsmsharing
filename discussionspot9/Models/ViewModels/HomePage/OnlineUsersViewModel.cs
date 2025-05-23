// Models/ViewModels/HomePage/OnlineUsersViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace discussionspot9.Models.ViewModels.HomePage
{
    /// <summary>
    /// ViewModel for online users section in sidebar
    /// </summary>
    public class OnlineUsersViewModel
    {
        /// <summary>
        /// Total number of users currently online
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TotalOnline { get; set; }

        /// <summary>
        /// Peak online users for today
        /// </summary>
        [Range(0, int.MaxValue)]
        public int PeakToday { get; set; }

        /// <summary>
        /// Time when peak was reached
        /// </summary>
        public DateTime PeakTime { get; set; }

        /// <summary>
        /// List of users to display (usually first few)
        /// </summary>
        public List<OnlineUserViewModel> VisibleUsers { get; set; } = new();

        /// <summary>
        /// Count of users active in each category
        /// </summary>
        public Dictionary<string, int> ActiveInCategories { get; set; } = new();

        /// <summary>
        /// Number of additional users not shown in visible list
        /// </summary>
        public int HiddenUserCount => Math.Max(0, TotalOnline - VisibleUsers.Count);

        /// <summary>
        /// Whether there are users to display
        /// </summary>
        public bool HasVisibleUsers => VisibleUsers.Any();

        /// <summary>
        /// Whether there are hidden users
        /// </summary>
        public bool HasHiddenUsers => HiddenUserCount > 0;

        /// <summary>
        /// Formatted peak time text
        /// </summary>
        public string PeakTimeText => PeakTime.ToString("h:mm tt");

        /// <summary>
        /// Whether peak was reached today
        /// </summary>
        public bool PeakWasToday => PeakTime.Date == DateTime.Today;

        /// <summary>
        /// Categories with active users
        /// </summary>
        public IEnumerable<KeyValuePair<string, int>> ActiveCategories =>
            ActiveInCategories.Where(kv => kv.Value > 0).OrderByDescending(kv => kv.Value);

        /// <summary>
        /// Whether there are categories with active users
        /// </summary>
        public bool HasActiveCategories => ActiveInCategories.Any(kv => kv.Value > 0);

        /// <summary>
        /// Total users across all categories (may be different from TotalOnline if some users are browsing outside categories)
        /// </summary>
        public int TotalInCategories => ActiveInCategories.Values.Sum();

        /// <summary>
        /// Maximum number of users to show in visible list
        /// </summary>
        public const int MaxVisibleUsers = 5;

        /// <summary>
        /// Text for the hidden users count
        /// </summary>
        public string HiddenUsersText => HiddenUserCount > 0 ? $"+{HiddenUserCount}" : "";

        /// <summary>
        /// Status color class based on online count
        /// </summary>
        public string OnlineStatusClass => TotalOnline switch
        {
            >= 50 => "text-success", // High activity
            >= 20 => "text-warning", // Medium activity
            >= 1 => "text-info",     // Low activity
            _ => "text-muted"        // No activity
        };

        /// <summary>
        /// Activity level description
        /// </summary>
        public string ActivityLevel => TotalOnline switch
        {
            >= 50 => "Very Active",
            >= 20 => "Active",
            >= 10 => "Moderate",
            >= 1 => "Quiet",
            _ => "No Activity"
        };
    }
}