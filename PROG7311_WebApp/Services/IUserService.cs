// Services/IUserService.cs
using Microsoft.AspNetCore.Identity;
using PROG7311_WebApp.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PROG7311_WebApp.Services
{
    // This interface defines the contract for our custom user management service.
    // By using an interface, our controllers depend on this contract, not a specific class,
    // which makes our code more flexible and easier to test.
    public interface IUserService
    {
        Task<SignInResult> LoginUserAsync(LoginViewModel model);
        Task LogoutUserAsync();
        Task<IdentityResult> CreateFarmerAsync(AddFarmerViewModel model);
        Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user);
        Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName);
    }
}