using System;

namespace discussionspot9.Helpers
{
    /// <summary>
    /// Helper class for generating user avatars and initials
    /// </summary>
    public static class AvatarHelper
    {
        private static readonly string[] DefaultColors = new string[]
        {
            "#FF5733", "#33FF57", "#3357FF", "#FF33FF", "#33FFFF", "#FFFF33",
            "#FF6347", "#4682B4", "#8A2BE2", "#D2691E", "#6B8E23", "#FFD700",
            "#E91E63", "#9C27B0", "#673AB7", "#3F51B5", "#2196F3", "#00BCD4",
            "#009688", "#4CAF50", "#8BC34A", "#FFC107", "#FF9800", "#FF5722"
        };

        /// <summary>
        /// Generates a URL for an avatar based on the display name.
        /// If a custom avatar URL is provided, it is returned.
        /// Otherwise, a placeholder image with initials and a random background color is generated.
        /// </summary>
        /// <param name="displayName">The user's display name.</param>
        /// <param name="customAvatarUrl">Optional: A custom avatar URL if available.</param>
        /// <param name="size">The desired size of the avatar (e.g., 64 for 64x64px).</param>
        /// <returns>A URL for the avatar image (either custom or generated SVG data URI).</returns>
        public static string GetAvatarUrl(string? displayName, string? customAvatarUrl, int size = 64)
        {
            // If custom avatar is provided, use it
            if (!string.IsNullOrEmpty(customAvatarUrl))
            {
                return customAvatarUrl;
            }

            // Fallback to default if no display name
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = "User";
            }

            // Generate initials
            string initials = GetInitials(displayName);

            // Get a consistent color based on the display name hash
            int hash = Math.Abs(displayName.GetHashCode());
            string color = DefaultColors[hash % DefaultColors.Length];

            // Generate SVG with initials and background color
            string svg = GenerateAvatarSvg(initials, color, size);

            // Return as base64 data URI
            return $"data:image/svg+xml;base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg))}";
        }

        /// <summary>
        /// Extracts initials from a display name.
        /// </summary>
        /// <param name="displayName">The full display name.</param>
        /// <returns>The initials (e.g., "JD" for "John Doe").</returns>
        public static string GetInitials(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return "??";
            }

            string[] parts = displayName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 0)
            {
                return "??";
            }
            else if (parts.Length == 1)
            {
                // Single word: take first 2 characters
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            }
            else
            {
                // Multiple words: take first character of first two words
                return (parts[0].Substring(0, 1) + parts[1].Substring(0, 1)).ToUpper();
            }
        }

        /// <summary>
        /// Generates an SVG avatar with initials and background color
        /// </summary>
        private static string GenerateAvatarSvg(string initials, string backgroundColor, int size)
        {
            // Calculate font size (approximately 40% of avatar size)
            int fontSize = (int)(size * 0.4);

            return $@"<svg width='{size}' height='{size}' viewBox='0 0 {size} {size}' xmlns='http://www.w3.org/2000/svg'>
                <rect width='{size}' height='{size}' fill='{backgroundColor}' rx='4'/>
                <text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' fill='white' font-family='Arial, Helvetica, sans-serif' font-size='{fontSize}px' font-weight='600'>{initials}</text>
            </svg>";
        }

        /// <summary>
        /// Get a color based on a string (for consistent avatar colors)
        /// </summary>
        /// <param name="input">Input string (e.g., username, email)</param>
        /// <returns>Hex color string</returns>
        public static string GetColorForString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return DefaultColors[0];
            }

            int hash = Math.Abs(input.GetHashCode());
            return DefaultColors[hash % DefaultColors.Length];
        }
    }
}
