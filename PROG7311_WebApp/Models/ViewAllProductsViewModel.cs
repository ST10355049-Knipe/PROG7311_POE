using Microsoft.AspNetCore.Mvc.Rendering; // Required for SelectList
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PROG7311_WebApp.Models
{
    public class ViewAllProductsViewModel
    {
        // Data to display
        public IEnumerable<Product> Products { get; set; } = new List<Product>();

        // Data for filter dropdowns
        public SelectList? Farmers { get; set; } // List of all farmers for filtering
        public SelectList? Categories { get; set; } // List of distinct product categories

        // Current filter values 
        [Display(Name = "Farmer")]
        public string? SelectedFarmerId { get; set; }

        [Display(Name = "Product Type")]
        public string? SelectedProductType { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? SelectedStartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? SelectedEndDate { get; set; }
    }
}