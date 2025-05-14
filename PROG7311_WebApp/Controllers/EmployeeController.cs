using Microsoft.AspNetCore.Authorization; // Required for the [Authorize] attribute
using Microsoft.AspNetCore.Identity;      // Core namespace for ASP.NET Core Identity services like UserManager
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Used for SelectList in ViewAllProducts
using PROG7311_WebApp.Models;             // For ViewModels like AddFarmerViewModel and ApplicationUser
using PROG7311_WebApp.Services;           // For IProductService
using System;                              // For DateTime in ViewAllProducts
using System.Linq;                         // For OrderBy in ViewAllProducts
using System.Threading.Tasks;             // For asynchronous operations

namespace PROG7311_WebApp.Controllers
{
    // The [Authorize] attribute ensures that only authenticated users can access this controller.
    // Roles = "Employee" further restricts access to users who are specifically in the "Employee" role.
    // This is a key part of implementing role-based security in ASP.NET Core Identity.
    // General concepts of Identity: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-9.0&tabs=visual-studio
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        // Readonly fields for injected services.
        // _userManager is used for managing users 
        // _productService is for product-related business logic.
        // _logger is for logging application events and errors.
        // For an overview of UserManager and other Identity services: https://dotnettutorials.net/lesson/usermanager-signinmanager-rolemanager-in-asp-net-core-identity/
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EmployeeController> _logger;
        private readonly IProductService _productService;

        // Constructor-based dependency injection.
        // ASP.NET Core's DI container provides instances of these services when the controller is created.
        // This is a standard pattern for managing dependencies.
        // Concept of injecting services (like UserManager) discussed in many tutorials, like concept from: https://www.youtube.com/watch?v=TfarnVqnhX0&ab_channel=kudvenkat
        public EmployeeController(UserManager<ApplicationUser> userManager, ILogger<EmployeeController> logger, IProductService productService)
        {
            _userManager = userManager;
            _logger = logger;
            _productService = productService;
        }

        // GET: /Employee/Index
        // Displays the main dashboard page for employees.
        public IActionResult Index()
        {
            // This view could be expanded to show summary information or quick actions for employees.
            return View();
        }

        // GET: /Employee/AddFarmer
        // Displays the form view for an employee to add a new farmer account.
        [HttpGet]
        public IActionResult AddFarmer()
        {
            // Passes a new, empty AddFarmerViewModel to the view,
            // which the form will bind to.
            return View(new AddFarmerViewModel());
        }

        // POST: /Employee/AddFarmer
        // Handles the form submission for creating a new farmer.
        // [ValidateAntiForgeryToken] helps prevent Cross-Site Request Forgery attacks.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFarmer(AddFarmerViewModel model)
        {
            // ModelState.IsValid checks if the submitted form data passes validation rules
            // defined by DataAnnotations in the AddFarmerViewModel.
            if (ModelState.IsValid)
            {
                // Create a new ApplicationUser object for the farmer.
                var farmerUser = new ApplicationUser
                {
                    UserName = model.Email, // Standard practice to use email as UserName.
                    Email = model.Email,
                    FullName = model.FullName, // Custom property for the user's full name.
                    EmailConfirmed = true      // Email is confirmed by default as an employee is creating it.
                };

                // Use UserManager to create the new user in the database.
                // This method handles password hashing and other Identity-specific setup.
                // Creating users with UserManager: https://dotnettutorials.net/lesson/usermanager-signinmanager-rolemanager-in-asp-net-core-identity/ (Covers UserManager)
                // General Identity overview: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-9.0&tabs=visual-studio
                var result = await _userManager.CreateAsync(farmerUser, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Farmer account for {model.Email} created successfully by employee {User.Identity?.Name}.");

                    // If user creation is successful, assign the "Farmer" role to the new user.
                    // This ensures they have the correct permissions within the system.
                    // Managing user roles: https://dotnettutorials.net/lesson/usermanager-signinmanager-rolemanager-in-asp-net-core-identity/ (Covers RoleManager, principles apply to UserManager role methods)
                    var roleResult = await _userManager.AddToRoleAsync(farmerUser, "Farmer");
                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation($"User {farmerUser.Email} added to Farmer role.");
                        TempData["SuccessMessage"] = "Farmer account created successfully!";
                        return RedirectToAction("Index"); // Redirect to employee dashboard.
                    }
                    else
                    {
                        // If adding to role fails, it's important to handle this.
                        // Here, we log errors and delete the created user to prevent an orphaned user account
                        // (a user existing without their intended role).
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        _logger.LogWarning($"Could not add user {farmerUser.Email} to Farmer role. Deleting user.");
                        await _userManager.DeleteAsync(farmerUser); // Clean up by deleting the user.
                        _logger.LogWarning($"User {farmerUser.Email} deleted due to failure in role assignment.");
                    }
                }
                else
                {
                    // If user creation fails (password too weak, duplicate email/username),
                    // add the errors to ModelState so they can be displayed on the form.
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // If ModelState is not valid, or if any operation failed and added errors to ModelState,
            // return the view with the submitted model to display errors and allow the user to correct them.
            return View(model);
        }

        // GET: /Employee/ViewAllProducts
        // Displays all products from all farmers, with options for filtering.
        [HttpGet]
        public async Task<IActionResult> ViewAllProducts(string? selectedFarmerId, string? selectedProductType, DateTime? selectedStartDate, DateTime? selectedEndDate)
        {
            _logger.LogInformation("Employee accessing ViewAllProducts page. Filters: FarmerId={FarmerId}, Type={ProductType}, Start={StartDate}, End={EndDate}",
                selectedFarmerId, selectedProductType, selectedStartDate, selectedEndDate);

            try
            {
                var products = await _productService.GetFilteredProductsAsync(
                    selectedFarmerId,
                    selectedProductType,
                    selectedStartDate,
                    selectedEndDate);

                // To populate the farmer filter dropdown, get all users currently in the "Farmer" role.
                // UserManager's GetUsersInRoleAsync is useful here.
                // Overview of UserManager: https://dotnettutorials.net/lesson/usermanager-signinmanager-rolemanager-in-asp-net-core-identity/
                var farmers = await _userManager.GetUsersInRoleAsync("Farmer");
                var categories = await _productService.GetDistinctProductCategoriesAsync();

                var viewModel = new ViewAllProductsViewModel
                {
                    Products = products,
                    Farmers = new SelectList(farmers.OrderBy(f => f.FullName), "Id", "FullName", selectedFarmerId),
                    Categories = new SelectList(categories, selectedProductType),
                    SelectedFarmerId = selectedFarmerId,
                    SelectedProductType = selectedProductType,
                    SelectedStartDate = selectedStartDate,
                    SelectedEndDate = selectedEndDate
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching products for ViewAllProducts page.");
                TempData["ErrorMessage"] = "An error occurred while loading product data. Please try again.";
                return RedirectToAction("Index");
            }
        }
    }
}