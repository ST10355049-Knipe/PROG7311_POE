using Microsoft.EntityFrameworkCore; // Required for ToListAsync, Include
using PROG7311_WebApp.Data;      // Required for AppDbContext
using PROG7311_WebApp.Models;    // Required for Product
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROG7311_WebApp.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductService> _logger;

        // Constructor to inject AppDbContext and ILogger
        public ProductService(AppDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetProductsByFarmerIdAsync(string farmerId)
        {
            // Retrieve products from the database where the FarmerId matches.
            try
            {
                return await _context.Products
                                     .Where(p => p.FarmerId == farmerId)
                                     .OrderByDescending(p => p.ProductionDate) 
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving products for farmer ID {farmerId}");
                return Enumerable.Empty<Product>();
            }
        }

        public async Task AddProductAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            // Add the new product to the DbContext and save changes.
            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Product '{product.Name}' added successfully for farmer ID {product.FarmerId}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding product '{product.Name}' for farmer ID {product.FarmerId}");
                // Rethrow or handle as per application's error handling strategy.
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetFilteredProductsAsync(
            string? selectedFarmerId = null,
            string? productType = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                // Using AsQueryable() allows me to build the query dynamically before execution
                var query = _context.Products.Include(p => p.Farmer).AsQueryable();

                // Apply filters conditionally
                if (!string.IsNullOrEmpty(selectedFarmerId))
                {
                    query = query.Where(p => p.FarmerId == selectedFarmerId);
                }

                var trimmedProductType = productType?.Trim().ToLower();

                if (!string.IsNullOrEmpty(trimmedProductType))
                {
                    query = query.Where(p => p.Category != null && p.Category.ToLower() == trimmedProductType);
                }

                if (startDate.HasValue)
                {
                    // Ensure the product's production date is on or after the startDate
                    query = query.Where(p => p.ProductionDate >= startDate.Value.Date);
                }

                if (endDate.HasValue)
                {
                    // Ensure the product's production date is on or before the endDate
                    // Adding .AddDays(1).Date makes the endDate inclusive for the whole day
                    query = query.Where(p => p.ProductionDate < endDate.Value.Date.AddDays(1));
                }

                // Order the results
                return await query.OrderBy(p => p.Farmer != null ? p.Farmer.FullName : string.Empty)
                                  .ThenBy(p => p.Name)
                                  .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filtered products.");
                return Enumerable.Empty<Product>();
            }
        }

        //get distinct categories
        public async Task<IEnumerable<string>> GetDistinctProductCategoriesAsync()
        {
            try
            {
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
