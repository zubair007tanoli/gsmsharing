using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace discussionspot9.Helpers
{
    public static class DisplayHelpers
    {
        public static string ToTimeAgo(this DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();
            return timeSpan.TotalSeconds switch
            {
                < 60 => "just now",
                < 3600 => $"{(int)timeSpan.TotalMinutes}m ago",
                < 86400 => $"{(int)timeSpan.TotalHours}h ago",
                _ => dateTime.ToString("MMM dd, yyyy") // Fallback for older dates
            };
        }

        public static string ToInitials(this string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return "U";
            var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
        }

        public static string ToAvatarColor(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "gray"; // Default color

            var colors = new string[] { "#4f46e5", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#06b6d4", "#a855f7", "#ec4899" };
            int hash = 0;
            foreach (char c in input)
            {
                hash = c + ((hash << 5) - hash);
            }
            return colors[Math.Abs(hash) % colors.Length];
        }

        public static string ToDomain(this string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            try
            {
                var uri = new Uri(url);
                return uri.Host.Replace("www.", "");
            }
            catch
            {
                return "External Link";
            }
        }

        // If you have rich text content that needs to be displayed as raw HTML
        // and you want to ensure it's properly sanitized, you might use an HtmlHelper
        // For now, Html.Raw(Model.Content) is used, but for user-generated content,
        // proper sanitization (e.g., using a library like HtmlSanitizer) is crucial.
        public static IHtmlContent RawHtml(this IHtmlHelper htmlHelper, string html)
        {
            // In a real application, you would sanitize `html` before returning.
            // For example:
            // var sanitizer = new HtmlSanitizer();
            // var sanitizedHtml = sanitizer.Sanitize(html);
            // return new HtmlString(sanitizedHtml);
            return new HtmlString(html); // CAUTION: Directly rendering raw HTML can lead to XSS vulnerabilities.
        }
    }
}
