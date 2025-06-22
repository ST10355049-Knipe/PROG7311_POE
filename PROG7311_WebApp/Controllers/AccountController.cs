using Microsoft.AspNetCore.Identity; // Still needed for SignInResult
using Microsoft.AspNetCore.Mvc;
using PROG7311_WebApp.Models;
using PROG7311_WebApp.Services; // Now using our custom service
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PROG7311_WebApp.Controllers
{
    public class AccountController : Controller
    {
        // To follow my lecturer's feedback, I've moved the direct calls to SignInManager
        // into a custom service. This controller now only depends on IUserService.
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        // The constructor is now simpler, only injecting the services this controller directly uses.
        public AccountController(IUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: /Account/Login
        // This action's job is just to display the login form.
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Storing the returnUrl so we know where to send the user after they log in.
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        // This action handles the login form submission.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl ??= Url.Content("~/"); // Default to homepage if no returnUrl.

            // The controller's first job is to check if the incoming data from the form is valid.
            if (ModelState.IsValid)
            {
                // The controller now delegates the actual work of logging in to the UserService.
                // It doesn't need to know about SignInManager or password hashing anymore.
                var result = await _userService.LoginUserAsync(model);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {model.Email} logged in successfully.");
                    return LocalRedirect(returnUrl); // Using LocalRedirect for security.
                }

                // The controller is still responsible for handling the result of the service call
                // and updating the UI (ModelState) accordingly.
                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"User account {model.Email} locked out.");
                    ModelState.AddModelError(string.Empty, "This account has been locked out, please try again later.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
                }
            }

            // If we get here, something failed, so we show the form again with error messages.
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name;
            // Just like login, logout logic is now handled by the service.
            await _userService.LogoutUserAsync();
            _logger.LogInformation($"User {userName} logged out successfully.");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        // Simple action to show the Access Denied view if a user is logged in
        // but tries to access a page they don't have the role for.
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}