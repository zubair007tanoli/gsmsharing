using gsmsharing.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using gsmsharing.ViewModels;

namespace gsmsharing.ExeMethods
{
    public static class CommunityHelper
    {
        private const int MetaTitleMaxLength = 60;
        private const int MetaDescriptionMaxLength = 160;
        private const int OgTitleMaxLength = 60;

        public static void SetDefaultSeoValues(CommunityViewModel community)
        {
            if (string.IsNullOrWhiteSpace(community.MetaTitle))
            {
                community.MetaTitle = TruncateWithEllipsis(community.Name, MetaTitleMaxLength);
            }

            if (string.IsNullOrWhiteSpace(community.MetaDescription))
            {
                community.MetaDescription = TruncateWithEllipsis(community.Description, MetaDescriptionMaxLength);
            }

            if (string.IsNullOrWhiteSpace(community.OgTitle))
            {
                community.OgTitle = TruncateWithEllipsis($"{community.Name} Community", OgTitleMaxLength);
            }

            if (string.IsNullOrWhiteSpace(community.Slug))
            {
                community.Slug = GenerateSlug(community.Name);
            }
        }

        private static string TruncateWithEllipsis(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            text = text.Trim();

            if (text.Length <= maxLength)
            {
                return text;
            }

            // Reserve 3 characters for the ellipsis
            return text.Substring(0, maxLength - 3).Trim() + "...";
        }

        private static string GenerateSlug(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            // Convert to lowercase and remove accents
            text = text.ToLowerInvariant();
            text = RemoveDiacritics(text);

            // Replace spaces and invalid characters with hyphens
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            text = Regex.Replace(text, @"\s+", "-");
            text = Regex.Replace(text, @"-+", "-");

            // Trim hyphens from start and end
            text = text.Trim('-');

            // Ensure the slug doesn't exceed the maximum length
            if (text.Length > 100)
            {
                text = text.Substring(0, 100).TrimEnd('-');
            }

            return text;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
