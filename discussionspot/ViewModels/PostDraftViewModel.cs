namespace discussionspot.ViewModels
{
    public class PostDraftViewModel
    {
        public int DraftId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string PostType { get; set; }

        public int? CommunityId { get; set; }

        public string CommunityName { get; set; }

        public string CommunityIconUrl { get; set; }

        public DateTime SavedAt { get; set; }

        public DateTime LastModified { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public string Url { get; set; } // For link type drafts

        public bool HasAttachments { get; set; }

        public bool IsNSFW { get; set; }

        public bool IsSpoiler { get; set; }

        public bool HasPoll { get; set; }

        public List<string> PollOptions { get; set; } = new List<string>();

        public bool AllowMultipleChoices { get; set; }

        public DateTime? PollExpiresAt { get; set; }

        // Convert to PostCreateViewModel for editing
        public PostCreateViewModel ToCreateViewModel()
        {
            return new PostCreateViewModel
            {
                Title = this.Title,
                Content = this.Content,
                CommunityId = this.CommunityId ?? 0,
                PostType = this.PostType,
                Url = this.Url,
                IsNSFW = this.IsNSFW,
                IsSpoiler = this.IsSpoiler,
                TagsString = string.Join(", ", this.Tags),
                HasPoll = this.HasPoll,
                PollOptions = this.PollOptions,
                AllowMultipleChoices = this.AllowMultipleChoices,
                PollEndsAt = this.PollExpiresAt
            };
        }
    }
}
