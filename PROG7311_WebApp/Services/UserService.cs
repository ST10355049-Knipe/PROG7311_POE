// Services/UserService.cs
using Microsoft.AspNetCore.Identity;
using PROG7311_WebApp.Models;
using System.Collections.Generic;
using System.Linq; // Added for Select() in error logging
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROG7311_WebApp.Services
{
    // This service was created to follow my lecturer's feedback about moving logic
    // out of the controllers. It now handles all the direct calls to ASP.NET Core Identity's
    // UserManager and SignInManager.
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           ILogger<UserService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<SignInResult> LoginUserAsync(LoginViewModel model)
        {
            // This is the core login logic using SignInManager.
            // PasswordSignInAsync handles checking the hashed password, setting the cookie, and lockout policies.
            // Ref for SignInManager: https://dotnettutorials.net/lesson/usermanager-signinmanager-rolemanager-in-asp-net-core-identity/
            return await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true); // Important security feature.
        }

        public async Task LogoutUserAsync()
        {
            // Calls SignInManager to clear the user's authentication cookie.
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> CreateFarmerAsync(AddFarmerViewModel model)
        {
            var farmerUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            // This multi-step process (create user, then add to role) is now handled in one service method.
            // UserManager.CreateAsync handles the password hashing and user creation.
            // General Identity concepts ref: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-9.0&tabs=visual-studio
            var result = await _userManager.CreateAsync(farmerUser, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning($"Failed to create user {model.Email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return result;
            }

            // If the user was created, we immediately try to add them to the "Farmer" role.
            _logger.LogInformation($"User {farmerUser.Email} created, now adding to Farmer role.");
            var roleResult = await _userManager.AddToRoleAsync(farmerUser, "Farmer");
            if (!roleResult.Succeeded)
            {
                // This is an important cleanup step. If role assignment fails, we shouldn't
                // leave a user in the system without their intended permissions.
                _logger.LogWarning($"Failed to add user {farmerUser.Email} to Farmer role. Deleting orphaned user.");
                await _userManager.DeleteAsync(farmerUser);
            }
            return roleResult;
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            // A helper method to get the full user object from the database
            // based on the claims principal from the controller.
            return await _userManager.GetUserAsync(user);
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName)
        {
            // Helper method for getting all users that belong to a specific role.
            // Used in the Employee controller to populate the farmer filter dropdown.
            // Ref for UserManager features: https://dotnettutorials.net/lesson/usermanager-signinmanager-rolemanager-in-asp-net-core-identity/
            return await _userManager.GetUsersInRoleAsync(roleName);
        }
    }
}