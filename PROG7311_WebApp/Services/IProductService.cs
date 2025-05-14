using PROG7311_WebApp.Models;
using System.Collections.Generic; // Needed for ICollection<T>
using System.Threading.Tasks; // Needed for async operations

namespace PROG7311_WebApp.Services
{
    // Defines the contract for product-related operations.
    // Controllers will depend on this interface, not the concrete ProductService class directly.
    // Reference: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
    public interface IProductService
    {
        // Gets all products listed by a specific farmer.
        // Takes farmerId as input and returns a collection of their products.
        Task<IEnumerable<Product>> GetProductsByFarmerIdAsync(string farmerId); // Fetch all products

        // Adds a new product to the system.
        // Takes a Product object as input.
        Task AddProductAsync(Product product); // Add a new product

        // Retrieves a list of products based on various filter criteria.
        // All filter parameters are optional.
        // Used by employees to view and search through all products.
        Task<IEnumerable<Product>> GetFilteredProductsAsync(
            string? selectedFarmerId = null,
            string? productType = null, 
            DateTime? startDate = null,
            DateTime? endDate = null); // Fetch products based on filters

        // Retrieves a list of all unique product categories currently in the system.
        Task<IEnumerable<string>> GetDistinctProductCategoriesAsync(); // Fetch distinct product categories
    }
}
