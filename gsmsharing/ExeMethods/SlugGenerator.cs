using System.Text.RegularExpressions;

namespace gsmsharing.ExeMethods
{
    public static class SlugGenerator
    {
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var slug = text.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("_", "-");

            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
            slug = Regex.Replace(slug, @"[-\s]+", "-");
            return slug.Trim('-');
        }

        public static async Task<string> EnsureUniqueSlugAsync(string baseSlug,
            Func<string, Task<bool>> existsCheck)
        {
            var slug = baseSlug;
            var counter = 1;

            while (await existsCheck(slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        public static string ShortenTitle(string title)
        {
            int maxLength = 60;
            if (string.IsNullOrEmpty(title)) return "";

            if (title.Length <= maxLength)
                return title;

            string[] words = title.Split(' ');
            string shortTitle = "";

            foreach (string word in words)
            {
                if ((shortTitle + " " + word).Length > maxLength)
                    break;
                shortTitle += (shortTitle == "" ? "" : " ") + word;
            }

            return shortTitle.TrimEnd() + "...";
        }

        public static string ShortenDescription(string description)
        {
            int maxLength = 160;
            if (string.IsNullOrEmpty(description)) return "";

            if (description.Length <= maxLength)
                return description;

            string[] words = description.Split(' ');
            string shortDesc = "";

            foreach (string word in words)
            {
                if ((shortDesc + " " + word).Length > maxLength)
                    break;
                shortDesc += (shortDesc == "" ? "" : " ") + word;
            }

            return shortDesc.TrimEnd() + "...";
        }
    }
}
