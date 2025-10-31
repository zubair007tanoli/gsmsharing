namespace discussionspot9.Constants
{
    /// <summary>
    /// Constants for Story-related values to avoid magic strings throughout the codebase
    /// </summary>
    public static class StoryConstants
    {
        // Story Status Values
        public const string StatusDraft = "draft";
        public const string StatusPublished = "published";
        public const string StatusArchived = "archived";

        // Slide Types
        public const string SlideTypeMedia = "media";
        public const string SlideTypeText = "text";
        public const string SlideTypeVideo = "video";
        public const string SlideTypeImage = "image";

        // Default Values
        public const int DefaultDuration = 5000; // milliseconds
        public const string DefaultBackgroundColor = "#667eea";
        public const string DefaultTextColor = "#FFFFFF";
        public const string DefaultAlignment = "center";

        // Pagination
        public const int IndexPageSize = 10;
        public const int ExplorePageSize = 12;
        public const int RelatedStoriesCount = 6;

        // Story Generation Styles
        public const string StyleInformative = "informative";
        public const string StyleCreative = "creative";
        public const string StyleEducational = "educational";

        // Story Lengths
        public const string LengthShort = "short";
        public const string LengthMedium = "medium";
        public const string LengthLong = "long";
    }
}

