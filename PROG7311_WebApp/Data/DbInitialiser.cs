using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PROG7311_WebApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace PROG7311_WebApp.Data
{
    public static class DbInitialiser
    {
        // This method is called from Program.cs to set up initial data.
        // It ensures the database is created and seeds roles, users, and sample products.
        public static async Task Initialse(IServiceProvider serviceProvider)
        {
            // Get the necessary services from the dependency injection container.
            // This is a common way to access services during application startup.
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure the database is created and all migrations are applied.
            // This is important for making sure the schema is up-to-date before trying to seed data.
            // Idea for using MigrateAsync() in startup seeding from:
            // https://medium.com/@roshanj100/users-and-roles-seeding-in-asp-net-core-identity-with-entity-framework-core-a-step-by-step-guide-28e6f76a18db
            await context.Database.MigrateAsync();

            // Define the roles we need in the application.
            string[] roleNames = { "Employee", "Farmer" };
            foreach (var roleName in roleNames)
            {
                // Check if the role already exists to avoid errors.
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    //Create the role if it doesn't exist.
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed an initial Employee user.
            // This user will be able to log in and manage farmer accounts.
            var employeeEmail = "employee1@agrienergy.com";
            var employeeUser = await userManager.FindByEmailAsync(employeeEmail);

            if (employeeUser == null)
            {
                employeeUser = new ApplicationUser
                {
                    UserName = employeeEmail,
                    Email = employeeEmail,
                    FullName = "Employee One",
                    EmailConfirmed = true
                };

                // Create the user with a default password.
                // UserManager handles password hashing automatically.
                var result = await userManager.CreateAsync(employeeUser, "Password.1");
                if (result.Succeeded)
                {
                    // If user creation was successful, add them to the "Employee" role.
                    await userManager.AddToRoleAsync(employeeUser, "Employee");
                }
                else
                {
                    // Log errors if user creation fails. Helps in debugging setup issues.
                    Console.WriteLine($"Error creating employee user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }


            }
            // Seed an initial Farmer user.
            // This demonstrates a farmer account and allows testing of farmer functionalities.
            string farmer1Id = string.Empty; // To store the ID for product seeding
            var farmerEmailForSeeding = "farmer1@agrifarm.com";
            var farmerUser = await userManager.FindByEmailAsync(farmerEmailForSeeding);

            if (farmerUser == null)
            {
                farmerUser = new ApplicationUser
                {
                    UserName = farmerEmailForSeeding,
                    Email = farmerEmailForSeeding,
                    FullName = "Farmer One",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(farmerUser, "Password.1");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(farmerUser, "Farmer");
                    farmer1Id = farmerUser.Id; // Store the farmer ID for later use
                }
                else
                {
                    Console.WriteLine($"Error creating farmer user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                farmer1Id = farmerUser.Id; // Get the existing farmer ID
            }

            // Seed initial product data for the farmer.
            // This follows the principle of data seeding described in EF Core documentation:
            // https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding
            // We only seed products if the farmer was successfully created/found and if no products exist yet (to avoid duplicates on multiple runs).
            if (!string.IsNullOrEmpty(farmer1Id) && !context.Products.Any())
            {
                context.Products.AddRange(
                    new Product
                    {
                        Name = "Tomatoes",
                        Category = "Vegetables",
                        ProductionDate = new DateTime(2025, 9, 15),
                        FarmerId = farmer1Id
                    },
                    new Product
                    {
                        Name = "Apples",
                        Category = "Fruits",
                        ProductionDate = new DateTime(2025, 10, 1),
                        FarmerId = farmer1Id
                    },

                    new Product
                    {
                        Name = "Local Honey",
                        Category = "Other",
                        ProductionDate = new DateTime(2025, 6, 4),
                        FarmerId = farmer1Id
                    }

                );
                await context.SaveChangesAsync();
            }
        }
    }
}
