using gsmsharing.ViewModels;
using System.Text.RegularExpressions;

namespace gsmsharing.ExeMethods
{
    public static class PostViewModelExtensions
    {
        public static void SetDefaultSeoValues(this PostViewModel post)
        {
            if (string.IsNullOrWhiteSpace(post.MetaTitle))
            {
                post.MetaTitle = post.Title?.Length > 60
                    ? post.Title.Substring(0, 57) + "..."
                    : post.Title;
            }

            if (string.IsNullOrWhiteSpace(post.OgTitle))
            {
                post.OgTitle = post.MetaTitle;
            }

            if (string.IsNullOrWhiteSpace(post.MetaDescription))
            {
                // Generate meta description from summary or content
                var description = !string.IsNullOrWhiteSpace(post.Summary)
                    ? post.Summary
                    : StripHtmlAndTruncate(post.Content, 155);

                post.MetaDescription = description?.Length > 160
                    ? description.Substring(0, 157) + "..."
                    : description;
            }

            if (string.IsNullOrWhiteSpace(post.OgDescription))
            {
                post.OgDescription = post.MetaDescription;
            }

            if (string.IsNullOrWhiteSpace(post.Slug))
            {
                post.Slug = GenerateSlug(post.Title);
            }
        }

        private static string StripHtmlAndTruncate(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // Remove HTML tags
            var text = Regex.Replace(input, "<.*?>", string.Empty);

            // Remove extra spaces and new lines
            text = Regex.Replace(text, @"\s+", " ").Trim();

            return text.Length > maxLength
                ? text.Substring(0, maxLength - 3) + "..."
                : text;
        }

        private static string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title)) return string.Empty;

            // Convert to lowercase and replace spaces with hyphens
            var slug = title.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("&", "and");

            // Remove any invalid characters
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", string.Empty);

            // Remove multiple hyphens
            slug = Regex.Replace(slug, @"-+", "-");

            // Remove leading/trailing hyphens
            slug = slug.Trim('-');

            return slug;
        }
    }
}
