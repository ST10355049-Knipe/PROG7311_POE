using Microsoft.EntityFrameworkCore; // Required for ToListAsync, Include, AsQueryable, etc.
using PROG7311_WebApp.Data;      // Required for AppDbContext
using PROG7311_WebApp.Models;    // Required for Product
using System;                     // Required for DateTime, ArgumentNullException
using System.Collections.Generic;
using System.Linq;                // Required for LINQ methods like Where, OrderBy, Select, Distinct
using System.Threading.Tasks;     // Required for async operations

namespace PROG7311_WebApp.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductService> _logger; // For logging potential errors or info.

        // Constructor: AppDbContext and ILogger are injected by the dependency injection system.
        // This allows the service to interact with the database and log messages.
        public ProductService(AppDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Retrieves all products associated with a specific farmer ID.
        public async Task<IEnumerable<Product>> GetProductsByFarmerIdAsync(string farmerId)
        {
            if (string.IsNullOrEmpty(farmerId))
            {
                _logger.LogWarning("GetProductsByFarmerIdAsync called with null or empty farmerId.");
                return Enumerable.Empty<Product>(); // Return empty if no ID provided.
            }

            try
            {
                // Using LINQ's Where clause to filter products by FarmerId.
                // OrderByDescending is used to show the most recent products first.
                // ToListAsync executes the query asynchronously.
                // General querying concepts: https://learn.microsoft.com/en-us/ef/core/querying/
                // Async LINQ usage: https://www.bytehide.com/blog/linq-csharp (section on async)
                return await _context.Products
                                     .Where(p => p.FarmerId == farmerId)
                                     .OrderByDescending(p => p.ProductionDate)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving products for farmer ID {farmerId}");
                return Enumerable.Empty<Product>(); // Return empty list on error to prevent crashes.
            }
        }

        // Adds a new product to the database.
        public async Task AddProductAsync(Product product)
        {
            if (product == null)
            {
                // Basic validation to ensure a product object is provided.
                throw new ArgumentNullException(nameof(product));
            }

            try
            {
                // Add the product to the DbContext's tracking.
                _context.Products.Add(product);
                // SaveChangesAsync persists the new product to the database.
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Product '{product.Name}' added successfully for farmer ID {product.FarmerId}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding product '{product.Name}' for farmer ID {product.FarmerId}");
                throw; // Rethrow the exception to be handled by the caller (e.g., controller).
            }
        }

        // Retrieves products based on a set of optional filter criteria.
        // This method demonstrates dynamic query building with LINQ and EF Core.
        public async Task<IEnumerable<Product>> GetFilteredProductsAsync(
            string? selectedFarmerId = null,
            string? productType = null,      // This is the category
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                // Start with the full set of products from the database.
                // .Include(p => p.Farmer) performs eager loading to fetch related Farmer data with each Product.
                // This helps avoid N+1 query problems when accessing Farmer.FullName in the view.
                // Ref for Eager Loading: https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager
                // .AsQueryable() allows us to build the query step-by-step by appending .Where() clauses.
                var query = _context.Products.Include(p => p.Farmer).AsQueryable();

                // Conditionally apply the farmer filter if a selectedFarmerId is provided.
                // This uses a lambda expression `p => p.FarmerId == selectedFarmerId` as the predicate.
                // General LINQ .Where usage: https://learn.microsoft.com/en-us/ef/core/querying/
                if (!string.IsNullOrEmpty(selectedFarmerId))
                {
                    query = query.Where(p => p.FarmerId == selectedFarmerId);
                }

                // Prepare the productType for case-insensitive comparison by converting to lowercase.
                // Also trim to handle potential leading/trailing spaces from input.
                var trimmedProductType = productType?.Trim().ToLower();

                // Conditionally apply the product type (category) filter.
                if (!string.IsNullOrEmpty(trimmedProductType))
                {
                    // Compare categories by converting both database value and filter value to lowercase.
                    // This ensures a case-insensitive match.
                    // The null check `p.Category != null` is a defensive measure.
                    query = query.Where(p => p.Category != null && p.Category.ToLower() == trimmedProductType);
                }

                // Conditionally apply the start date filter.
                if (startDate.HasValue)
                {
                    // Compare only the Date part, ignoring time.
                    query = query.Where(p => p.ProductionDate >= startDate.Value.Date);
                }

                // Conditionally apply the end date filter.
                if (endDate.HasValue)
                {
                    // To make the endDate inclusive for the whole day, we check if ProductionDate is less than the day AFTER the endDate.
                    query = query.Where(p => p.ProductionDate < endDate.Value.Date.AddDays(1));
                }

                // Order the results for consistent presentation.
                // First by farmer's full name (if available), then by product name.
                // Using async execution with ToListAsync.
                // Overview of async LINQ: https://www.bytehide.com/blog/linq-csharp
                return await query.OrderBy(p => p.Farmer != null ? p.Farmer.FullName : string.Empty)
                                  .ThenBy(p => p.Name)
                                  .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filtered products.");
                return Enumerable.Empty<Product>(); // Gracefully return empty list on error.
            }
        }

        // Retrieves a list of unique product categories from the database.
        // Used to populate filter dropdowns.
        public async Task<IEnumerable<string>> GetDistinctProductCategoriesAsync()
        {
            try
            {
                // .Select(p => p.Category) projects only the Category column.
                // .Distinct() ensures each category appears only once.
                // .OrderBy() sorts the categories alphabetically.
                return await _context.Products
                                     .Select(p => p.Category)
                                     .Distinct()
                                     .OrderBy(category => category)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving distinct product categories.");
                return Enumerable.Empty<string>();
            }
        }
    }
}