// Data/AppDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PROG7311_WebApp.Models;

namespace PROG7311_WebApp.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet for Products will be added in the next step
        public DbSet<Product> Products { get; set; } // Added DbSet for Products
        // ApplicationUser is handled by IdentityDbContext

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ProductionDate).IsRequired();

                // Define the relationship between ApplicationUser (Farmer) and Product
                // An ApplicationUser (Farmer) can have many Products
                // Each Product belongs to one ApplicationUser (Farmer)
                entity.HasOne(d => d.Farmer)
                      .WithMany(p => p.Products) 
                      .HasForeignKey(d => d.FarmerId)
                      .OnDelete(DeleteBehavior.Cascade); // If a farmer is deleted, their products are deleted
            });
        }
    }
}