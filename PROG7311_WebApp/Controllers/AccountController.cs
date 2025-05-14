using Microsoft.AspNetCore.Identity;      // Core namespace for ASP.NET Core Identity, including SignInManager and UserManager.
using Microsoft.AspNetCore.Mvc;
using PROG7311_WebApp.Models;             // For ApplicationUser and LoginViewModel.
using System.Threading.Tasks;             // For asynchronous operations like Login and Logout.
using Microsoft.Extensions.Logging;


namespace PROG7311_WebApp.Controllers
{
    public class AccountController : Controller
    {
        // Readonly fields for injected Identity services and logger.
        // _userManager is used for user-related operations (though less in this controller, more in Employee/Farmer).
        // _signInManager is crucial here for handling user sign-in and sign-out processes.
        // _logger is for recording important events or errors during account operations.
        // Overview of these managers: https://dotnettutorials.net/lesson/usermanager-signinmanager-rolemanager-in-asp-net-core-identity/
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        // Constructor for dependency injection.
        // ASP.NET Core's DI system provides instances of UserManager, SignInManager, and ILogger.
        // This is a standard way to get access to necessary services.
        // The concept of injecting services like SignInManager is fundamental to ASP.NET Core Identity.
        // General Identity introduction Ref: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-9.0&tabs=visual-studio
        // Conceptual explanation (though older): https://www.youtube.com/watch?v=TfarnVqnhX0&ab_channel=kudvenkat (covers injecting services)
        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: /Account/Login
        // This action displays the login form to the user.
        // It can also receive a 'returnUrl' if the user was redirected from a page that requires authentication.
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Store the returnUrl in ViewData to pass it to the Login view.
            // The view's form will then include this returnUrl when posting back,
            // so the user can be redirected appropriately after a successful login.
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        // This action processes the data submitted from the login form.
        // [ValidateAntiForgeryToken] attribute helps prevent Cross-Site Request Forgery (CSRF) attacks
        // by ensuring the form submission is legitimate.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            // If returnUrl is null or empty, default to the application's homepage.
            // Url.Content("~/") generates a root-relative path to the application's base.
            returnUrl ??= Url.Content("~/");

            // ModelState.IsValid checks if the submitted data (from LoginViewModel)
            // passes the validation rules defined by DataAnnotations.
            if (ModelState.IsValid)
            {
                // Attempt to sign in the user using the email (as username) and password.
                // _signInManager.PasswordSignInAsync is the core method for this.
                // - model.Email: The username (we use email).
                // - model.Password: The password entered by the user.
                // - model.RememberMe: If true, a persistent cookie is created.
                // - lockoutOnFailure: true is a security best practice, it locks the user out
                //   after a configured number of failed login attempts.
                // Details on SignInManager Ref: https://dotnettutorials.net/lesson/usermanager-signinmanager-rolemanager-in-asp-net-core-identity/
                // General login flow in Identity Ref: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-9.0&tabs=visual-studio
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, // Using Email as UserName for login
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {model.Email} logged in successfully.");
                    // LocalRedirect is used to prevent open redirect attacks. It ensures the
                    // returnUrl is a local path within the application.
                    return LocalRedirect(returnUrl);
                }
                // Handle other possible sign-in outcomes.
                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"User account {model.Email} locked out due to too many failed attempts.");
                    ModelState.AddModelError(string.Empty, "This account has been locked out, please try again later.");
                }
                else if (result.RequiresTwoFactor)
                {
                    // This application does not currently implement Two-Factor Authentication (2FA),
                    // but this check shows how Identity supports it.
                    _logger.LogInformation($"User {model.Email} requires two-factor authentication.");
                    return RedirectToAction("LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                }
                else
                {
                    // Generic error message for other login failures (e.g., incorrect password).
                    ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
                }
            }

            // If ModelState is not valid (e.g., required fields missing) or if login failed,
            // return the Login view with the current model. This allows validation errors
            // to be displayed and the user to re-enter their details.
            return View(model);
        }

        // POST: /Account/Logout
        // Handles the user logout process.
        // Using [HttpPost] and [ValidateAntiForgeryToken] for logout is a security measure
        // to prevent malicious sites from logging users out via GET requests.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // User.Identity?.Name gets the username of the currently logged-in user for logging purposes.
            // The null-conditional operator (?.) is used in case User.Identity is null.
            var userName = User.Identity?.Name;

            // _signInManager.SignOutAsync() clears the user's authentication cookie, effectively logging them out.
            // More on SignInManager (Ref): https://dotnettutorials.net/lesson/usermanager-signinmanager-rolemanager-in-asp-net-core-identity/
            await _signInManager.SignOutAsync();
            _logger.LogInformation($"User {userName} logged out successfully.");

            // After logout, redirect the user to a safe, public page, typically the homepage.
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        // This action displays a page indicating that the currently authenticated user
        // does not have the necessary permissions/roles to access a requested resource.
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}