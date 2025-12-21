namespace GsmsharingV2.DTOs
{
    public class CommunityDto
    {
        public int CommunityID { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string Rules { get; set; }
        public string CoverImage { get; set; }
        public string IconImage { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public bool? IsPrivate { get; set; }
        public bool? IsVerified { get; set; }
        public int? MemberCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public int PostCount { get; set; }
    }

    public class CreateCommunityDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Rules { get; set; }
        public string CoverImage { get; set; }
        public string IconImage { get; set; }
        public bool? IsPrivate { get; set; }
        public int? CategoryID { get; set; }
    }

    public class UpdateCommunityDto
    {
        public int CommunityID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Rules { get; set; }
        public string CoverImage { get; set; }
        public string IconImage { get; set; }
        public bool? IsPrivate { get; set; }
        public bool? IsVerified { get; set; }
        public int? CategoryID { get; set; }
    }
}

