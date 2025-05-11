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
    }
}
