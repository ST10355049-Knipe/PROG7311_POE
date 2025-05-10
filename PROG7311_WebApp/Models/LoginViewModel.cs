using System.ComponentModel.DataAnnotations; 

namespace PROG7311_WebApp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email address is required.")] // Basic validation
        [EmailAddress(ErrorMessage = "Invalid email address format.")] // Ensures it's a valid email format
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)] // Tells ASP.NET Core to render this as a password input
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")] // Label for the checkbox in the view
        public bool RememberMe { get; set; }
    }
}