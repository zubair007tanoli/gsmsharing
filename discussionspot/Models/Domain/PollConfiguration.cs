using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents configuration settings for a poll
    /// </summary>
    public class PollConfiguration : BaseEntity
    {
        [Key]
        [ForeignKey("Post")]
        public int PostId { get; set; }

        [Display(Name = "Allow Multiple Choices")]
        public bool AllowMultipleChoices { get; set; } = false;

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }  // NULL means poll never expires

        [Display(Name = "Show Results Before Voting")]
        public bool ShowResultsBeforeVoting { get; set; } = true;

        [Display(Name = "Show Results Before End")]
        public bool ShowResultsBeforeEnd { get; set; } = true;

        [Display(Name = "Allow Adding Options")]
        public bool AllowAddingOptions { get; set; } = false;

        [Display(Name = "Minimum Options")]
        public int MinOptions { get; set; } = 2;

        [Display(Name = "Maximum Options")]
        public int MaxOptions { get; set; } = 10;

        // Navigation properties
        public virtual Post Post { get; set; } = null!;

        // Helper properties
        /// <summary>
        /// Gets a value indicating whether the poll has ended
        /// </summary>
        [NotMapped]
        public bool HasEnded => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;

        /// <summary>
        /// Gets the remaining time until the poll ends
        /// </summary>
        [NotMapped]
        public TimeSpan? TimeRemaining => EndDate.HasValue ? EndDate.Value - DateTime.UtcNow : null;

        /// <summary>
        /// Gets a human-readable string of the remaining time
        /// </summary>
        [NotMapped]
        public string TimeRemainingFormatted
        {
            get
            {
                if (!EndDate.HasValue) return "No end date";
                if (HasEnded) return "Poll ended";

                var timeSpan = EndDate.Value - DateTime.UtcNow;

                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} minutes remaining";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hours remaining";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays} days remaining";

                return $"{(int)(timeSpan.TotalDays / 30)} months remaining";
            }
        }
    }
}
