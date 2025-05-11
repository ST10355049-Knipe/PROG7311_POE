using PROG7311_WebApp.Models;
using System.Collections.Generic; // Needed for ICollection<T>
using System.Threading.Tasks; // Needed for async operations

namespace PROG7311_WebApp.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProductsByFarmerIdAsync(string farmerId); // Fetch all products
        //Task<Product?> GetProductByIdAsync(int productId); // Fetch a product by its ID
        Task AddProductAsync(Product product); // Add a new product
        //Task UpdateProductAsync(Product product); // Update an existing product
       // Task DeleteProductAsync(int id); // Delete a product by its ID
        //Task <IEnumerable<Product>> GetAllProductsAsync(); // Fetch all products for employee view
    }
}
