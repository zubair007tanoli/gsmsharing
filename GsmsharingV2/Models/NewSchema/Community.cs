namespace GsmsharingV2.Models.NewSchema
{
    public class Community
    {
        public long CommunityID { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string BannerUrl { get; set; }
        public long? CreatorID { get; set; }
        public long? CategoryID { get; set; }
        public int MemberCount { get; set; } = 0;
        public int PostCount { get; set; } = 0;
        public bool IsPrivate { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

