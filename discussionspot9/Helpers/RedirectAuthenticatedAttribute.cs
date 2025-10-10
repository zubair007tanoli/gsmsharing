using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace discussionspot9.Helpers
{
    // This attribute automatically redirects authenticated users
    // away from Login/Register/Auth pages to the Home page
    public class RedirectAuthenticatedAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Not needed, but required by IActionFilter interface
        }
    }
}
