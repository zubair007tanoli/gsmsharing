using GsmsharingV2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GsmsharingV2.Controllers
{
    public class UserAccountsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<UserAccountsController> _logger;

        public UserAccountsController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<UserAccountsController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User created a new account with password.");
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            // Get external login providers
            var externalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ViewData["ExternalLogins"] = externalLogins;
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", "UserAccounts", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                _logger.LogError("Error from external provider: {RemoteError}", remoteError);
                return RedirectToAction("Login", new { ReturnUrl = returnUrl, ErrorMessage = $"Error from external provider: {remoteError}" });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Error loading external login information.");
                return RedirectToAction("Login", new { ReturnUrl = returnUrl, ErrorMessage = "Error loading external login information." });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {LoginProvider} provider.", info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction("Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login", new { ReturnUrl = returnUrl, ErrorMessage = "Error loading external login information during confirmation." });
            }
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {LoginProvider} provider.", info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile(string? username)
        {
            ApplicationUser user;
            
            if (string.IsNullOrEmpty(username))
            {
                // View own profile
                user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }
            }
            else
            {
                // View other user's profile
                user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    return NotFound();
                }
            }

            // Get user statistics
            var dbContext = HttpContext.RequestServices.GetRequiredService<Database.ApplicationDbContext>();
            var postCount = await dbContext.Posts.CountAsync(p => p.UserId == user.Id && p.PostStatus == "published");
            var commentCount = await dbContext.Comments.CountAsync(c => c.UserId == user.Id && c.IsApproved != false);
            
            // Calculate karma (upvotes - downvotes)
            var upvotes = await dbContext.PostVotes.CountAsync(v => v.Post.UserId == user.Id && v.VoteType == 1) +
                         await dbContext.CommentVotes.CountAsync(v => v.Comment.UserId == user.Id && v.VoteType == 1);
            var downvotes = await dbContext.PostVotes.CountAsync(v => v.Post.UserId == user.Id && v.VoteType == -1) +
                           await dbContext.CommentVotes.CountAsync(v => v.Comment.UserId == user.Id && v.VoteType == -1);
            var karma = upvotes - downvotes;

            ViewBag.PostCount = postCount;
            ViewBag.CommentCount = commentCount;
            ViewBag.Karma = karma;
            ViewBag.Upvotes = upvotes;
            ViewBag.Downvotes = downvotes;

            return View(user);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var dbContext = HttpContext.RequestServices.GetRequiredService<Database.ApplicationDbContext>();
            
            // Get user statistics
            var postCount = await dbContext.Posts.CountAsync(p => p.UserId == user.Id);
            var publishedPostCount = await dbContext.Posts.CountAsync(p => p.UserId == user.Id && p.PostStatus == "published");
            var draftPostCount = await dbContext.Posts.CountAsync(p => p.UserId == user.Id && p.PostStatus == "draft");
            var commentCount = await dbContext.Comments.CountAsync(c => c.UserId == user.Id);
            var totalViews = await dbContext.Posts.Where(p => p.UserId == user.Id).SumAsync(p => p.ViewCount ?? 0);
            
            // Recent activity
            var recentPosts = await dbContext.Posts
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new { p.PostID, p.Title, p.CreatedAt, p.ViewCount, p.UpvoteCount, p.DownvoteCount })
                .ToListAsync();

            var recentComments = await dbContext.Comments
                .Where(c => c.UserId == user.Id)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new { c.CommentID, c.Content, c.CreatedAt, c.UpvoteCount })
                .ToListAsync();

            ViewBag.PostCount = postCount;
            ViewBag.PublishedPostCount = publishedPostCount;
            ViewBag.DraftPostCount = draftPostCount;
            ViewBag.CommentCount = commentCount;
            ViewBag.TotalViews = totalViews;
            ViewBag.RecentPosts = recentPosts;
            ViewBag.RecentComments = recentComments;

            return View(user);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}

