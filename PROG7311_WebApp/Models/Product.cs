using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROG7311_WebApp.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Production Date")]
        public DateTime ProductionDate { get; set; }

        // Foreign Key to link Product to a Farmer (ApplicationUser)
        [Required]
        public string FarmerId { get; set; } = string.Empty; // IdentityUser uses string IDs

        [ForeignKey("FarmerId")]
        public virtual ApplicationUser? Farmer { get; set; } // Navigation property
    }
}