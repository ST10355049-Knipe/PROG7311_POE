using Microsoft.AspNetCore.Authorization; // Required for the [Authorize] attribute
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PROG7311_WebApp.Models; 
using System.Threading.Tasks;

namespace PROG7311_WebApp.Controllers
{
    [Authorize(Roles = "Employee")] // Ensures only "Employee" role can access this controller
    public class EmployeeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EmployeeController> _logger;

        // Constructor to inject UserManager and ILogger
        public EmployeeController(UserManager<ApplicationUser> userManager, ILogger<EmployeeController> logger)
        {
            _userManager = userManager;
            _logger = logger;
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
    }
}
