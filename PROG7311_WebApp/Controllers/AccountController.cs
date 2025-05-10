using Microsoft.AspNetCore.Identity; // Needed for UserManager and SignInManager
using Microsoft.AspNetCore.Mvc;
using PROG7311_WebApp.Models; // Needed for ApplicationUser and LoginViewModel
using System.Threading.Tasks; // For async operations

namespace PROG7311_WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        // Constructor to inject Identity services
        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        
        // Displays the login form to the user.
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl; // Store return URL if user was redirected from a protected page
            return View();
        }

        
        // Handles the submitted login form data.
        [HttpPost]
        [ValidateAntiForgeryToken] // Stops Cross-Site Request Forgery attacks
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl ??= Url.Content("~/"); // Default redirect to homepage if no returnUrl

            if (ModelState.IsValid) 
            {
                // Attempt to sign in the user using their password.
                
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, // Using Email as UserName for login
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true); // lock out users after too many failed attempts

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {model.Email} logged in successfully.");
                    // Redirect to the returnUrl or homepage.
                    return LocalRedirect(returnUrl);
                }
                // Handle other sign-in results like lockout
                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"User account {model.Email} locked out.");
                    ModelState.AddModelError(string.Empty, "This account has been locked out, please try again later.");
                }
                else if (result.RequiresTwoFactor)
                {
                    // Not implemented
                    return RedirectToAction("LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                }
                else
                {
                    // Generic error if login fails for other reasons
                    ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
                }
            }

            // If ModelState is invalid or login fails, redisplay the form with errors.
            return View(model);
        }

        
        // Handles user logout.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name; // Get username for logging before signing out
            await _signInManager.SignOutAsync(); // Clears the authentication cookie
            _logger.LogInformation($"User {userName} logged out successfully.");
            // Redirect to homepage after logout
            return RedirectToAction("Index", "Home");
        }

        
        // Displays a page when an authorized user tries to access a resource they don't have permission for.
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}