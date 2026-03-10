using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Web.ViewModels;
using System.Threading.Tasks;

namespace ExpenseManager.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null, string? error = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;

            if (!string.IsNullOrEmpty(error))
                ViewData["AuthError"] = error;

            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Your account is inactive. Please contact administrator.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    
                    return RedirectToAction("Index", "Home");
                }
                
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult MicrosoftLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("Index", "Home");
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                redirectUrl = returnUrl;
            }

            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            properties.Items["prompt"] = "select_account";

            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            
            // Also sign out from OIDC if the user was authenticated via Microsoft
            if (User.Identity?.AuthenticationType == OpenIdConnectDefaults.AuthenticationScheme)
            {
                return SignOut(new AuthenticationProperties { RedirectUri = Url.Action("Login", "Account") },
                    OpenIdConnectDefaults.AuthenticationScheme);
            }

            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            TempData["Error"] = "Action not allowed";
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AuthError(string? message = null)
        {
            ViewData["ErrorMessage"] = message ?? "you are unauthorized Contact administrator.";
            return View();
        }
    }
}
