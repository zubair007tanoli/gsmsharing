using System.Text.RegularExpressions;

namespace discussionspot9.Helpers
{
    public static class MentionHelper
    {
        // Regex pattern to match @username mentions
        private static readonly Regex MentionRegex = new Regex(
            @"@([a-zA-Z0-9_]{3,50})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        /// <summary>
        /// Extract all @username mentions from text
        /// </summary>
        /// <param name="text">Text containing mentions</param>
        /// <returns>List of unique usernames mentioned (without @)</returns>
        public static List<string> ExtractMentions(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            var matches = MentionRegex.Matches(text);
            var mentions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match match in matches)
            {
                if (match.Success && match.Groups.Count > 1)
                {
                    mentions.Add(match.Groups[1].Value);
                }
            }

            return mentions.ToList();
        }

        /// <summary>
        /// Convert mentions to clickable links in HTML
        /// </summary>
        /// <param name="text">Text containing mentions</param>
        /// <returns>HTML with clickable mention links</returns>
        public static string ConvertMentionsToLinks(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            return MentionRegex.Replace(text, match =>
            {
                var username = match.Groups[1].Value;
                return $"<a href='/u/{username}' class='mention'>@{username}</a>";
            });
        }

        /// <summary>
        /// Check if text contains specific username mention
        /// </summary>
        /// <param name="text">Text to search</param>
        /// <param name="username">Username to find (without @)</param>
        /// <returns>True if username is mentioned</returns>
        public static bool ContainsMention(string text, string username)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(username))
                return false;

            var mentions = ExtractMentions(text);
            return mentions.Any(m => m.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Validate if a string is a valid username for mentions
        /// </summary>
        /// <param name="username">Username to validate</param>
        /// <returns>True if valid username format</returns>
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            // Username must be 3-50 characters, alphanumeric and underscore only
            return Regex.IsMatch(username, @"^[a-zA-Z0-9_]{3,50}$");
        }
    }
}

