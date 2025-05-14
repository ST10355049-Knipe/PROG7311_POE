using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROG7311_WebApp.Models
{
    // Represents a product offered by a farmer in the system.
    // This is an entity class that will be mapped to a "Products" table in the database by EF Core.
    // General EF Core modeling: https://learn.microsoft.com/en-us/ef/core/modeling/
    public class Product
    {
        // Primary Key for the Products table, auto-incremented by the database.
        public int Id { get; set; }

        // DataAnnotations are used here for simple validation rules enforced by ASP.NET Core MVC
        // both client-side and server-side.
        // Reference: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product category is required.")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Production date is required.")]
        [DataType(DataType.Date)] // Helps with UI rendering (e.g., date picker) and model binding.
        [Display(Name = "Production Date")] // How the property name should be displayed in UI labels.
        public DateTime ProductionDate { get; set; }

        // Foreign Key to link this Product to an ApplicationUser (the Farmer).
        // This establishes the "many" side of the one-to-many relationship (one Farmer has many Products).
        // Reference: https://learn.microsoft.com/en-us/ef/core/modeling/relationships#one-to-many
        [Required]
        public string FarmerId { get; set; } = string.Empty; // String because ApplicationUser (IdentityUser) uses string IDs.

        // Navigation property to the Farmer (ApplicationUser) who owns this product.
        // The [ForeignKey("FarmerId")] attribute specifies which property holds the foreign key value.
        // This allows EF Core to load the related Farmer object (using .Include()).
        [ForeignKey("FarmerId")]
        public virtual ApplicationUser? Farmer { get; set; }
    }
}