using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.RegularExpressions;

namespace discussionspot9.Helpers
{
    public static class UrlHelperExtensions
    {
        #region User Profile URLs

        /// <summary>
        /// Generate clean user profile URL: /u/username
        /// </summary>
        public static string UserProfile(this IUrlHelper urlHelper, string displayName)
        {
            return urlHelper.RouteUrl("user_profile", new { displayName }) ?? $"/u/{displayName}";
        }

        /// <summary>
        /// Generate user posts URL: /u/username/posts
        /// </summary>
        /// 
        public static string Profile(this IUrlHelper urlHelper, string displayName)
        {
            return urlHelper.UserProfile(displayName);
        }
        public static string UserPosts(this IUrlHelper urlHelper, string displayName)
        {
            return urlHelper.RouteUrl("user_posts", new { displayName }) ?? $"/u/{displayName}/posts";
        }

        /// <summary>
        /// Generate user comments URL: /u/username/comments
        /// </summary>
        public static string UserComments(this IUrlHelper urlHelper, string displayName)
        {
            return urlHelper.RouteUrl("user_comments", new { displayName }) ?? $"/u/{displayName}/comments";
        }

        #endregion

        #region Authentication URLs

        /// <summary>
        /// Generate combined auth URL: /auth
        /// </summary>
        public static string Auth(this IUrlHelper urlHelper, string? returnUrl = null)
        {
            var url = urlHelper.RouteUrl("auth_combined") ?? "/auth";
            return AppendReturnUrl(url, returnUrl);
        }

        /// <summary>
        /// Generate clean login URL: /login
        /// </summary>
        public static string Login(this IUrlHelper urlHelper, string? returnUrl = null)
        {
            var url = urlHelper.RouteUrl("auth_login") ?? "/login";
            return AppendReturnUrl(url, returnUrl);
        }

        /// <summary>
        /// Generate clean register URL: /register
        /// </summary>
        public static string Register(this IUrlHelper urlHelper, string? returnUrl = null)
        {
            var url = urlHelper.RouteUrl("auth_register") ?? "/register";
            return AppendReturnUrl(url, returnUrl);
        }

        /// <summary>
        /// Generate logout URL: /logout
        /// </summary>
        public static string Logout(this IUrlHelper urlHelper)
        {
            return urlHelper.RouteUrl("auth_logout") ?? "/logout";
        }

        /// <summary>
        /// Generate access denied URL: /access-denied
        /// </summary>
        public static string AccessDenied(this IUrlHelper urlHelper)
        {
            return urlHelper.RouteUrl("access_denied") ?? "/access-denied";
        }

        #endregion

        #region Account Management URLs

        /// <summary>
        /// Generate account profile URL: /account/profile
        /// </summary>
        public static string AccountProfile(this IUrlHelper urlHelper)
        {
            return urlHelper.RouteUrl("account_profile") ?? "/account/profile";
        }

        /// <summary>
        /// Generate account settings URL: /account/settings
        /// </summary>
        public static string AccountSettings(this IUrlHelper urlHelper)
        {
            return urlHelper.RouteUrl("account_settings") ?? "/account/settings";
        }

        /// <summary>
        /// Generate change password URL: /account/change-password
        /// </summary>
        public static string ChangePassword(this IUrlHelper urlHelper)
        {
            return urlHelper.RouteUrl("account_password") ?? "/account/change-password";
        }

        /// <summary>
        /// Generate password reset URL: /reset-password
        /// </summary>
        public static string ResetPassword(this IUrlHelper urlHelper, string? code = null, string? email = null)
        {
            var url = urlHelper.RouteUrl("password_reset") ?? "/reset-password";

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(code))
                queryParams.Add($"code={Uri.EscapeDataString(code)}");
            if (!string.IsNullOrEmpty(email))
                queryParams.Add($"email={Uri.EscapeDataString(email)}");

            return queryParams.Any() ? $"{url}?{string.Join("&", queryParams)}" : url;
        }

        /// <summary>
        /// Generate email confirmation URL: /confirm-email
        /// </summary>
        public static string ConfirmEmail(this IUrlHelper urlHelper, string? userId = null, string? code = null)
        {
            var url = urlHelper.RouteUrl("email_confirm") ?? "/confirm-email";

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(userId))
                queryParams.Add($"userId={Uri.EscapeDataString(userId)}");
            if (!string.IsNullOrEmpty(code))
                queryParams.Add($"code={Uri.EscapeDataString(code)}");

