
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic; 

namespace PROG7311_WebApp.Models
{
    public class ApplicationUser : IdentityUser // Inherit from IdentityUser
    {
        [PersonalData] //  indicates it's personal data
        public string? FullName { get; set; }

        // Navigation property for products (if the user is a farmer)
        public virtual ICollection<Product>? Products { get; set; }
    }
}