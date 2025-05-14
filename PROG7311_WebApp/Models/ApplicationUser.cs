using Microsoft.AspNetCore.Identity;
using System.Collections.Generic; 

namespace PROG7311_WebApp.Models
{
    // This class extends the built-in IdentityUser to add application-specific properties.
    // Extending IdentityUser is a common way to customise user profiles.
    // Reference: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/add-user-data
    public class ApplicationUser : IdentityUser // Inherit from IdentityUser
    {
        // [PersonalData] attribute can be used to flag properties containing personal data,
        // relevant for data management features if scaffolded Identity UI is used.
        // Reference: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/manage-user-data
        [PersonalData] //  indicates it's personal data
        public string? FullName { get; set; }

        // Navigation property: A farmer can have a collection of products.
        // This defines the "one" side of a one-to-many relationship with the Product entity.
        // EF Core uses this for loading related data.
        // Concept of navigation properties: https://learn.microsoft.com/en-us/ef/core/modeling/relationships
        public virtual ICollection<Product>? Products { get; set; }
    }
}