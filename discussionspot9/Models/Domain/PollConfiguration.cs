using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace discussionspot9.Models.Domain
{
    public class PollConfiguration
    {
        [Key, ForeignKey("Post")]
        public int PostId { get; set; }

        public bool AllowMultipleChoices { get; set; }

        public DateTime? EndDate { get; set; }

        public bool ShowResultsBeforeVoting { get; set; }

        public bool ShowResultsBeforeEnd { get; set; }

        public bool AllowAddingOptions { get; set; }

        public int MinOptions { get; set; }

        public int MaxOptions { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string? PollQuestion { get; set; }

        public string? PollDescription { get; set; }

        public string? ClosedByUserId { get; set; }

        public DateTime? ClosedAt { get; set; }

        // Navigation properties
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("ClosedByUserId")]
        public virtual Microsoft.AspNetCore.Identity.IdentityUser? ClosedByUser { get; set; }
    }
}
