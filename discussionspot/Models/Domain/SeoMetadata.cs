using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using discussionspot.Data;
using discussionspot.Data.discussionspot.Data;

namespace discussionspot.Models.Domain
{
    /// <summary>
    /// Represents SEO metadata for communities and posts
    /// </summary>
    public class SeoMetadata : BaseEntity
    {
        [Key, Column(Order = 0)]
        [StringLength(20)]
        [Display(Name = "Entity Type")]
        public string EntityType { get; set; } = string.Empty;  // community, post

        [Key, Column(Order = 1)]
        [Display(Name = "Entity ID")]
        public int EntityId { get; set; }

        [StringLength(200)]
        [Display(Name = "Meta Title")]
        public string? MetaTitle { get; set; }

        [StringLength(500)]
        [Display(Name = "Meta Description")]
        public string? MetaDescription { get; set; }

        [StringLength(2048)]
        [Display(Name = "Canonical URL")]
        [DataType(DataType.Url)]
        public string? CanonicalUrl { get; set; }

        [StringLength(200)]
        [Display(Name = "Open Graph Title")]
        public string? OgTitle { get; set; }

        [StringLength(500)]
        [Display(Name = "Open Graph Description")]
        public string? OgDescription { get; set; }

        [StringLength(2048)]
        [Display(Name = "Open Graph Image URL")]
        [DataType(DataType.Url)]
        public string? OgImageUrl { get; set; }

        [StringLength(20)]
        [Display(Name = "Twitter Card Type")]
        public string TwitterCard { get; set; } = "summary";

        [StringLength(200)]
        [Display(Name = "Twitter Title")]
        public string? TwitterTitle { get; set; }

        [StringLength(500)]
        [Display(Name = "Twitter Description")]
        public string? TwitterDescription { get; set; }

        [StringLength(2048)]
        [Display(Name = "Twitter Image URL")]
        [DataType(DataType.Url)]
        public string? TwitterImageUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Keywords")]
        public string? Keywords { get; set; }

        [Display(Name = "Structured Data")]
        public string? StructuredData { get; set; }  // JSON-LD format

        // Navigation properties (no standard navigation properties since this is a shared table)

        // Helper methods
        /// <summary>
        /// Gets the associated community, if applicable
        /// </summary>
        /// <param name="context">Database context</param>
        /// <returns>Associated community or null</returns>
        public Community? GetCommunity(ApplicationDbContext context)
        {
            if (EntityType != "community") return null;
            return context.Communities.Find(EntityId);
        }

        /// <summary>
        /// Gets the associated post, if applicable
        /// </summary>
        /// <param name="context">Database context</param>
        /// <returns>Associated post or null</returns>
        public Post? GetPost(ApplicationDbContext context)
        {
            if (EntityType != "post") return null;
            return context.Posts.Find(EntityId);
        }

        /// <summary>
        /// Generates SEO metadata for a post
        /// </summary>
        /// <param name="post">Post to generate metadata for</param>
        public void GenerateFromPost(Post post)
        {
            EntityType = "post";
            EntityId = post.PostId;

            // Generate meta data
            MetaTitle = post.Title;
            MetaDescription = post.Content?.Length > 160
                ? post.Content.Substring(0, 157) + "..."
                : post.Content;

            // Open Graph
            OgTitle = post.Title;
            OgDescription = MetaDescription;
            if (post.Media?.Any() == true && post.Media.Any(m => m.MediaType == "image"))
            {
                OgImageUrl = post.Media.FirstOrDefault(m => m.MediaType == "image")?.Url;
            }

            // Twitter
            TwitterTitle = post.Title;
            TwitterDescription = MetaDescription;
            TwitterImageUrl = OgImageUrl;
            TwitterCard = string.IsNullOrEmpty(OgImageUrl) ? "summary" : "summary_large_image";

            // Keywords
            var tags = post.PostTags?.Select(pt => pt.Tag.Name).ToList() ?? new List<string>();
            if (post.Community != null)
            {
                tags.Add(post.Community.Name);
            }
            Keywords = string.Join(", ", tags);

            // Structured data (JSON-LD)
            StructuredData = $@"{{
                ""@context"": ""https://schema.org"",
                ""@type"": ""DiscussionForumPosting"",
                ""headline"": ""{post.Title}"",
                ""datePublished"": ""{post.CreatedAt:yyyy-MM-ddTHH:mm:sszzz}"",
                ""dateModified"": ""{post.UpdatedAt:yyyy-MM-ddTHH:mm:sszzz}"",
                ""author"": {{
                    ""@type"": ""Person"",
                    ""name"": ""{post.User?.UserName ?? "Anonymous"}""
                }},
                ""discussionUrl"": ""{post.ExternalUrl}""
            }}";
        }

        /// <summary>
        /// Generates SEO metadata for a community
        /// </summary>
        /// <param name="community">Community to generate metadata for</param>
        public void GenerateFromCommunity(Community community)
        {
            EntityType = "community";
            EntityId = community.CommunityId;

            // Generate meta data
            MetaTitle = community.Title;
            MetaDescription = community.ShortDescription ?? community.Description;
            if (MetaDescription?.Length > 160)
            {
                MetaDescription = MetaDescription.Substring(0, 157) + "...";
            }

            // Open Graph
            OgTitle = community.Title;
            OgDescription = MetaDescription;
            OgImageUrl = community.BannerUrl ?? community.IconUrl;

            // Twitter
            TwitterTitle = community.Title;
            TwitterDescription = MetaDescription;
            TwitterImageUrl = OgImageUrl;
            TwitterCard = string.IsNullOrEmpty(OgImageUrl) ? "summary" : "summary_large_image";

            // Keywords
            var keywords = new List<string> { community.Name };
            if (community.Category != null)
            {
                keywords.Add(community.Category.Name);
            }
            Keywords = string.Join(", ", keywords);

            // Structured data (JSON-LD)
            StructuredData = $@"{{
                ""@context"": ""https://schema.org"",
                ""@type"": ""DiscussionForumPosting"",
                ""headline"": ""{community.Title}"",
                ""datePublished"": ""{community.CreatedAt:yyyy-MM-ddTHH:mm:sszzz}"",
                ""dateModified"": ""{community.UpdatedAt:yyyy-MM-ddTHH:mm:sszzz}"",
                ""author"": {{
                    ""@type"": ""Person"",
                    ""name"": ""{community.Creator?.UserName ?? "Anonymous"}""
                }}
            }}";
        }
    }
}
