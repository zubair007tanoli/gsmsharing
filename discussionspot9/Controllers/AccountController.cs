using discussionspot9.Models.ViewModels;
using discussionspot9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using discussionspot9.Helpers;

namespace DiscussionSpot9.Controllers
{
    public class AccountController(IUserService userService, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) : Controller
    {
        private readonly IUserService _userService = userService;
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly SignInManager<IdentityUser> _signInManager = signInManager;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Auth(string? returnUrl = null)
        {
            var model = new AuthViewModel
            {
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };
            return View(model);
        }
        #region Registration
        [HttpGet]
        [RedirectAuthenticated]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind(Prefix = "RegisterModel")] RegisterViewModel registerViewModel, string? returnUrl = null)
        {
            // Clear validation errors for LoginModel since we're only processing registration
            var loginModelKeys = ModelState.Keys.Where(k => k.StartsWith("LoginModel.")).ToList();
            foreach (var key in loginModelKeys)
            {
                ModelState.Remove(key);
            }

            // FIX: Explicitly set ReturnUrl from the parameter
            registerViewModel.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                var authViewModel = new AuthViewModel
                {
                    RegisterModel = registerViewModel,
                    ReturnUrl = returnUrl ?? Url.Content("~/"),
                    ShowRegister = true  // Important to show register form on error
                };
                return View("Auth", authViewModel);
            }

            var result = await _userService.RegisterUserAsync(registerViewModel);

            if (!result.Succeeded)
            {
                TempData["ErrorMessageUserName"] = "User Name Already Exist Choose Another Name.";
                result.Errors.ToList().ForEach(error => ModelState.AddModelError(string.Empty, error.Description));

                return RedirectToAction("Auth", new { returnUrl = returnUrl });
            }

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Welcome to DiscussionSpot, {registerViewModel.DisplayName}! Your account has been created successfully.";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction("Auth", new { returnUrl = returnUrl });
        }
        #endregion

        #region Login
        [HttpGet]
        [RedirectAuthenticated]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind(Prefix = "LoginModel")] LoginViewModel loginViewModel, string? returnUrl = null)
        {
            // FIX: Explicitly set ReturnUrl from the parameter
            loginViewModel.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                var authViewModel = new AuthViewModel
                {
                    LoginModel = loginViewModel,
                    ReturnUrl = returnUrl ?? Url.Content("~/"),
                    ShowRegister = false  // Show login form
                };
                return View("Auth", authViewModel);
            }

            var result = await _userService.LoginUserAsync(loginViewModel);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Welcome back!";

                if (!string.IsNullOrEmpty(loginViewModel.ReturnUrl) && Url.IsLocalUrl(loginViewModel.ReturnUrl))
                {
                    return Redirect(loginViewModel.ReturnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account has been locked due to multiple failed login attempts. Please try again later.");
            }
            else if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Your account is not confirmed. Please check your email for confirmation instructions.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return RedirectToAction("Auth", new { returnUrl = loginViewModel.ReturnUrl });
        }
        #endregion

        #region Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutUserAsync();
            TempData["InfoMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Profile Management
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var profile = await _userService.GetUserProfileAsync(userId);

            if (profile == null)
            {
                return NotFound();
            }

            return View(profile);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _userService.UpdateUserProfileAsync(userId, model);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Your profile has been updated successfully.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        #endregion

        #region Password Management
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _userService.ChangePasswordAsync(userId, model);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Your password has been changed successfully.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult NewPassword()
        {
            return View();
        }
        #endregion

        #region Email Confirmation

        [HttpGet]
        public IActionResult ConfirmEmail()
        {

            return View();
        }

        [HttpPost]
        public IActionResult ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                return NotFound();
            }


            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Public Profile View
        [HttpGet]
        public async Task<IActionResult> ViewUser(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                return NotFound();
            }

            var userProfile = await _userService.GetUserProfileByDisplayNameAsync(displayName);
            if (userProfile == null)
            {
                return NotFound();
            }

            var userStats = await _userService.GetUserStatsAsync(userProfile.UserId);
            if (userStats == null)
            {
                return NotFound();
            }

            return View(userStats);
        }
        #endregion

        #region AJAX Endpoints
        [HttpGet]
        public async Task<IActionResult> CheckDisplayName(string displayName, string? currentUserId = null)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                return Json(new { available = false, message = "Display name is required." });
            }

            var isAvailable = !await _userService.IsDisplayNameTakenAsync(displayName, currentUserId);
            var message = isAvailable ? "Display name is available." : "This display name is already taken.";

            return Json(new { available = isAvailable, message });
        }
        #endregion

        #region Access Denied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        #endregion

        #region External Login (Google OAuth)

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (remoteError != null)
            {
                TempData["ErrorMessage"] = $"Error from external provider: {remoteError}";
                return RedirectToAction("Auth");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["ErrorMessage"] = "Error loading external login information.";
                return RedirectToAction("Auth");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                // If the user does not have an account, create one
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email?.Split('@')[0] ?? "User";

                if (string.IsNullOrEmpty(email))
                {
                    TempData["ErrorMessage"] = "Email not received from provider.";
                    return RedirectToAction("Auth");
                }

                var user = new IdentityUser { UserName = email, Email = email };
                var createResult = await _userManager.CreateAsync(user);

                if (createResult.Succeeded)
                {
                    // Add external login
                    createResult = await _userManager.AddLoginAsync(user, info);
                    
                    if (createResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                TempData["ErrorMessage"] = "Unable to create account. Please try again.";
                return RedirectToAction("Auth");
            }
        }

        #endregion
    }
}