            return queryParams.Any() ? $"{url}?{string.Join("&", queryParams)}" : url;
        }

        #endregion

        #region Community URLs

        /// <summary>
        /// Generate clean community URL: /r/communityname
        /// </summary>
        public static string Community(this IUrlHelper urlHelper, string communitySlug)
        {
            return urlHelper.RouteUrl("community_detail", new { slug = communitySlug }) ?? $"/r/{communitySlug}";
        }

        /// <summary>
        /// Generate clean post URL: /r/community/posts/post-slug
        /// </summary>
        public static string Post(this IUrlHelper urlHelper, string communitySlug, string postSlug)
        {
            return urlHelper.RouteUrl("community_posts", new { communitySlug, postSlug }) ?? $"/r/{communitySlug}/posts/{postSlug}";
        }

        /// <summary>
        /// Generate community creation URL: /create-community
        /// </summary>
        public static string CreateCommunity(this IUrlHelper urlHelper)
        {
            return urlHelper.RouteUrl("community_create") ?? "/create-community";
        }

        /// <summary>
        /// Generate post creation URL: /r/community/create
        /// </summary>
        public static string CreatePost(this IUrlHelper urlHelper, string communitySlug)
        {
            return urlHelper.RouteUrl("post_create", new { communitySlug }) ?? $"/r/{communitySlug}/create";
        }

        /// <summary>
        /// Generate communities list URL: /communities
        /// </summary>
        public static string Communities(this IUrlHelper urlHelper, int? page = null, string? sort = null)
        {
            var url = urlHelper.RouteUrl("communities_list") ?? "/communities";

            var queryParams = new List<string>();
            if (page.HasValue && page > 1)
                queryParams.Add($"page={page}");
            if (!string.IsNullOrEmpty(sort))
                queryParams.Add($"sort={Uri.EscapeDataString(sort)}");

            return queryParams.Any() ? $"{url}?{string.Join("&", queryParams)}" : url;
        }

        #endregion

        #region Search & Discovery URLs

        /// <summary>
        /// Generate search URL: /search
        /// </summary>
        public static string Search(this IUrlHelper urlHelper, string? query = null, string? type = null, string? community = null)
        {
            var url = urlHelper.RouteUrl("search") ?? "/search";

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(query))
                queryParams.Add($"q={Uri.EscapeDataString(query)}");
            if (!string.IsNullOrEmpty(type))
                queryParams.Add($"type={Uri.EscapeDataString(type)}");
            if (!string.IsNullOrEmpty(community))
                queryParams.Add($"community={Uri.EscapeDataString(community)}");

            return queryParams.Any() ? $"{url}?{string.Join("&", queryParams)}" : url;
        }

        /// <summary>
        /// Generate popular posts URL: /popular
        /// </summary>
        public static string Popular(this IUrlHelper urlHelper, string? timeframe = null)
        {
            var url = urlHelper.RouteUrl("home_popular") ?? "/popular";
            return !string.IsNullOrEmpty(timeframe) ? $"{url}?t={Uri.EscapeDataString(timeframe)}" : url;
        }

        /// <summary>
        /// Generate all posts URL: /all
        /// </summary>
        public static string All(this IUrlHelper urlHelper, string? sort = null)
        {
            var url = urlHelper.RouteUrl("home_all") ?? "/all";
            return !string.IsNullOrEmpty(sort) ? $"{url}?sort={Uri.EscapeDataString(sort)}" : url;
        }

        #endregion

        #region AJAX URLs

        /// <summary>
        /// Generate AJAX check display name URL
        /// </summary>
        public static string CheckDisplayName(this IUrlHelper urlHelper)
        {
            return urlHelper.RouteUrl("ajax_check_displayname") ?? "/ajax/account/check-displayname";
        }

        /// <summary>
        /// Generate API URL for various controllers
        /// </summary>
        public static string ApiUrl(this IUrlHelper urlHelper, string controller, string action, object? values = null)
        {
            return urlHelper.RouteUrl("api_routes", new { controller, action }) ?? $"/api/{controller}/{action}";
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Generate slug from text (for creating URL-friendly strings)
        /// </summary>
        public static string ToSlug(this string text, int maxLength = 50)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Convert to lowercase
            text = text.ToLowerInvariant();

            // Replace accented characters with their non-accented equivalents
            text = RemoveAccents(text);

            // Replace spaces with hyphens
            text = Regex.Replace(text, @"\s+", "-");

            // Remove invalid characters
            text = Regex.Replace(text, @"[^a-z0-9\-_]", "");

            // Remove multiple consecutive hyphens
            text = Regex.Replace(text, @"-+", "-");

            // Trim hyphens from start and end
            text = text.Trim('-');

            // Limit length
            if (text.Length > maxLength)
            {
                text = text.Substring(0, maxLength).TrimEnd('-');
            }

            return text;
        }

        /// <summary>
        /// Generate breadcrumb for community posts
        /// </summary>
        public static BreadcrumbItem[] GetBreadcrumb(this IUrlHelper urlHelper,
            string? communitySlug = null,
            string? communityName = null,
            string? postTitle = null)
        {
            var breadcrumb = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Url = urlHelper.Content("~/") }
            };

            if (!string.IsNullOrEmpty(communitySlug))
            {
                breadcrumb.Add(new BreadcrumbItem
                {
                    Text = $"r/{communityName ?? communitySlug}",
                    Url = urlHelper.Community(communitySlug)
                });
            }

            if (!string.IsNullOrEmpty(postTitle))
            {
                breadcrumb.Add(new BreadcrumbItem
                {
                    Text = postTitle.Length > 30 ? postTitle.Substring(0, 30) + "..." : postTitle,
                    Url = null // Current page, no link
                });
            }

            return breadcrumb.ToArray();
        }

        /// <summary>
        /// Build absolute URL for external sharing
        /// </summary>
        public static string AbsoluteUrl(this IUrlHelper urlHelper, string relativeUrl)
        {
            var request = urlHelper.ActionContext.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{relativeUrl}";
        }

        #endregion

        #region Private Helper Methods

        private static string AppendReturnUrl(string url, string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return $"{url}?returnUrl={Uri.EscapeDataString(returnUrl)}";
            }
            return url;
        }

        private static string RemoveAccents(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Normalize(System.Text.NormalizationForm.FormD);
            var chars = text.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();

            return new string(chars).Normalize(System.Text.NormalizationForm.FormC);
        }

        #endregion
    }

    /// <summary>
    /// Represents a breadcrumb navigation item
    /// </summary>
    public class BreadcrumbItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool IsActive => string.IsNullOrEmpty(Url);
    }

    /// <summary>
    /// HTML Helper Extensions for common Reddit-like UI patterns
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Generate a user link with avatar
        /// </summary>
        public static IHtmlContent UserLink(this IHtmlHelper htmlHelper,
            string displayName,
            string? avatarUrl = null,
            bool showAvatar = true,
            string? cssClass = null)
        {
            var urlHelper = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>()
                .GetUrlHelper(htmlHelper.ViewContext);
            var userUrl = urlHelper.UserProfile(displayName);

            var additionalClass = !string.IsNullOrEmpty(cssClass) ? $" {cssClass}" : "";

            if (showAvatar && !string.IsNullOrEmpty(avatarUrl))
            {
                return htmlHelper.Raw($@"<a href=""{userUrl}"" class=""user-link{additionalClass}"">
                           <img src=""{avatarUrl}"" alt=""{displayName}"" class=""user-avatar-tiny me-1"" />
                           u/{displayName}
                         </a>");
            }

            return htmlHelper.Raw($@"<a href=""{userUrl}"" class=""user-link{additionalClass}"">u/{displayName}</a>");
        }

        /// <summary>
        /// Generate a community link with optional icon
        /// </summary>
        public static IHtmlContent CommunityLink(this IHtmlHelper htmlHelper,
            string communitySlug,
            string? communityName = null,
            string? iconUrl = null,
            bool showIcon = false,
            string? cssClass = null)
        {
            var urlHelper = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>()
                .GetUrlHelper(htmlHelper.ViewContext);
            var communityUrl = urlHelper.Community(communitySlug);
            var displayName = communityName ?? communitySlug;
            var additionalClass = !string.IsNullOrEmpty(cssClass) ? $" {cssClass}" : "";

            if (showIcon && !string.IsNullOrEmpty(iconUrl))
            {
                return htmlHelper.Raw($@"<a href=""{communityUrl}"" class=""community-link{additionalClass}"">
                           <img src=""{iconUrl}"" alt=""{displayName}"" class=""community-icon-tiny me-1"" />
                           r/{displayName}
                         </a>");
            }

            return htmlHelper.Raw($@"<a href=""{communityUrl}"" class=""community-link{additionalClass}"">r/{displayName}</a>");
        }

        /// <summary>
        /// Generate relative time display (e.g., "2 hours ago")
        /// </summary>
        public static string TimeAgo(this IHtmlHelper htmlHelper, DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

            if (timeSpan.TotalSeconds < 60)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) != 1 ? "s" : "")} ago";

            return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) != 1 ? "s" : "")} ago";
        }

        /// <summary>
        /// Format large numbers with K, M, B suffixes
        /// </summary>
        public static string FormatCount(this IHtmlHelper htmlHelper, int count)
        {
            if (count < 1000)
                return count.ToString();
            if (count < 10000)
                return $"{count / 1000.0:0.#}k";
            if (count < 1000000)
                return $"{count / 1000}k";
            if (count < 10000000)
                return $"{count / 1000000.0:0.#}M";
            if (count < 1000000000)
                return $"{count / 1000000}M";

            return $"{count / 1000000000.0:0.#}B";
        }

        /// <summary>
        /// Generate vote buttons
        /// </summary>
        public static IHtmlContent VoteButtons(this IHtmlHelper htmlHelper,
            int score,
            bool? userVote = null,
            string entityType = "post",
            int entityId = 0)
        {
            var upvoteClass = userVote == true ? "active" : "";
            var downvoteClass = userVote == false ? "active" : "";

            return htmlHelper.Raw($@"
                <div class=""vote-buttons"" data-entity-type=""{entityType}"" data-entity-id=""{entityId}"">
                    <button class=""vote-btn upvote {upvoteClass}"" aria-label=""Upvote"">
                        <i class=""fas fa-arrow-up""></i>
                    </button>
                    <span class=""vote-count"">{htmlHelper.FormatCount(score)}</span>
                    <button class=""vote-btn downvote {downvoteClass}"" aria-label=""Downvote"">
                        <i class=""fas fa-arrow-down""></i>
                    </button>
                </div>
            ");
        }
    }
}