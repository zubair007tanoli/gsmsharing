namespace discussionspot9.Extensions
{
    public static class StringExtensions
    {
        public static string ToSlug(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Convert to lowercase, remove invalid characters, and replace spaces with hyphens
            return input.ToLower()
                        .Replace(" ", "-")
                        .Replace(".", "")
                        .Replace(",", "")
                        .Replace("!", "")
                        .Replace("?", "")
                        .Replace("'", "")
                        .Replace("\"", "")
                        .Replace(";", "")
                        .Replace(":", "")
                        .Replace("/", "")
                        .Replace("\\", "")
                        .Replace("&", "and");
        }
    }
}
