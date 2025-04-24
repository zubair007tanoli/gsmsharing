using System.ComponentModel.DataAnnotations;

namespace discussionspot.ViewModels
{
    public class SeoMetadataViewModel
    {
        public string EntityType { get; set; }
        public int EntityId { get; set; }

        [StringLength(200)]
        public string MetaTitle { get; set; }

        [StringLength(500)]
        public string MetaDescription { get; set; }

        [StringLength(2048)]
        public string CanonicalUrl { get; set; }

        [StringLength(200)]
        public string OgTitle { get; set; }

        [StringLength(500)]
        public string OgDescription { get; set; }

        [StringLength(2048)]
        public string OgImageUrl { get; set; }

        [StringLength(20)]
        public string TwitterCard { get; set; } = "summary";

        [StringLength(200)]
        public string TwitterTitle { get; set; }

        [StringLength(500)]
        public string TwitterDescription { get; set; }

        [StringLength(2048)]
        public string TwitterImageUrl { get; set; }

        [StringLength(500)]
        public string Keywords { get; set; }

        public string StructuredData { get; set; }
    }
}
