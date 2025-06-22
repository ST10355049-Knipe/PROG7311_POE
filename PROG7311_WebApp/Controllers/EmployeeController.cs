using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROG7311_WebApp.Models;
using PROG7311_WebApp.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PROG7311_WebApp.Controllers
{
    // This attribute ensures only users in the "Employee" role can access any action here.
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        // Following the feedback, this controller no longer uses UserManager directly.
        // It uses our custom IUserService for user-related tasks and IProductService for product tasks.
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IUserService userService, IProductService productService, ILogger<EmployeeController> logger)
        {
            _userService = userService;
            _productService = productService;
            _logger = logger;
        }

        // Displays the main dashboard for employees.
        public IActionResult Index()
        {
            return View();
        }

        // Displays the form for adding a new farmer.
        [HttpGet]
        public IActionResult AddFarmer()
        {
            return View(new AddFarmerViewModel());
        }

        // Handles the form submission for creating a new farmer.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFarmer(AddFarmerViewModel model)
        {
            if (ModelState.IsValid)
            {
                // This action is now much simpler. The complex logic of creating a user AND
                // assigning them a role is handled in a single call to our service.
                var result = await _userService.CreateFarmerAsync(model);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Farmer account for {model.Email} created successfully by employee {User.Identity?.Name}.");
                    TempData["SuccessMessage"] = "Farmer account created successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    // If the service indicates failure, we add the errors to our model state
                    // so the user can see what went wrong on the form.
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        // Displays all products with filtering options.
        [HttpGet]
        public async Task<IActionResult> ViewAllProducts(string? selectedFarmerId, string? selectedProductType, DateTime? selectedStartDate, DateTime? selectedEndDate)
        {
            _logger.LogInformation("Employee accessing ViewAllProducts page.");
            try
            {
                // The controller's job is to get the data from services...
                var products = await _productService.GetFilteredProductsAsync(selectedFarmerId, selectedProductType, selectedStartDate, selectedEndDate);
                var farmers = await _userService.GetUsersInRoleAsync("Farmer"); // ...using our new UserService
                var categories = await _productService.GetDistinctProductCategoriesAsync();

                //  prepare the ViewModel for the view.
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
                TempData["ErrorMessage"] = "An error occurred while loading product data.";
                return RedirectToAction("Index");
            }
        }
    }
}