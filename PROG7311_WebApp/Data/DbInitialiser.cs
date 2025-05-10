using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PROG7311_WebApp.Models;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;


namespace PROG7311_WebApp.Data
{
    public static class DbInitialiser
    {
        public static async Task Initialse(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            string[] roleNames = { "Employee", "Farmer" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

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

                var result = await userManager.CreateAsync(employeeUser, "Password.1");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(employeeUser, "Employee");
                }
                else
                {
                    Console.WriteLine($"Error creating employee user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }


            }
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
