using gsmsharing.Models;
using gsmsharing.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Security.Claims;

namespace gsmsharing.Controllers
{
    public class UserAccountsController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        //private readonly IEmailSender emailSender;
        private readonly IWebHostEnvironment hostEnvironment;

        public UserAccountsController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, 
            IWebHostEnvironment hostEnvironment)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.hostEnvironment = hostEnvironment;
        }

        //[HttpGet]
        //public async Task<IActionResult> ProfileAsync()
        //{
        //    try
        //    {
        //        var UserData = await dataProcessor.GetUserDataAsync("05562034-c44a-41ac-8301-b9b1987ed1a6");
        //        var (user, profile) = UserData.Value;
        //        var userModel = new UserProfileViewModel
        //        {
        //            Users = user,
        //            Profile = profile
        //        };
        //        return View(userModel);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}

        [HttpGet]
        public IActionResult Register(string? returnUrl)
        {
            RegisterViewModel registerViewModel = new();
            registerViewModel.ReturnUrl = returnUrl;
            return View(registerViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                registerViewModel.ReturnUrl = returnUrl ?? Url.Content("!/");

                var user = new ApplicationUser { UserName = registerViewModel.UserName, Email = registerViewModel.Email };
                var checkUserName = await userManager.FindByNameAsync(user.UserName);
                var CheckEmail = await userManager.FindByEmailAsync(user.Email);
                if (CheckEmail != null)
                {
                    ViewData["EmailError"] = "This Email is already Exist";

                    return View(registerViewModel);
                }
                if (checkUserName != null)
                {
                    ViewData["UsernameError"] = "This username is already taken";

                    return View(registerViewModel);
                }
                else
                {
                    var results = await userManager.CreateAsync(user, registerViewModel.Password);
                    if (results.Succeeded)
                    {
                        var roleResult = await userManager.AddToRoleAsync(user, "User");
                        if (roleResult.Succeeded)
                        {
                            Console.WriteLine("Role Assigned");
                        }
                        else
                        {
                            Console.WriteLine("Role Not Assigned");
                        }
                        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                                   new { userId = user.Id, token = token },
                                                   Request.Scheme);
                        //var paths = Path.Combine(hostEnvironment.WebRootPath, "Template/EmailConfirmation.html");
                        //var HtmlString = System.IO.File.ReadAllText(paths);
                        //HtmlString = HtmlString.Replace("{{UserName}}", registerViewModel.UserName);
                        //HtmlString = HtmlString.Replace("{{LinkAddress}}", confirmationLink);

                        //await emailSender.SendEmailAsync(registerViewModel.Email, "Confirm", HtmlString);
                        //await signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }
                }

            }

            return View(registerViewModel);
        }

        public async Task<IActionResult> ConfirmEmailAsync(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");  // Or display an error page
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");  // Or display an error page
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return RedirectToAction("EmailConfirmed", "Account");  // Or other appropriate action
            }

            // If failed, display an error or informational view
            return RedirectToAction("EmailConfirmationFailed", new { userId });
        }
        [HttpGet]
        public IActionResult EmailConfirmed()
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Check if email is already confirmed
            if (await userManager.IsEmailConfirmedAsync(user))
            {
                // Handle the case where the user's email is already confirmed 
                return RedirectToAction("EmailAlreadyConfirmed");
            }

            // Regenerate token
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                       new { userId = user.Id, token = token },
                                       Request.Scheme);
            var paths = Path.Combine(hostEnvironment.WebRootPath, "Template/EmailConfirmation.html");
            var HtmlString = System.IO.File.ReadAllText(paths);
            HtmlString = HtmlString.Replace("{{UserName}}", user.UserName);
            HtmlString = HtmlString.Replace("{{LinkAddress}}", confirmationLink);

            //await emailSender.SendEmailAsync(user.Email, "Confirm", HtmlString);
            await signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        //Password Reset Starts
        //ForgotPassword.cshtml: Form to enter the email address.
        //ForgotPasswordConfirmation.cshtml: A confirmation message.
        //ResetPassword.cshtml: Form for a new password, hidden fields for token and email.
        //ResetPasswordConfirmation.cshtml: A confirmation message after reset.

        [HttpGet]
        public IActionResult ResetPassword()
        {
            ResetPasswordViewModel resetPasswordViewModel = new();
            return View(resetPasswordViewModel);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ForgotPasswordViewModel forgotPasswordViewModel = new();
            return View(forgotPasswordViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ForgotPasswordConfirmation");
            }
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            token = WebUtility.UrlEncode(token); // Important for URL safety

            var callbackUrl = Url.Action(
            "ResetPasswordAfterMail",
            "Account",
                new { token, email = user.Email },
                protocol: Request.Scheme);


            var paths = Path.Combine(hostEnvironment.WebRootPath, "Template/EmailConfirmation.html");
            var HtmlString = System.IO.File.ReadAllText(paths);
            HtmlString = HtmlString.Replace("{{UserName}}", user.UserName);
            HtmlString = HtmlString.Replace("{{LinkAddress}}", callbackUrl);

            //await emailSender.SendEmailAsync(model.Email, "Reset Your Password", HtmlString);
            return RedirectToAction("ForgotPasswordWait");
        }
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login");
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in an existing user if the external login email already exists
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            // If the user doesn't have an account, create one with the external login info
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (email != null)
            {
                var user = new ApplicationUser { UserName = email, Email = email };
                var identityResult = await userManager.CreateAsync(user);

                if (identityResult.Succeeded)
                {
                    identityResult = await userManager.AddLoginAsync(user, info);
                    if (identityResult.Succeeded)
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
            }

            // Add additional errors for failed creation here 
            return View("Login");
        }
        [HttpGet]
        public IActionResult ForgotPasswordWait()
        {
            return View();
        }


        public IActionResult ResetPasswordAfterMail(string token, string email)
        {
            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation");
            }
            model.Token = WebUtility.UrlDecode(model.Token);
            var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }
            // ... (Add errors to ModelState if not succeeded) ...
            return RedirectToAction("ResetPasswordAfterMail");
        }
        //Password Reset Ends
        public IActionResult ResetPasswordConfirmation()
        {
            return View(); // Redirect to login          
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginViewModel loginViewModel = new();
            return View(loginViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                var LoginUser = await userManager.FindByEmailAsync(loginViewModel.Email);

                if (LoginUser != null && await userManager.CheckPasswordAsync(LoginUser, loginViewModel.Password))
                {
                    var Results = await signInManager.PasswordSignInAsync(LoginUser, loginViewModel.Password, loginViewModel.RememberMe, lockoutOnFailure: false);
                    if (Results.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                }
                // If we got this far, something failed
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(loginViewModel);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> BlockUser(string Id)
        {
            var user = await userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }

            // Lock the user account for 30 minutes
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(30));

            TempData["Message"] = "User has been blocked successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var AllUsers = await userManager.Users.ToListAsync();
            if (AllUsers.Count > 0)
            {
                var viewModel = AllUsers.Select(u => new UserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email
                });
                return View(viewModel);
            }
            else
                return RedirectToAction("AdminHome", "Admin");

        }

    }
}
