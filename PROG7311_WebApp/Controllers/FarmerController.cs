using Microsoft.AspNetCore.Authorization; // For [Authorize] attribute.
using Microsoft.AspNetCore.Identity;      // Core namespace for ASP.NET Core Identity services like UserManager.
using Microsoft.AspNetCore.Mvc;
using PROG7311_WebApp.Models;             // For ApplicationUser, Product, AddProductViewModel.
using PROG7311_WebApp.Services;           // For IProductService.
using System;                              // For Exception (though not explicitly used in catch blocks yet).
using System.Threading.Tasks;             // For asynchronous operations.

namespace PROG7311_WebApp.Controllers
{
    // [Authorize(Roles = "Farmer")] ensures only authenticated users with the "Farmer" role
    // can access actions within this controller. This is a fundamental part of securing
    // role-specific functionalities.
    // Introduction to ASP.NET Core Identity: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-9.0&tabs=visual-studio
    [Authorize(Roles = "Farmer")]
    public class FarmerController : Controller
    {
        // Injected services for user management, product logic, and logging.
        // UserManager is central for Identity operations.
        // For an overview of UserManager and other Identity services: https://dotnettutorials.net/lesson/usermanager-signinmanager-rolemanager-in-asp-net-core-identity/
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductService _productService;
        private readonly ILogger<FarmerController> _logger;

        // Constructor for dependency injection.
        // The ASP.NET Core framework provides instances of these services.
        public FarmerController(UserManager<ApplicationUser> userManager,
                                IProductService productService,
                                ILogger<FarmerController> logger)
        {
            _userManager = userManager;
            _productService = productService;
            _logger = logger;
        }

        // GET: /Farmer/Index
        // Serves as the main landing page or dashboard for logged-in farmers.
        public IActionResult Index()
        {
            ViewData["WelcomeMessage"] = "Welcome to your Farmer Dashboard!";
            return View();
        }

        // GET: /Farmer/AddProduct
        // Displays the form for a farmer to add a new product to their offerings.
        [HttpGet]
        public IActionResult AddProduct()
        {
            // Provides an empty AddProductViewModel to the view for form binding.
            return View(new AddProductViewModel());
        }

        // POST: /Farmer/AddProduct
        // Processes the submitted new product form.
        // [ValidateAntiForgeryToken] helps mitigate CSRF attack risks.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(AddProductViewModel model)
        {
            // Ensures the submitted data passes validation rules from the ViewModel.
            if (ModelState.IsValid)
            {
                // Retrieve the currently authenticated farmer to link the product to them.
                // User.Identity (via the User property in Controller) provides the current user's claims principal.
                // _userManager.GetUserAsync(User) fetches the full ApplicationUser object.
                // Understanding UserManager: https://dotnettutorials.net/lesson/usermanager-signinmanager-rolemanager-in-asp-net-core-identity/
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    // This is a safeguard; [Authorize] should prevent unauthenticated access.
                    _logger.LogWarning("User not found (null) while trying to add a product by an authenticated farmer. This indicates an issue.");
                    return Challenge(); // Or redirect to login with an error.
                }

                var product = new Product
                {
                    Name = model.Name,
                    Category = model.Category,
                    ProductionDate = model.ProductionDate,
                    FarmerId = currentUser.Id // associate this product with the logged-in farmer.
                };

                try
                {
                    await _productService.AddProductAsync(product);
                    _logger.LogInformation($"Product '{product.Name}' added by farmer '{currentUser.Email}'.");
                    TempData["SuccessMessage"] = "Product added successfully!";
                    return RedirectToAction("MyProducts"); // Show the farmer their updated list of products.
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error adding product '{product.Name}' for farmer '{currentUser.Email}'.");
                    ModelState.AddModelError(string.Empty, "An error occurred while adding the product. Please try again.");
                }
            }

            // If model state is invalid, return the view with the model to display errors.
            return View(model);
        }

        // GET: /Farmer/MyProducts
        // Displays a list of products that were added by the currently logged-in farmer.
        [HttpGet]
        public async Task<IActionResult> MyProducts()
        {
            // Get the ApplicationUser object for the currently logged-in user.
            // This is essential to fetch only their products.
            // General Identity concepts and UserManager usage:
            // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-9.0&tabs=visual-studio
            // https://www.youtube.com/watch?v=TfarnVqnhX0&ab_channel=kudvenkat (Concept of Identity services)
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("User not found (null) while trying to view their products. This should not happen for an authorized user.");
                return Challenge();
            }

            var products = await _productService.GetProductsByFarmerIdAsync(currentUser.Id);

            // Pass any success message (e.g., from AddProduct redirect) to the view.
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            return View(products);
        }
    }
}