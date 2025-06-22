using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROG7311_WebApp.Models;
using PROG7311_WebApp.Services;
using System;
using System.Threading.Tasks;

namespace PROG7311_WebApp.Controllers
{
    [Authorize(Roles = "Farmer")]
    public class FarmerController : Controller
    {
        // This controller also now uses our custom IUserService instead of UserManager directly.
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly ILogger<FarmerController> _logger;

        public FarmerController(IUserService userService, IProductService productService, ILogger<FarmerController> logger)
        {
            _userService = userService;
            _productService = productService;
            _logger = logger;
        }

        // Serves as the main landing page for logged-in farmers.
        public IActionResult Index()
        {
            ViewData["WelcomeMessage"] = "Welcome to your Farmer Dashboard!";
            return View();
        }

        // Displays the form for a farmer to add a new product.
        [HttpGet]
        public IActionResult AddProduct()
        {
            return View(new AddProductViewModel());
        }

        // Processes the submitted new product form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(AddProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Getting the current user is now a clean call to our service layer.
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                {
                    _logger.LogWarning("Authenticated user could not be found. This is unexpected.");
                    return Challenge(); // Forces re-authentication if something is wrong.
                }

                var product = new Product
                {
                    Name = model.Name,
                    Category = model.Category,
                    ProductionDate = model.ProductionDate,
                    FarmerId = currentUser.Id // Link the product to the current farmer.
                };

                try
                {
                    await _productService.AddProductAsync(product);
                    TempData["SuccessMessage"] = "Product added successfully!";
                    return RedirectToAction("MyProducts");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error adding product for farmer '{currentUser.Email}'.");
                    ModelState.AddModelError(string.Empty, "An error occurred while adding the product.");
                }
            }
            return View(model);
        }

        // Displays a list of products that were added by the currently logged-in farmer.
        [HttpGet]
        public async Task<IActionResult> MyProducts()
        {
            // Just like in AddProduct, we get the current user via the service.
            var currentUser = await _userService.GetCurrentUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("Authenticated user could not be found when trying to view products.");
                return Challenge();
            }

            var products = await _productService.GetProductsByFarmerIdAsync(currentUser.Id);

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            return View(products);
        }
    }
}