using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace discussionspot.Models
{
    public class PollConfiguration
    {
        public int PostId { get; set; }
        public bool AllowMultipleChoices { get; set; } = false;
        public DateTime? EndDate { get; set; }
        public bool ShowResultsBeforeVoting { get; set; } = true;
        public bool ShowResultsBeforeEnd { get; set; } = true;
        public bool AllowAddingOptions { get; set; } = false;
        public int MinOptions { get; set; } = 2;
        public int MaxOptions { get; set; } = 10;
    }
}
