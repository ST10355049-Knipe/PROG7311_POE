using System;
using System.ComponentModel.DataAnnotations; // Required for DataAnnotations

namespace PROG7311_WebApp.Models
{
    public class AddProductViewModel
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product category is required.")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Production date is required.")]
        [DataType(DataType.Date)] // Ensures a date picker is typically shown in the UI
        [Display(Name = "Production Date")]
        public DateTime ProductionDate { get; set; } = DateTime.Today; // Default to today's date
    }
}
