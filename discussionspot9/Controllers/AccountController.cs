using discussionspot9.Models.ViewModels;
using discussionspot9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DiscussionSpot9.Controllers
{
    public class AccountController(IUserService userService, UserManager<IdentityUser> userManager) : Controller
    {
        private readonly IUserService _userService = userService;
        private readonly UserManager<IdentityUser> _userManager = userManager;

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
        public async Task<IActionResult> Register([Bind(Prefix = "RegisterModel")] RegisterViewModel registerViewModelRegisterViewModel, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(registerViewModelRegisterViewModel);
            }

            var result = await _userService.RegisterUserAsync(registerViewModelRegisterViewModel);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Welcome to DiscussionSpot, {registerViewModelRegisterViewModel.DisplayName}! Your account has been created successfully.";

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

            return View(registerViewModelRegisterViewModel);
        }
        #endregion

        #region Login
        [HttpGet]
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
        public async Task<IActionResult> Login([Bind(Prefix = "LoginModel")] LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
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

            return View(loginViewModel);
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
    }
}