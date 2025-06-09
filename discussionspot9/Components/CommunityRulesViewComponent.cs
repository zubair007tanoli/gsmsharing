using Microsoft.AspNetCore.Mvc;

namespace discussionspot9.Components
{
    public class CommunityRulesViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string communitySlug, string rules)
        {
            // Here you would typically fetch the community rules from a database or service
            // For demonstration purposes, we'll use a static string
            var communityRules = string.IsNullOrEmpty(rules)
                ? "No specific rules defined for this community."
                : rules;
            return View("Default", communityRules);
        }
    }
}
