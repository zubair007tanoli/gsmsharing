using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pdfpeaks.Models;
using Pdfpeaks.Models.ViewModels;
using Serilog;
using System.Diagnostics;
using System.Security.Claims;

namespace Pdfpeaks.Controllers
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

        [AllowAnonymous]
        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName ?? model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in: {Email}", model.Email);
                
                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Dashboard");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Account is locked. Please try again later or reset your password.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if username already exists
            var existingUserName = await _userManager.FindByNameAsync(model.Username);
            if (existingUserName != null)
            {
                ModelState.AddModelError("Username", "This username is already taken.");
                return View(model);
            }

            // Check if email already exists
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SubscriptionTier = SubscriptionTier.Free,
                DownloadsRemaining = 3, // Free tier gets 3 downloads
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password: {Email}", model.Email);

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["SuccessMessage"] = $"Welcome {model.FirstName}! Your account has been created successfully.";
                return RedirectToAction("Dashboard");
            }

            // If we got this far, something failed
            foreach (var error in result.Errors)
            {
                _logger.LogWarning("Registration error: {Error}", error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }

        [Authorize]
        public IActionResult Settings()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            // Request a redirect to the external login provider
            var redirectUrl = Url.Action("ExternalLoginCallback", "UserAccounts", new { returnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                _logger.LogError("External login error: {Error}", remoteError);
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("External login info is null");
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Provider} provider", info.LoginProvider);
                
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Dashboard");
            }

            if (result.IsLockedOut)
            {
                return RedirectToAction("Login");
            }
            else
            {
                // If the user does not have an account, create one
                var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? 
                           info.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ??
                               info.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ??
                              info.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname");

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogError("External login did not provide email");
                    ModelState.AddModelError(string.Empty, "Email not provided by external login provider.");
                    return RedirectToAction("Login");
                }

                var user = new ApplicationUser
                {
                    UserName = email.Split('@')[0],
                    Email = email,
                    FirstName = firstName ?? "",
                    LastName = lastName ?? "",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    SubscriptionTier = SubscriptionTier.Free,
                    DownloadsRemaining = 3,
                    IsActive = true,
                    EmailVerified = true // External login providers verify email
                };

                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    _logger.LogInformation("User created via external login: {Email}", email);
                    
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (addLoginResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User logged in with external provider: {Provider}", info.LoginProvider);
                        
                        TempData["SuccessMessage"] = $"Welcome! Your account has been created via {info.LoginProvider}.";
                        
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Dashboard");
                    }
                }

                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "UserAccounts", new { returnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
