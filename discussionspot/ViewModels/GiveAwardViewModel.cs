using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class GiveAwardViewModel
    {
        [Required]
        public int AwardId { get; set; }

        [Required]
        public string EntityType { get; set; } // "post" or "comment"

        [Required]
        public int EntityId { get; set; }

        [StringLength(500)]
        public string Message { get; set; }

        public bool IsAnonymous { get; set; } = false;

        // For the dropdown
        public List<SelectListItem> AvailableAwards { get; set; }
    }
}
