using Microsoft.AspNetCore.Authorization; // For [Authorize]
using Microsoft.AspNetCore.Identity;      // For UserManager
using Microsoft.AspNetCore.Mvc;
using PROG7311_WebApp.Models;             // For ApplicationUser, Product, AddProductViewModel
using PROG7311_WebApp.Services;           // For IProductService
using System.Threading.Tasks;

namespace PROG7311_WebApp.Controllers
{
    [Authorize(Roles = "Farmer")] // Restrict access to users in the "Farmer" role
    public class FarmerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductService _productService; // Using our service
        private readonly ILogger<FarmerController> _logger;

        public FarmerController(UserManager<ApplicationUser> userManager,
                                IProductService productService,
                                ILogger<FarmerController> logger)
        {
            _userManager = userManager;
            _productService = productService;
            _logger = logger;
        }

        // GET: /Farmer/ or /Farmer/Index
        // This will be the farmer's main dashboard page.
        public IActionResult Index()
        {
            
            ViewData["WelcomeMessage"] = "Welcome to your Farmer Dashboard!";
            return View();
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            return View(new AddProductViewModel()); // Pass an empty view model to the form
        }

        // POST: /Farmer/AddProduct
        // Handles the submission of the new product form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(AddProductViewModel model)
        {
            if (ModelState.IsValid) // Check if the submitted data is valid based on ViewModel annotations
            {
                // Get the ID of the currently logged-in farmer
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    _logger.LogWarning("User not found while trying to add a product. This could indicate an issue.");
                    return Challenge(); // Or redirect to login
                }

                var product = new Product
                {
                    Name = model.Name,
                    Category = model.Category,
                    ProductionDate = model.ProductionDate,
                    FarmerId = currentUser.Id // Associate the product with the logged-in farmer
                };

                try
                {
                    await _productService.AddProductAsync(product); // Use the service to add the product
                    _logger.LogInformation($"Product '{product.Name}' added by farmer '{currentUser.Email}'.");
                    TempData["SuccessMessage"] = "Product added successfully!";
                    return RedirectToAction("MyProducts"); // Redirect to the list of their products
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error adding product for farmer '{currentUser.Email}'.");
                    ModelState.AddModelError(string.Empty, "An error occurred while adding the product. Please try again.");
                    
                }
            }

            // If ModelState is invalid, return the view with the model to display validation errors
            return View(model);
        }
    }
}
    
