using Microsoft.AspNetCore.Authorization; // Required for the [Authorize] attribute
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROG7311_WebApp.Models;
using PROG7311_WebApp.Services;
using System.Threading.Tasks;

namespace PROG7311_WebApp.Controllers
{
    [Authorize(Roles = "Employee")] // Ensures only "Employee" role can access this controller
    public class EmployeeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EmployeeController> _logger;
        private readonly IProductService _productService; 

        // Constructor to inject UserManager and ILogger
        public EmployeeController(UserManager<ApplicationUser> userManager, ILogger<EmployeeController> logger, IProductService productService)
        {
            _userManager = userManager;
            _logger = logger;
            _productService = productService;
        }

        
        // A simple dashboard for employees.
        public IActionResult Index()
        {
            
            return View();
        }

        // Displays the form to add a new farmer.
        [HttpGet]
        public IActionResult AddFarmer()
        {
            return View(new AddFarmerViewModel()); // Pass an empty view model to the view
        }

        
        // Handles the submission of the new farmer form.
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against CSRF attacks
        public async Task<IActionResult> AddFarmer(AddFarmerViewModel model)
        {
            if (ModelState.IsValid) // Check if the submitted data meets validation rules in the ViewModel
            {
                var farmerUser = new ApplicationUser
                {
                    UserName = model.Email, 
                    Email = model.Email,
                    FullName = model.FullName,
                    EmailConfirmed = true 
                                          
                };

                // Create the new user with the provided password
                var result = await _userManager.CreateAsync(farmerUser, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Farmer account for {model.Email} created successfully by employee {User.Identity?.Name}.");

                    // If user creation is successful, add the user to the "Farmer" role.
                    var roleResult = await _userManager.AddToRoleAsync(farmerUser, "Farmer");
                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation($"User {farmerUser.Email} added to Farmer role.");
                        TempData["SuccessMessage"] = "Farmer account created successfully!"; // Message for the user
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // Handle errors adding user to role
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        _logger.LogWarning($"Could not add user {farmerUser.Email} to Farmer role.");
                        await _userManager.DeleteAsync(farmerUser);
                        _logger.LogWarning($"User {farmerUser.Email} deleted due to failure in role assignment.");
                    }
                }
                else
                {
                    // If user creation fails, add errors to ModelState to display them in the view.
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // If ModelState is invalid, or if there were errors, return the view with the model to display errors.
            return View(model);
        }

        // Displays all products with filtering options.
        [HttpGet]
        public async Task<IActionResult> ViewAllProducts(string? selectedFarmerId, string? selectedProductType, DateTime? selectedStartDate, DateTime? selectedEndDate)
        {
            _logger.LogInformation("Employee accessing ViewAllProducts page. Filters: FarmerId={FarmerId}, Type={ProductType}, Start={StartDate}, End={EndDate}",
                selectedFarmerId, selectedProductType, selectedStartDate, selectedEndDate);

            try
            {
                // Get products based on filters using the service
                var products = await _productService.GetFilteredProductsAsync(
                    selectedFarmerId,
                    selectedProductType,
                    selectedStartDate,
                    selectedEndDate);

                // Get data for filter dropdowns
                var farmers = await _userManager.GetUsersInRoleAsync("Farmer");
                var categories = await _productService.GetDistinctProductCategoriesAsync();

                var viewModel = new ViewAllProductsViewModel
                {
                    Products = products,
                    // Create SelectList for farmers dropdown. Value is Farmer's ID, Text is Farmer's FullName.
                    Farmers = new SelectList(farmers.OrderBy(f => f.FullName), "Id", "FullName", selectedFarmerId),
                    // Create SelectList for categories dropdown. Value and Text are both the category name.
                    Categories = new SelectList(categories, selectedProductType),

                    // Pass current filter values back to the view to repopulate the form
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
                // It's good practice to have an error view or handle this gracefully.
                // For now, returning a simple error message or redirecting.
                TempData["ErrorMessage"] = "An error occurred while loading product data. Please try again.";
                return RedirectToAction("Index"); // Redirect to employee dashboard on error
            }
        }
    }
}
