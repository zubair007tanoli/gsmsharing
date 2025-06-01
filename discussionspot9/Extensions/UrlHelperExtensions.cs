using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string Community(this IUrlHelper urlHelper, string slug)
        {
            return $"/community/{slug}";
        }
    }
}